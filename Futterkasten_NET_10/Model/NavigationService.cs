using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Futterkasten.Model;
using Futterkasten;

namespace Futterkasten
{
    public partial class NavigationService : INavigationService
    {
        public async Task PageMainPage()
        {
            await Shell.Current.GoToAsync(new ShellNavigationState("//MainPage"));
        }

        public async Task PageEssenslisteBearbeiten()
        {
            await Shell.Current.GoToAsync(new ShellNavigationState("///View/EssenslisteBearbeiten.xaml"));
        }

        public object GoToEssenslisteBearbeiten(string essensliste, Dictionary<string, object> dictionary)
        {
            throw new NotImplementedException();
        }
    }
}
