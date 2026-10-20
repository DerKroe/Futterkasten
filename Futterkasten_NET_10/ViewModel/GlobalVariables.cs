using System.Collections.ObjectModel;
using Futterkasten.Model;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;


namespace Futterkasten.ViewModel
{
    partial class GlobalVariables : ObservableObject, IGlobalVariables
    {
        public static bool fileChanged = false;
        public GlobalVariables() {}

        public ObservableCollection<Essen> EssensListeAktuell { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
