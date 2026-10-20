using Futterkasten.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;


namespace Futterkasten.ViewModel
{
    partial class EssenBearbeitenViewModel : ObservableObject
    {
        int counter = 1;
        readonly string stream = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Essensliste.txt");
        private readonly IDispatcherTimer timer;

        [ObservableProperty] private ObservableCollection<Essen> essenAktuellLocal; // für Anzeige auf NewPage.xaml

        [ObservableProperty] private Essen collectionViewSelectedItem;
        
        [ObservableProperty] private bool istAufwändig;
        
        [ObservableProperty] private bool magJessy;
       
        [ObservableProperty] private bool magRene;
        [ObservableProperty] private bool jessysInsuFood;
       
        [ObservableProperty] private string? hinzufügenInput;

        [ObservableProperty] private bool hinzufügenButtonIsEnabled;
        
        public EssenBearbeitenViewModel()
        {
            essenAktuellLocal = new ObservableCollection<Essen>();
            Load_Essensliste();

            if (!File.Exists(stream))
            {
                var file = File.Create(stream);
                file.Close();
            }

            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Start();   
            
        }

        private void Load_Essensliste()
        {
            Essen essen = new Essen();
            List<string> gerichteAlle = new List<string>();
            
            var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
                gerichteAlle.Add(reader.ReadLine());
            reader.Close();

            for (var i = 0; i < gerichteAlle.Count; i++)
            {
                var zeile = gerichteAlle[i];
                var values = zeile.Split(";");

                for (var j = 0; j < values.Length - 1; j++)
                {
                    if (j == 0) essen.Name = values[j];
                    if (j == 1 && values[j] == " +") essen.MagJessy = true;
                    else if (j == 1 && values[j] == " -") essen.MagJessy = false;
                    if (j == 2 && values[j] == " +") essen.MagRene = true;
                    else if (j == 2 && values[j] == " -") essen.MagRene = false;
                    if (j == 3 && values[j] == " +") essen.IsAufwändig = true;
                    else if (j == 3 && values[j] == " -") essen.IsAufwändig = false;
                    if (j == 4 && values[j] == " +") essen.JessysInsuFood = true;
                    else if (j == 4 && values[j] == " -") essen.JessysInsuFood = false;
                }
                EssenAktuellLocal.Add(essen);
            }
            HinzufügenButtonIsEnabled = false;
            //MainPageViewModel.EssensListeAktuell = EssenAktuellLocal;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (HinzufügenInput != null)
            {
                HinzufügenButtonIsEnabled = true;
            }
        }

        [RelayCommand]                 
        async private Task Hinzufügen()                            
        {
            HinzufügenButtonIsEnabled = false;
            char charJessy;
            if (MagJessy) charJessy = '+'; else charJessy = '-';
            char charRene;
            if (MagRene) charRene = '+'; else charRene = '-';
            char charAufwändig;
            if (IstAufwändig) charAufwändig = '+'; else charAufwändig = '-';
            char charJessyInsuFood;
            if (JessysInsuFood) charJessyInsuFood = '+'; else charJessyInsuFood = '-';

            var sw = new StreamWriter(stream, true);
            sw.WriteLine(HinzufügenInput + "; " +
                            charJessy + "; " +
                            charRene + "; " +
                            charAufwändig + "; " +
                            charJessyInsuFood + "; ");
            Essen essen = new Essen { Name = HinzufügenInput, IsAufwändig = IstAufwändig, MagJessy = MagJessy, MagRene = MagRene, JessysInsuFood = JessysInsuFood };
            EssenAktuellLocal.Add(essen);
            sw.Close();

            HinzufügenInput = null;
            MagJessy = false;
            MagRene = false;
            IstAufwändig = false;
            JessysInsuFood = false;
            
            GlobalVariables.fileChanged = true;
            MainPageViewModel.EssensListeAktuell = EssenAktuellLocal;
            Load_Essensliste();
            //NavigationService navigationService = new NavigationService(); // wieder zurück auf MainPage nach Eingabe!
            //await navigationService.PageMainPage();
        }
        
        [RelayCommand]
        async private Task CollectionViewSelectionChanged()                  // Löschen
        {
            char charJessy;
            if (MagJessy) charJessy = '+'; else charJessy = '-';
            char charRene;
            if (MagRene) charRene = '+'; else charRene = '-';
            char charAufwändig;
            if (IstAufwändig) charAufwändig = '+'; else charAufwändig = '-';
            char charJessyInsuFood;
            if (JessysInsuFood) charJessyInsuFood = '+'; else charJessyInsuFood = '-';

            if (counter == 1)
            {
                if (await Application.Current.MainPage.DisplayAlertAsync("Achtung!", "Möchten Sie den Eintrag: " + CollectionViewSelectedItem.Name + " wirklich löschen?", "Ja", "Nein")
                   && EssenAktuellLocal.Contains(CollectionViewSelectedItem))
                {
                    counter++;
                    EssenAktuellLocal.Remove(CollectionViewSelectedItem);

                    var sw = new StreamWriter(stream);
                    for (int i = 0; i < EssenAktuellLocal.Count; i++)
                    {
                        sw.WriteLine(EssenAktuellLocal[i].Name + "; " + charJessy + "; " + charRene + "; " + charAufwändig + ";" + charJessyInsuFood + ";" );
                    }
                    sw.Close();
                }
            }
            counter = 1;
            GlobalVariables.fileChanged = true;
        }
    }
}