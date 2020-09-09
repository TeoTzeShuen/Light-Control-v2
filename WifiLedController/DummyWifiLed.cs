using System;
using System.Diagnostics;
using System.Net;

namespace WifiLedController
{
    internal class DummyWifiLed : WifiLed
    {
        /* Creates a new light object
         * discoIP is the IP it was found on
         * macaddress is its macaddress. Assuming these are unique for now.
         */

        public DummyWifiLed(IPAddress discoIP, String macAddress) : base(discoIP, macAddress)
        {
        }

        //Turn the light on. Whatever setting are stored on the light at last use will be restored
        public override void TurnOn()
        {
            Active = true;
        }

        //Turns light off. Can be safely sent when light is already off.
        public override void TurnOff()
        {
            Active = false;
        }

        /* This function changes the rgb and white values of the connected light
         * Values are supplied in byte[0-255] values are self describing
         */

        public override void UpdateRGBWW(byte red, byte green, byte blue, byte warmwhite)
        {
            //Update internal values
            Red = red;
            Green = green;
            Blue = blue;
            WarmWhite = warmwhite;
        }

        /* This function updates this object to match the status of actual light.
         *
         */

        public override void GetStatus()
        {
            //Dummy has no need to check anything
            OnWifiLedUpdate();
        }

        protected override byte[] SendPackage(byte[] package, int expectedReplyLength)
        {
            Debug.WriteLine("[Dummy_SendPackage]Function should not have been called. Returning empty response...");
            return null;
        }
    }
}