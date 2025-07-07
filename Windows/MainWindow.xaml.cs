using System;
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
            Board.LoadJson();
            //Board board = new();
            //board.CreateInitialBoardstate();
            //Board.List.Add(board);
            Board.SaveJson();
            InitializeComponent();
            Width = GlobalWinValues.screenWidth * 0.75;
            Height = GlobalWinValues.screenHeight * 0.75;
            Left = ((GlobalWinValues.screenWidth / 2) - (Width / 2)) * 1;
            Top = ((GlobalWinValues.screenHeight / 2) - (Height / 2)) * 1;
        }
    }
}