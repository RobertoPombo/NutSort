using System;

namespace NutSort.ViewModels
{
    public class MainVM : GTRC_WPF.ViewModels.MainVM
    {
        public static MainVM? Instance;

        private BoardstateVM? boardstateVM;

        public MainVM()
        {
            Instance = this;
            BoardstateVM = new BoardstateVM();
        }

        public BoardstateVM? BoardstateVM { get { return boardstateVM; } set { boardstateVM = value; RaisePropertyChanged(); } }
    }
}
