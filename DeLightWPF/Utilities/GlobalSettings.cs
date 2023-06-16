using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace DeLightWPF.Utilities
{
    public class GlobalSettings
    {
        public static GlobalSettings Instance { get; private set; } = new();
        public double DefaultFadeTime { get; set; } = 3;

        public double DefaultVolume { get; set; } = .2;

        public double DefaultDuration { get; set; } = 5;
        public string LastShowPath { get; set; } = "";

        public Screen? Screen { get; set; } = Screen.PrimaryScreen;

        public GlobalSettings()
        {
            Console.WriteLine("Created new settings");
        }
        public static void Load()
        {
            // Find settings json and load it, or create a new one if it doesn't exist
            if (File.Exists("settings.json"))
            {
                using StreamReader r = new ("settings.json");
                string json = r.ReadToEnd();
                Instance = JsonSerializer.Deserialize<GlobalSettings>(json) ?? new GlobalSettings();
            }
            else
            {
                Console.WriteLine("settings.json file not found. Creating new settings file.");
                Instance = new GlobalSettings();
                Save();
            }
        }
        public static void Save()
        {
            string json = JsonSerializer.Serialize(Instance);
            File.WriteAllText("settings.json", json);
        }
    }
}
