using System;
using System.IO;

namespace NutSort
{
    public delegate void Notify();

    public static class GlobalValues
    {
        private static string baseDirectory = "C:\\Users\\Public\\Documents\\NutSort App Data\\";
        private static string currentLogText = string.Empty;

        public static string BaseDirectory { get { return baseDirectory; } set { baseDirectory = value; } }
        public static string ConfigDirectory { get { return baseDirectory + "config\\"; } }
        public static string DataDirectory { get { return baseDirectory + "data\\"; } }
        public static string DebugDirectory { get { return baseDirectory + "debug\\"; } }
        public static string CurrentLogText { get { return currentLogText; } set { currentLogText = value; OnNewLogText(); } }

        public static event Notify? NewLogText;
        public static void OnNewLogText() { NewLogText?.Invoke(); }

        public static void CreateDirectories()
        {
            if (!Directory.Exists(BaseDirectory)) { Directory.CreateDirectory(BaseDirectory); }
            if (!Directory.Exists(ConfigDirectory)) { Directory.CreateDirectory(ConfigDirectory); }
            if (!Directory.Exists(DataDirectory)) { Directory.CreateDirectory(DataDirectory); }
            if (!Directory.Exists(DebugDirectory)) { Directory.CreateDirectory(DebugDirectory); }
        }
    }
}
