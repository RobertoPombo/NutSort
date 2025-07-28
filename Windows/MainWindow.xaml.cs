using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

using NutSort.Models;
using GTRC_WPF;

namespace NutSort.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            GlobalValues.CreateDirectories();
            NutColor.LoadJson();
            Board.LoadJson();
            UpdateThemeColors();
            InitializeComponent();
            Width = GlobalWinValues.screenWidth * 0.18;
            Height = GlobalWinValues.screenHeight * 0.69;
            Left = ((GlobalWinValues.screenWidth / 2) - (Width / 2)) * 1;
            Top = ((GlobalWinValues.screenHeight / 2) - (Height / 2)) * 1;
            Closing += CloseWindow;
        }

        public void CloseWindow(object? sender, CancelEventArgs e)
        {
            foreach (Board board in Board.List)
            {
                board.StopSolving();
            }
            if (ViewModels.MainVM.Instance?.BoardstateVM is not null)
            {
                ViewModels.MainVM.Instance.BoardstateVM.AnimationIsRunning = false;
            }
        }

        public void UpdateThemeColors()
        {
            GlobalWinValues.UpdateWpfColors(this, LoadThemeColorJson());
        }

        public static List<GTRC_Basics.Models.Color> LoadThemeColorJson()
        {
            List<GTRC_Basics.Models.Color> list = [];
            string path = GlobalValues.ConfigDirectory + "ThemeColor.json";
            if (!File.Exists(path)) { File.WriteAllText(path, JsonConvert.SerializeObject(new List<GTRC_Basics.Models.Color>(), Formatting.Indented), Encoding.Unicode); }
            try
            {
                list = JsonConvert.DeserializeObject<List<GTRC_Basics.Models.Color>>(File.ReadAllText(path, Encoding.Unicode)) ?? [];
                GlobalValues.CurrentLogText = "Theme colors restored.";
            }
            catch { GlobalValues.CurrentLogText = "Restore theme colors failed!"; }
            return list;
        }
    }
}