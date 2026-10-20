using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Futterkasten.Model
{
    internal interface INavigationService
    {
        Task PageMainPage();
        Task PageEssenslisteBearbeiten();
        object GoToEssenslisteBearbeiten(string essensliste, Dictionary<string, object> dictionary);
    }
}
