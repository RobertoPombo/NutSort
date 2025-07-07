using System;

namespace NutSort.ViewModels
{
    public class MainVM : GTRC_WPF.ViewModels.MainVM
    {
        public static MainVM? Instance;

        public MainVM()
        {
            Instance = this;
        }
    }
}
