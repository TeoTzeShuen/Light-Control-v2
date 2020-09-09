using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WifiLedController
{
    internal class WifiLed : IEquatable<WifiLed>
    {
        public IPAddress iPAddress { get; }
        public string macAddress { get; }
        public string name { get; set; }

        private static int TcpPort = 5577;
        private TcpClient Sender = new TcpClient();

        public byte Red { get; set; } = 0;
        public byte Green { get; set; } = 0;
        public byte Blue { get; set; } = 0;
        public byte WarmWhite { get; set; } = 0;
        public bool Active { get; set; } = false;
        public byte CurrentFunction { get; set; } = 0x61;
        public byte FunctionSpeed { get; set; } = 0;

        private int request = 0;

        private DateTime lastNetworkActivity;

        /* Creates a new light object
         * discoIP is the IP it was found on
         * macaddress is its macaddress. Assuming these are unique for now.
         */

        public WifiLed(IPAddress discoIP, String macAddress)
        {
            iPAddress = discoIP;

            this.macAddress = macAddress;
            name = macAddress;
            Sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            lastNetworkActivity = DateTime.Now;
        }

        //Turn the light on. Whatever setting are stored on the light at last use will be restored
        public virtual void TurnOn()
        {
            Debug.WriteLine("({0})[TurnOn]", name);
            Active = true;
            byte[] onPackage = new Byte[4];
            onPackage[0] = 0x71;
            onPackage[1] = 0x23;
            onPackage[2] = 0x0F;
            onPackage[3] = CalculateCRC(onPackage);

            SendPackage(onPackage, 0);
        }

        //Turns light off. Can be safely sent when light is already off.
        public virtual void TurnOff()
        {
            Active = false;
            byte[] offPackage = new Byte[4];
            offPackage[0] = 0x71;
            offPackage[1] = 0x24;
            offPackage[2] = 0x0F;
            offPackage[3] = CalculateCRC(offPackage);

            SendPackage(offPackage, 0);
        }

        /* This function changes the rgb and white values of the connected light
         * Values are supplied in byte[0-255] values are self describing
         */

        public virtual void UpdateRGBWW(byte red, byte green, byte blue, byte warmwhite)
        {
            //Update internal values
            Red = red;
            Green = green;
            Blue = blue;
            WarmWhite = warmwhite;

            //Prepare package to update the actual light
            byte[] colorPackage = new Byte[8];

            colorPackage[0] = 0x31; //packageid
            colorPackage[1] = red;    //red
            colorPackage[2] = green;    //green
            colorPackage[3] = blue;    //blue
            colorPackage[4] = warmwhite;   //warm-white
            colorPackage[5] = 0;    //unused
            colorPackage[6] = 0x0F; //local
            colorPackage[7] = CalculateCRC(colorPackage);

            SendPackage(colorPackage, 0);//We expect no reply
        }

        /*https://github.com/sidoh/ledenet_api/blob/b6f0c5cba03c535cdc877ac2f9787e88763d16be/lib/ledenet/packets/set_function_request.rb
         *
         *
         */

        public virtual void UpdateFunction(byte functionId, byte speed)
        {
            CurrentFunction = functionId;
            FunctionSpeed = speed;

            byte[] functionPackage = new byte[5];

            functionPackage[0] = 0x61;
            functionPackage[1] = functionId;
            functionPackage[2] = speed;//0-100
            functionPackage[3] = 0x0F;
            functionPackage[4] = CalculateCRC(functionPackage);

            SendPackage(functionPackage, 0);
        }

        /* This function updates this object to match the status of actual light.
         *
         */

        public virtual void GetStatus()
        {
            byte[] statusRequestPackage = new Byte[4];

            statusRequestPackage[0] = 0x81;
            statusRequestPackage[1] = 0x8A;
            statusRequestPackage[2] = 0x8B;
            statusRequestPackage[3] = CalculateCRC(statusRequestPackage);

            byte[] reply = SendPackage(statusRequestPackage, 14);
            //https://github.com/sidoh/ledenet_api/blob/b6f0c5cba03c535cdc877ac2f9787e88763d16be/lib/ledenet/packets/status_response.rb
            //Reply package structure.
            //          [0]uint8: packet_id, value: 0x81
            //          [1]uint8: device_name
            //          [2]uint8 :power_status
            //          # I'm not sure these are the correct field labels.  Basing it off of some
            //          # documentation that looks like it's for a slightly different protocol.
            //          [3]uint8 :mode
            //          [4]uint8 :run_status
            //          [5]uint8 :speed
            //          [6]uint8 :red
            //          [7]uint8 :green
            //          [8]uint8 :blue
            //          [9]uint8 :warm_white

            //          [10-12]uint24be :unused_payload
            //          [13]uint8 :checksum
            //Check if we had the right first byte and correct crc
            if (reply[0] == 0x81 && reply[13] == CalculateCRC(reply))
            {
                Debug.WriteLine("[getStatus] Got a correct reply!");
                //Now to do something with it.
                if (reply[2] == 0x23)
                {
                    Active = true;
                }
                else
                {
                    Active = false;
                }

                CurrentFunction = reply[3];
                FunctionSpeed = reply[5];

                Red = reply[6];
                Green = reply[7];
                Blue = reply[8];
                WarmWhite = reply[9];
                OnWifiLedUpdate();
            }
            else
            {
                Debug.WriteLine("[getStatus] CRC mismatch. Got " + reply[13] + ". Expected " + CalculateCRC(reply) + ". ");
            }
            /*
            int count = 0;
            foreach (byte subpart in reply) {
                Debug.WriteLine("[getStatus] reply[" + count + "]=" + subpart);
                count++;
            }
            */
        }

        /* SendPackage does the heavy lifting in this object of actually sending and listening for the replies.
         * byte[] package =  byte array with a pre crc'ed last byte
         * int expectedReplyLenght is the lenght in bytes we will wait to receive from the light
         * Returns byte[] which is the reply if any is received or null otherwise
         */

        protected virtual byte[] SendPackage(byte[] package, int expectedReplyLength)
        {
            request++;
            //If not connected, make a connection. Might need proper error handling
            //Debug.WriteLine("Sender timeout: {0},{1}",Sender.ReceiveTimeout,Sender.SendTimeout);
            byte[] reply = null;
            NetworkStream strem = null;

            for (int attempts = 0; attempts < 5; attempts++)
            {
                //Check if we are still connected or if more than 4 minutes have passed (might need tuning)
                //Debug.WriteLine("[SendPackage]Last network activity was {0} seconds ago.", DateTime.Now.Subtract(lastNetworkActivity).TotalSeconds);
                if (!Sender.Connected || DateTime.Now.Subtract(lastNetworkActivity) > TimeSpan.FromMinutes(4))
                {
                    try
                    {
                        //If are not connected any more. just reconnecting may not be the answer.
                        Debug.WriteLine("[SendPackage] Attempting to reconnect...");
                        Sender.Dispose();
                        Sender = new TcpClient();
                        Sender.Connect(iPAddress, TcpPort);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[SendPackage] Error connecting to WifiLed: " + e.Message);
                        Debug.Fail(e.StackTrace);
                    }
                }
                // Debug.WriteLine(Encoding.ASCII.GetString(package));

                try
                {
                    strem = Sender.GetStream();
                    strem.Write(package, 0, package.Length);
                    break;
                }
                catch (IOException e)
                {
                    Debug.WriteLine("[SendPackage] Encountered an exception while trying to send.");
                    Sender.Close();
                }
            }
            if (strem == null)
            {
                throw new IOException("Unable to acquire a stream to send the package.");
            }

            if (expectedReplyLength > 0)
            {
                reply = new Byte[expectedReplyLength];
                //Wait for a limited time until
                byte countdown = 5;

                //Debug.WriteLine("[sendPackage] Before the LOOP we have " + sender.Available + " bytes ready and expecting: " + expectedReplyLength + " | countdown = " + countdown);
                while (Sender.Available != expectedReplyLength && countdown > 0)
                {
                    Debug.WriteLine("[sendPackage] Waiting for reply to arrive. ZZZZZ");
                    System.Threading.Thread.Sleep(200);
                    countdown--;
                }
                Debug.WriteLine("[sendPackage] After the LOOP we have " + Sender.Available + " bytes ready and expecting: " + expectedReplyLength + " | countdown = " + countdown);
                if (Sender.Available == expectedReplyLength)
                {
                    strem.Read(reply, 0, expectedReplyLength);
                    Debug.WriteLine("[sendPackage] Reply is: " + Encoding.ASCII.GetString(reply));
                }
                //processing the result is left to the calling function
            }
            else if (Sender.Available > 0)
            {
                Debug.WriteLine("[sendPackage] We received " + Sender.Available + " bytes from socket. But expected: " + expectedReplyLength);
                reply = new Byte[Sender.Available];
                strem.Read(reply, 0, Sender.Available);
                Debug.WriteLine("[sendPackage] Unhandled data is: " + Encoding.ASCII.GetString(reply));
            }
            //Debug.WriteLine("[SendPackage] sending package {0} complete.", request);
            lastNetworkActivity = DateTime.Now;
            return reply;
        }

        protected byte[] SendPackageAsync(byte[] package, int expectedReplyLength)
        {
            request++;
            //If not connected, make a connection. Might need proper error handling

            byte[] reply = null;
            NetworkStream strem = null;
            Task write;
            for (int attempts = 0; attempts < 5; attempts++)
            {
                if (!Sender.Connected)
                {
                    try
                    {
                        //If are not connected any more. just reconnecting may not be the answer.
                        Debug.WriteLine("[SendPackage] Attempting to reconnect...");
                        Sender.Dispose();
                        Sender = new TcpClient();
                        Sender.Connect(iPAddress, TcpPort);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[SendPackage] Error connecting to WifiLed: " + e.Message);
                        Debug.Fail(e.StackTrace);
                    }
                }
                // Debug.WriteLine(Encoding.ASCII.GetString(package));

                try
                {
                    strem = Sender.GetStream();
                    write = strem.WriteAsync(package, 0, package.Length);
                    write.Wait();
                    break;
                }
                catch (IOException e)
                {
                    Debug.WriteLine("[SendPackage] Encountered an exception while trying to send.");
                    Debug.WriteLine(e);
                    Sender.Close();
                }
            }
            if (strem == null)
            {
                throw new IOException("Unable to acquire a stream to send the package.");
            }

            if (expectedReplyLength > 0)
            {
                reply = new Byte[expectedReplyLength];
                //Just wait until done
                Task read = strem.ReadAsync(reply, 0, expectedReplyLength);
                read.Wait();

                //processing the result is left to the calling function
            }
            else if (Sender.Available > 0)
            {
                Debug.WriteLine("[sendPackage] We received " + Sender.Available + " bytes from socket. But expected: " + expectedReplyLength);
                reply = new Byte[Sender.Available];
                strem.Read(reply, 0, Sender.Available);
                Debug.WriteLine("[sendPackage] Unhandled data is: " + Encoding.ASCII.GetString(reply));
            }
            Debug.WriteLine("[SendPackage] sending package {0} complete.", request);
            return reply;
        }

        protected byte CalculateCRC(byte[] package)
        {
            byte checksum = 0;
            for (uint i = 0; i < package.Length - 1; ++i)
            {
                checksum += package[i];
            }
            return checksum;
        }

        public override String ToString()
        {
            return (name);
        }

        public bool Equals(WifiLed other)
        {
            Debug.WriteLine(other.ToString());
            if (other != null && macAddress.Equals(other.macAddress))
            {
                return true;
            }
            return false;
        }

        /* This function sends events to whoever is listening.
         * This uses some logic to not step on the toes of other threads. Primarily for the sake of mainview.
         * https://www.codeproject.com/Articles/11848/Another-Way-to-Invoke-UI-from-a-Worker-Thread
         */

        protected virtual void OnWifiLedUpdate()
        {
            //WifiLedUpdated?.Invoke(this, EventArgs.Empty);
            EventHandler<EventArgs> handler = WifiLedUpdated;
            if (null != handler)
            {//not quite sure why there are so many null checks, I guess sanity checks?
                foreach (EventHandler<EventArgs> singleCast in handler.GetInvocationList())
                {
                    ISynchronizeInvoke syncInvoke = singleCast.Target as ISynchronizeInvoke;
                    try
                    {
                        if ((null != syncInvoke) && (syncInvoke.InvokeRequired))
                        {
                            syncInvoke.Invoke(singleCast, new object[] { this, EventArgs.Empty });
                        }
                        else
                        {
                            singleCast(this, EventArgs.Empty);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[OnWifiLedUpdate] Encountered an exception: " + e.Message);
                    }
                }
            }
        }

        public event EventHandler<EventArgs> WifiLedUpdated;
    }
}