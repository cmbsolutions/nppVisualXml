using nppVisualXml.Properties;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using nppVisualXml.Storage.Models;
using System.Collections.Generic;

namespace nppVisualXml.Storage
{
    public class Settings
    {
        public SettingsModel settings { get; set; }

        private string FilePath { get; set; }

        public void Load(bool reset = false)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string savePath = Path.Combine(appDataPath, "CMBSolutions", "nppVisualXml");
            FilePath = Path.Combine(savePath, "nppVisualXml.json");

            if (!File.Exists(FilePath) || reset)
            {
                try
                {
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    using (StreamWriter writer = new StreamWriter(FilePath))
                    {
                        writer.WriteLine(Resources.nppVisualXmlSettings);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Error creating settingsfile");
                    return;
                }
            }

            try
            {
                settings = DeserializeJSonFile(FilePath);


                if (settings.Appversion != "0.0.1")
                {
                    SettingsModel defaults = DeserializeJSonFromString(Resources.nppVisualXmlSettings);

                    settings.Appname = "nppVisualXml";
                    settings.Appversion = "0.0.1";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private SettingsModel DeserializeJSonFile(string jsonfile)
        {
            SettingsModel tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText(jsonfile));
            return tmp;
        }

        private SettingsModel DeserializeJSonFromString(string json)
        {
            SettingsModel tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsModel>(json);
            return tmp;
        }

        // Save JSON string to a file
        public void Save()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FilePath, json);                       
        }
    }
}
