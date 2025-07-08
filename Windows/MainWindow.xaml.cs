using System;
using System.ComponentModel;
using System.Windows;

using GTRC_WPF;
using NutSort.Models;

namespace NutSort.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            GlobalValues.CreateDirectories();
            NutColor.LoadJson();
            //Board.LoadJson();
            //Board board = new() { StackCount = 11, StackHeight = 4, NutSameColorCount = 4, ColorCount = 9, MaxColumnsCount = 5 };
            //board.CreateInitialBoardstate("schwarz|schwarz|orange|orange|blass|grün|grün|hellblau|rosa|grau|gelb|hellblau|blass|lila|grau|rosa|lila|blass|orange|grau|schwarz|rosa|grün|grau|blass|schwarz|gelb|lila|grün|orange|lila|rosa|gelb|hellblau|gelb|hellblau|||||||||");
            Board board = new() { StackCount = 4, StackHeight = 4, NutSameColorCount = 2, ColorCount = 4, MaxColumnsCount = 5 };
            board.CreateInitialBoardstate("schwarz|schwarz|orange|orange|grün|grün|hellblau|hellblau|");
            Board.List.Add(board);
            Board.SaveJson();
            InitializeComponent();
            Width = GlobalWinValues.screenWidth * 0.3;
            Height = GlobalWinValues.screenHeight * 0.25;
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
        }
    }
}