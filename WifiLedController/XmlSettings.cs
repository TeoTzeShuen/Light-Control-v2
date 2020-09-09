using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace WifiLedController
{
    internal class XmlSettings
    {
        private XmlDocument settings;
        private readonly string FileUrl = "./Settings.xml";//settings file in same folder as working directory

        public XmlSettings()
        {
            //settings being null is a problem for loading
            settings = new XmlDocument();
        }

        public string FindCustomName(string macAddress)
        {
            XmlNode result = settings.SelectSingleNode("//CustomName[MacAddress=\"" + macAddress + "\"]/Name");
            Debug.WriteLine("[XmlSettings.FindCustomName({0})] result = {1}", macAddress, result);
            if (result == null)
            {
                return null;
            }
            return result.InnerText;
        }

        //Does adding and updating of elements
        public void AddCustomName(string macAddress, string name)
        {
            //Check if we have a node already
            XmlNode result = settings.DocumentElement.SelectSingleNode("//CustomName[MacAddress=\"" + macAddress + "\"]/Name");
            if (result == null)
            {//no existing node available
                //create CustomName element
                XmlElement customName = settings.CreateElement("CustomName");
                //Add macaddress to the custom name
                XmlElement macAddressElement = settings.CreateElement("MacAddress");
                macAddressElement.InnerText = macAddress;
                customName.AppendChild(macAddressElement);
                //add name
                XmlElement nameElement = settings.CreateElement("Name");
                nameElement.InnerText = name;
                customName.AppendChild(nameElement);

                settings.DocumentElement.AppendChild(customName);
            }
            else
            {
                //simple update
                result.InnerText = name;
            }
        }

        //  X, Xwidth,Xstride, Y, Yheight, Ystride
        public (int, int, int, int, int, int, float, bool, bool) ReadAmbianceSettings()
        {
            XmlNode result = settings.DocumentElement.SelectSingleNode("//AmbianceSettings");
            //default settings
            int X = 0;
            int Xwidth = 1920;
            int Xstride = 10;
            int Y = 1060;
            int Yheight = 20;
            int Ystride = 2;
            float LimiterTimeValue = 10;
            bool LimiterActive = true;
            bool LimiterSecondsUpdate = false;
            //attempt read the options
            if (result != null)
            {//<X>0</X><Xwidth>1920</Xwidth><Xstride>10</Xstride><Y>1060</Y><Yheight>20</Yheight><Ystride>2</Ystride>
                Debug.WriteLine("[ReadAmbianceSettings()] " + result.InnerXml);
                //Parse the Xml
                try
                {
                    X = int.Parse(result["X"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading X.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    Xwidth = int.Parse(result["Xwidth"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading Xwidth.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    Xstride = int.Parse(result["Xstride"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading Xstride.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    Y = int.Parse(result["Y"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading Y.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    Yheight = int.Parse(result["Yheight"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading Yheight.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    Ystride = int.Parse(result["Ystride"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading Ystride.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    LimiterTimeValue = float.Parse(result["LimiterTimeValue"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading LimiterTimeValue.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    LimiterActive = bool.Parse(result["LimiterActive"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading LimiterActive.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    LimiterSecondsUpdate = bool.Parse(result["LimiterSecondsUpdate"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceSettings()] Encountered exception while reading LimiterSecondsUpdate.");
                    Debug.WriteLine(ex.Message);
                }
            }
            //Default values
            return (X, Xwidth, Xstride, Y, Yheight, Ystride, LimiterTimeValue, LimiterActive, LimiterSecondsUpdate);
        }

        public void AddAmbianceSettings(int X, int Xwidth, int Xstride, int Y, int Yheight, int Ystride, float LimiterTimeValue, bool LimiterActive, bool LimiterSecondsUpdate)
        {
            XmlNode result = settings.DocumentElement.SelectSingleNode("//AmbianceSettings");

            Debug.WriteLine("[AddAmbianceSettings] {0}, {1}, {2}, {3}, {4}, {5}", X, Xwidth, Xstride, Y, Yheight, Ystride);
            if (result == null)
            {//No settings at all
                XmlElement colorSettings = settings.CreateElement("AmbianceSettings");

                //Set up the internal structure and values
                XmlElement setting = settings.CreateElement("X");
                setting.InnerText = X.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Xwidth");
                setting.InnerText = Xwidth.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Xstride");
                setting.InnerText = Xstride.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Y");
                setting.InnerText = Y.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Yheight");
                setting.InnerText = Yheight.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Ystride");
                setting.InnerText = Ystride.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("LimiterTimeValue");
                setting.InnerText = LimiterTimeValue.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("LimiterActive");
                setting.InnerText = LimiterActive.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("LimiterSecondsUpdate");
                setting.InnerText = LimiterSecondsUpdate.ToString();
                colorSettings.AppendChild(setting);

                settings.DocumentElement.AppendChild(colorSettings);
            }
            else
            {//There are some settings
                Debug.WriteLine("[AddAmbianceSettings] " + result.InnerXml);

                Debug.WriteLine("[ReadAmbianceSettings()] " + result.InnerXml);
                //Parse the Xml
                try
                {
                    result["X"].InnerText = X.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading X.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("X");
                    setting.InnerText = X.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Xwidth"].InnerText = Xwidth.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading Xwidth.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Xwidth");
                    setting.InnerText = Xwidth.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Xstride"].InnerText = Xstride.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading Xstride.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Xstride");
                    setting.InnerText = Xstride.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Y"].InnerText = Y.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading Y.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Y");
                    setting.InnerText = Y.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Yheight"].InnerText = Yheight.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading Yheight.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Yheight");
                    setting.InnerText = Yheight.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Ystride"].InnerText = Ystride.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading Ystride.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Ystride");
                    setting.InnerText = Ystride.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["LimiterTimeValue"].InnerText = LimiterTimeValue.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading LimiterTimeValue.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("LimiterTimeValue");
                    setting.InnerText = LimiterTimeValue.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["LimiterActive"].InnerText = LimiterActive.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading LimiterActive.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("LimiterActive");
                    setting.InnerText = LimiterActive.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["LimiterSecondsUpdate"].InnerText = LimiterSecondsUpdate.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceSettings()] Encountered exception while reading LimiterSecondsUpdate.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("LimiterSecondsUpdate");
                    setting.InnerText = LimiterSecondsUpdate.ToString();
                    result.AppendChild(setting);
                }
            }
        }

        public (float, float, float, string) ReadAmbianceColorTuningSettings()
        {
            XmlNode result = settings.DocumentElement.SelectSingleNode("//AmbianceColorTuningSettings");
            float red = 0;
            float green = 0;
            float blue = 0;
            string mode = "Additive";
            if (result != null)
            {
                Debug.WriteLine("[ReadAmbianceColorTuningSettings()] " + result.InnerXml);
                //Parse the Xml
                try
                {
                    red = float.Parse(result["Red"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceColorTuningSettings()] Encountered exception while reading Red.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    green = float.Parse(result["Green"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceColorTuningSettings()] Encountered exception while reading Red.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    blue = float.Parse(result["Blue"].InnerText);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceColorTuningSettings()] Encountered exception while reading Red.");
                    Debug.WriteLine(ex.Message);
                }
                try
                {
                    mode = result["Mode"].InnerText;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[ReadAmbianceColorTuningSettings()] Encountered exception while reading Red.");
                    Debug.WriteLine(ex.Message);
                }
            }
            //Default values
            return (red, green, blue, mode);
        }

        public void AddAmbianceColorTuningSettings(float red, float green, float blue, string mode)
        {
            XmlNode result = settings.DocumentElement.SelectSingleNode("//AmbianceColorTuningSettings");
            //Debug.WriteLine("[AddAmbianceSettings] " + result.InnerXml);

            if (result == null)
            {
                XmlElement colorSettings = settings.CreateElement("AmbianceColorTuningSettings");

                //Set up the internal structure and values
                XmlElement setting = settings.CreateElement("Red");
                setting.InnerText = red.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Green");
                setting.InnerText = green.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Blue");
                setting.InnerText = blue.ToString();
                colorSettings.AppendChild(setting);

                setting = settings.CreateElement("Mode");
                setting.InnerText = mode;
                colorSettings.AppendChild(setting);

                settings.DocumentElement.AppendChild(colorSettings);
            }
            else
            {
                try
                {
                    result["Red"].InnerText = red.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceColorTuningSettings()] Encountered exception while reading Red.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Red");
                    setting.InnerText = red.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Green"].InnerText = green.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AddAmbianceColorTuningSettings()] Encountered exception while reading Green.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Green");
                    setting.InnerText = green.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Blue"].InnerText = blue.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceColorTuningSettings()] Encountered exception while reading Blue.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Blue");
                    setting.InnerText = blue.ToString();
                    result.AppendChild(setting);
                }
                try
                {
                    result["Mode"].InnerText = mode;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[AddAmbianceColorTuningSettings()] Encountered exception while reading Mode.");
                    Debug.WriteLine(ex.Message);
                    XmlElement setting = settings.CreateElement("Mode");
                    setting.InnerText = mode.ToString();
                    result.AppendChild(setting);
                }
            }
        }

        private string GenerateXMLSchemaText()
        {
            string schema = "<?xml version = \"1.0\" encoding = \"utf-8\"?>"
                            + "<xs:schema targetNamespace = \"http://tempuri.org/XMLSchema.xsd\" "
                            + "elementFormDefault = \"qualified\" "
                            + "xmlns = \"http://tempuri.org/XMLSchema.xsd\" "
                            + "xmlns:mstns = \"http://tempuri.org/XMLSchema.xsd\" "
                            + "xmlns:xs = \"http://www.w3.org/2001/XMLSchema\">"
                            + "<xs:element name = \"Settings\" />"
                            + "<xs:element name = \"CustomName\" type = \"MacAddressNamePair\"/>"
                            + "<xs:complexType name = \"MacAddressNamePair\">"
                            + "<xs:sequence>"
                            + "<xs:element name = \"MacAddress\" type = \"xs:string\"/>"
                            + "<xs:element name = \"Name\" type = \"xs:string\"/>"
                            + "</xs:sequence>"
                            + "</xs:complexType>"
                            + "<xs:element name = \"AmbianceSettings\" type = \"AmbianceSettingsValues\"/>"
                            + "<xs:complexType name = \"AmbianceSettingsValues\">"
                            + "<xs:sequence>"
                            + "<xs:element name = \"X\" type = \"xs:int\"/>"
                            + "<xs:element name = \"Xwidth\" type = \"xs:int\"/>"
                            + "<xs:element name = \"Xstride\" type = \"xs:int\"/>"
                            + "<xs:element name = \"Y\" type = \"xs:int\"/>"
                            + "<xs:element name = \"Yheight\" type = \"xs:int\"/>"
                            + "<xs:element name = \"Ystride\" type = \"xs:int\"/>"
                            + "<xs:element name = \"LimiterTimeValue\" type = \"xs:float\"/>"
                            + "<xs:element name = \"LimiterActive\" type = \"xs:boolean\"/>"
                            + "<xs:element name = \"LimiterSecondsUpdate\" type = \"xs:boolean\" />"
                            + "</xs:sequence>"
                            + "</xs:complexType >"
                            + "<xs:element name=\"AmbianceColorTuningSettings\" type=\"AmbianceColorTuningSettingsValues\"/>"
                            + "<xs:complexType name=\"AmbianceColorTuningSettingsValues\">"
                            + "<xs:sequence>"
                            + "<xs:element name=\"Red\" type=\"xs:float\"/>"
                            + "<xs:element name=\"Green\" type=\"xs:float\"/>"
                            + "<xs:element name=\"Blue\" type=\"xs:float\"/>"
                            + "<xs:element name=\"Mode\" type=\"xs:string\"/>"
                            + "</xs:sequence>"
                            + "</xs:complexType>"
                            + "</xs:schema>";
            return schema;
        }

        private XmlSchema GetXmlSchema()
        {
            XmlSchemaSet xs = new XmlSchemaSet();
            XmlSchema schema;
            byte[] byteArray = Encoding.UTF8.GetBytes(GenerateXMLSchemaText());
            MemoryStream stream = new MemoryStream(byteArray);
            XmlReader reader = XmlReader.Create(stream);

            schema = xs.Add("http://tempuri.org/XMLSchema.xsd", reader);

            return schema;
        }

        //Try to load existing settings
        public void Load()
        {
            if (File.Exists(FileUrl))
            {
                Debug.WriteLine("[XmlSettings.Load()] Attempting to load File.");
                try
                {//Try to load settings from file
                    //Debug.WriteLine(File.ReadAllText(FileUrl));
                    settings.Load(FileUrl);
                    Debug.WriteLine("[XmlSettings.Load()] File loaded..");
                    Debug.WriteLine(settings.InnerText);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("[XmlSettings.Load()] File exists but Exception {0}", e);
                    Debug.WriteLine(e.StackTrace);
                    //Create an empty representations
                    //settings = new XmlDocument();
                    settings.AppendChild(settings.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                    settings.AppendChild(settings.CreateElement("Settings"));//rootnode is settings
                }
            }
            else
            {
                //no existing file so create an empty one
                //settings = new XmlDocument();
                settings.AppendChild(settings.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                settings.AppendChild(settings.CreateElement("Settings"));
            }
            settings.Schemas.Add(GetXmlSchema());
        }

        //************************************************************************************
        //
        //  Event handler that is raised when XML doesn't validate against the schema.
        //
        //************************************************************************************
        private void settings_ValidationEventHandler(object sender, System.Xml.Schema.ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                System.Windows.Forms.MessageBox.Show("The following validation warning occurred: " + e.Message);
            }
            else if (e.Severity == XmlSeverityType.Error)
            {
                System.Windows.Forms.MessageBox.Show("The following critical validation errors occurred: " + e.Message);
                Type objectType = sender.GetType();
            }
        }

        public void Save()
        {
            settings.Validate(settings_ValidationEventHandler);
            settings.Save(FileUrl);
        }
    }
}