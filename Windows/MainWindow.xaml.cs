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
            //Board.LoadJson();
            //Board board = new() { StackCount = 4, StackHeight = 4, NutSameColorCount = 2, ColorCount = 4, MaxColumnsCount = 5 };
            //board.CreateInitialBoardstate("schwarz|schwarz|orange|orange|grün|grün|hellblau|hellblau|");
            //Board board = new() { StackCount = 11, StackHeight = 4, NutSameColorCount = 4, ColorCount = 9, MaxColumnsCount = 5 };
            //board.CreateInitialBoardstate("schwarz|schwarz|orange|orange|blass|grün|grün|hellblau|rosa|grau|gelb|hellblau|blass|lila|grau|rosa|lila|blass|orange|grau|schwarz|rosa|grün|grau|blass|schwarz|gelb|lila|grün|orange|lila|rosa|gelb|hellblau|gelb|hellblau|||||||||");
            Board board = new() { StackCount = 14, StackHeight = 4, NutSameColorCount = 4, ColorCount = 12, MaxColumnsCount = 5 };
            board.CreateInitialBoardstate("grau|rot|grün|lila|blau|lila|orange|gelb|blau|gelb|gelb|blass|schwarz|pink|pink|schwarz|rot|rosa|grau|rot|orange|grün|orange|rot|blau|blass|rosa|schwarz|grau|blass|hellblau|hellblau|pink|orange|schwarz|lila|pink|gelb|blass|hellblau|rosa|grün|grau|grün|hellblau|blau|rosa|lila|||||||||");
            Board.List.Add(board);
            Board.SaveJson();
            UpdateThemeColors();
            InitializeComponent();
            Width = GlobalWinValues.screenWidth * 0.2;
            Height = GlobalWinValues.screenHeight * 0.5;
            Left = ((GlobalWinValues.screenWidth / 2) - (Width / 2)) * 1;
            Top = ((GlobalWinValues.screenHeight / 2) - (Height / 2)) * 1;
            Closing += CloseWindow;
            board.Solve();
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