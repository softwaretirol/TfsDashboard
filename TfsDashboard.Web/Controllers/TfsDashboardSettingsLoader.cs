using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TfsDashboard.Library;

namespace TfsDashboard.Web.Controllers
{
    public class TfsDashboardSettingsLoader
    {
        private static string _settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TfsDashboardSettings.json");
        public static IEnumerable<TfsDashboardSettings> Load()
        {
            string json;
            if(!File.Exists(_settingsFile))
            {
                var defaultSettings = new TfsDashboardSettings[1];
                defaultSettings[0] = new TfsDashboardSettings();
                json  = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                File.WriteAllText(_settingsFile, json);
                return defaultSettings;
            }
            else
            {
                json = File.ReadAllText(_settingsFile);
            }

            return JsonConvert.DeserializeObject<TfsDashboardSettings[]>(json);
        }
    }
}