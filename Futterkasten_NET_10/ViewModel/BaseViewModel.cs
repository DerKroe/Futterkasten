using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Futterkasten.Model;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Timers;
using System.IO;
using CommunityToolkit.Mvvm.Input;

namespace Futterkasten.ViewModel
{
    partial class BaseViewModel : ObservableObject
    {
        public GlobalVariables glo_Va;
        public ICommand MagJessyCommand { get; set; }
        public ICommand MagReneCommand { get; set; }
        public ICommand IsAufwaendigCommand { get; set; }
        public ICommand Btn_NextAdviceCommand { get; set; }
        public ICommand Btn_AuswählenCommand { get; set; }
        public ICommand PageMainPageCommand { get; set; }
        public ICommand PageEssenslisteBearbeiten {  get; set; }
        private readonly double duration;
        private readonly List<string> gerichteAlle;
        private readonly List<Essen> Ignorlist;
        private const string komma = ", ";
        private readonly List<Essen> Kopie;
        private bool löschen;
        private readonly string pathFileIgno = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Ignorliste.txt");
        private readonly Random zufall;
        private Essen essen;
        private bool IsAusgewählt;
        private bool jessyChecked;
        private bool reneChecked;
        private bool aufwandChecked;
        private readonly string timeStampAnzeigetemp;
        private List<Essen> EssensListeFiltered;
        private IDispatcherTimer? timer;
        INavigationService navigationService;
        #region Variablen

        private Button _boolAuswählen = new Button 
        { 
          HeightRequest = 100, WidthRequest = 100
        };
        public Button BtnAuswählen 
        { get { return _boolAuswählen; } set { OnPropertyChanged(); } }


        private Label _lblHauptAnzeige = new Label();
        public Label lblHauptAnzeige 
        { get { return _lblHauptAnzeige; } set { OnPropertyChanged(); } }


        private Label _lblIgnorList = new Label 
        { Text = "IgnorList" };
        public Label LblIgnorliste 
        { get { return _lblIgnorList; } set { OnPropertyChanged(); } }


        private Label _lblEssensliste = new Label
            { Text = "" };
        public Label LblEssensliste 
        { get { return _lblEssensliste; } set { OnPropertyChanged(); } }

        private Button _btnNeuerVorschlag = new Button 
        { HeightRequest = 100, WidthRequest = 100 };
        public Button BtnNeuerVorschlag 
        { get { return _btnNeuerVorschlag; } set { OnPropertyChanged(); } }

        private Label _lblEssenslisteAktuellCount = new Label 
        { Text = "" };
        public Label LblEssenslisteAktuellCount 
        { get { return _lblEssenslisteAktuellCount; } set { OnPropertyChanged(); } }

        private Button _btnIsAufwaendig = new Button 
        { HeightRequest = 100, WidthRequest = 100, BackgroundColor = Colors.White, BorderColor = Colors.Gray, TextColor = Colors.Gray };
        public Button BtnIsAufwaendig 
        { get => _btnIsAufwaendig; set => SetProperty(ref _btnIsAufwaendig, value); }

        private Button _btnMagJessy = new Button 
        { HeightRequest = 100, WidthRequest = 100, BackgroundColor = Colors.White, BorderColor = Colors.Gray, TextColor = Colors.Gray };
        public Button BtnMagJessy 
        { get { return _btnMagJessy; } set { SetProperty(ref _btnMagJessy, value); } }

        private Button _btnMagRene = new Button 
        { HeightRequest = 100, WidthRequest = 100, BackgroundColor = Colors.White, BorderColor = Colors.Gray, TextColor = Colors.Gray };
        public Button BtnMagRene 
        { get => _btnMagRene; set => SetProperty(ref _btnMagRene, value); }
            
        private Label _lblTimeStamp = new Label();
        public Label LblTimeStamp 
        { get { return _lblTimeStamp; } set { SetProperty(ref _lblTimeStamp, value); } }

        #endregion
        public BaseViewModel()
        {
            glo_Va = new GlobalVariables();

            navigationService = new NavigationService();
            MagJessyCommand = new Command(MagJessy_Execute);
            MagReneCommand = new Command(MagRene_Execute);
            IsAufwaendigCommand = new Command(IsAufwaendig_Execute);
            Btn_NextAdviceCommand = new Command(NextAdviceExecute);
            Btn_AuswählenCommand = new Command(AuswählenExecute);
            //PageMainPageCommand = new Command(PageMainPageNavigationExecute);
            PageEssenslisteBearbeiten = new Command(PageEssenslisteBearbeitenNavigationExecute);


            timeStampAnzeigetemp = "";
            gerichteAlle = new List<string>();
            Kopie = new List<Essen>();
            jessyChecked = false;
            /////////////////////////////////////////////////////////
            duration = 3; // Sekunden ändern                    ////
            /////////////////////////////////////////////////////////
            IsAusgewählt = false;
            aufwandChecked = false;
            timer = Application.Current.Dispatcher.CreateTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(duration);
            timer.Start();

            EssensListeFiltered = new List<Essen>();
            if (!File.Exists(pathFileIgno))
                    {
                        var file = File.Create(pathFileIgno);
                        file.Close();
                    }

            reneChecked = false;
            zufall = new Random();
            Ignorlist = new List<Essen>();
            LoadIgnorliste();
            
            MagJessyCommand = new Command(MagJessy_Execute);
            Init();
        }

        private void AuswählenExecute()
        {
            essen.TimeStamp = DateTime.Now;
            Ignorlist.Add(essen);
            CheckConditions();
            
            if (glo_Va.EssensListeAktuell.Count > 0)
                using (var fs = new FileStream(pathFileIgno, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    var sw = new StreamWriter(fs);
                    sw.WriteLine(essen.Name);
                    sw.WriteLine(essen.TimeStamp.ToString(CultureInfo.InvariantCulture));
                    sw.Close();
                    fs.Close();
                }

            foreach (var essen_for in glo_Va.EssensListeAktuell)
                if (essen_for.Name == lblHauptAnzeige.Text)
                {
                    glo_Va.EssensListeAktuell.Remove(essen_for);
                    break;
                }

            Thread.Sleep(10);
            IsAusgewählt = true;

            #region AnzeigeGedöns_Auswählen

            LblIgnorliste.Text = "Ignorliste: ";
            for (var i = 0; i < Ignorlist.Count; i++) LblIgnorliste.Text += Ignorlist[i].Name + komma;

            LblEssensliste.Text = "Essensliste: ";
            for (var i = 0; i < glo_Va.EssensListeAktuell.Count; i++) LblEssensliste.Text += glo_Va.EssensListeAktuell[i].Name + komma;

            if (glo_Va.EssensListeAktuell.Count <= 0)
            {
                BtnAuswählen.IsEnabled = false;
                BtnNeuerVorschlag.IsEnabled = false;
            }
            else
            {
                BtnAuswählen.IsEnabled = true;
                BtnNeuerVorschlag.IsEnabled = true;
            }

            if (glo_Va.EssensListeAktuell.Count == 0)
            {
                lblHauptAnzeige.Text = "Musst hungern heute!!! HA HA";
                LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + glo_Va.EssensListeAktuell.Count;
                return;
            }

            GenerateDisplay();
            lblHauptAnzeige.Text = essen.Name;

            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + glo_Va.EssensListeAktuell.Count;

            #endregion
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (IsAusgewählt)
            {
                IsAusgewählt = false;
                return;
            }

            if (Ignorlist.Count > 0)
            {
                for (var i = 0; i < Ignorlist.Count; i++)
                {
                    if (DateTime.Now > Ignorlist[i].TimeStamp.AddSeconds(duration))
                    {
                        glo_Va.EssensListeAktuell.Add(Ignorlist[i]);
                        Ignorlist.Remove(Ignorlist[i]);
                        löschen = true;
                    }
                }
            }

            Thread.Sleep(100);

            if (Ignorlist.Count >= 0 && löschen)
            {

                using (var fs = new FileStream(pathFileIgno, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)) // TXT neu erstellen mit aktualisierter Ignolist 
                {
                    var sw = new StreamWriter(fs);
                    for (var i = 0; i < Ignorlist.Count; i++)
                    {
                        if (Ignorlist.Count == 0)
                            sw.WriteLine(string.Empty);
                        sw.WriteLine(Ignorlist[i].Name);
                        sw.WriteLine(Ignorlist[i].TimeStamp.ToString());
                    }
                    sw.Close();
                    fs.Close();
                }
                löschen = false;
            }
            IsAusgewählt = false;

            #region AnzeigeGedöns_Tick

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            LblTimeStamp.Text = "Now: " + DateTime.Now + timeStampAnzeigetemp;
            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + glo_Va.EssensListeAktuell.Count;
            LblIgnorliste.Text = "Ignorliste: ";

            for (var i = 0; i < Ignorlist.Count; i++) 
                LblIgnorliste.Text += Ignorlist[i].Name + komma;

            LblEssensliste.Text = "Essensliste: ";
            for (var i = 0; i < glo_Va.EssensListeAktuell.Count; i++) LblEssensliste.Text += glo_Va.EssensListeAktuell[i].Name + komma;

            if (lblHauptAnzeige.Text == "")
            {
                BtnAuswählen.IsEnabled = false;
            }
            else
            {
                BtnAuswählen.IsEnabled = true;
                BtnNeuerVorschlag.IsEnabled = true;
            }

            #endregion
        }

        private void LoadIgnorliste()
        {
            var dateTimeConverter = new DateTimeConverter();
            var content = new Essen();
            using (var fs = new FileStream(pathFileIgno, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    content.Name = sr.ReadLine();
                    content.TimeStamp = DateTime.ParseExact(sr.ReadLine(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    LblIgnorliste.Text += content.Name + komma;
                    Ignorlist.Add(content);
                }
            }
        }

        public void Init()
        {
            string stream = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Essensliste.txt");
            var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
                gerichteAlle.Add(reader.ReadLine());
            reader.Close();

            for (var i = 0; i < gerichteAlle.Count; i++)
            {
                essen = new Essen();
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
                }

                glo_Va.EssensListeAktuell.Add(essen);
                Kopie.Add(essen);
            }


            for (var i = 0; i < Ignorlist.Count; i++)
                glo_Va.EssensListeAktuell.Remove(Ignorlist[i]);
            //glo_Va.EssensListeAktuell.RemoveAll(x => x.Name == Ignorlist[i].Name);

            for (var i = 0; i < glo_Va.EssensListeAktuell.Count; i++)
                LblEssensliste.Text += glo_Va.EssensListeAktuell[i].Name + komma;

            if (glo_Va.EssensListeAktuell.Count > 0)
            {
                GenerateDisplay();
                lblHauptAnzeige.Text = essen.Name;
            }

            else
            {
                lblHauptAnzeige.Text = "Musst hungern heute!!! HA HA";
            }

            if (glo_Va.EssensListeAktuell.Count < 0)
            {
                BtnAuswählen.IsEnabled = false;
                BtnNeuerVorschlag.IsEnabled = false;
            }
            else
            {
                BtnAuswählen.IsEnabled = true;
                BtnNeuerVorschlag.IsEnabled = true;
            }

            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + glo_Va.EssensListeAktuell.Count;
        }

        public void NextAdviceExecute()
        {
            CheckConditions();
            GenerateDisplay();

            if (glo_Va.EssensListeAktuell.Count == 0)
            {
                lblHauptAnzeige.Text = "Musst hungern heute!!! HA HA";
            }
            else
            {
                lblHauptAnzeige.Text = essen.Name;
            }

            if (lblHauptAnzeige.Text != "")
                BtnAuswählen.IsEnabled = true;
        }

        private void BtnResetIgnorliste_Clicked(object sender, EventArgs e)
        {
            var komma = ", ";
            File.WriteAllText(pathFileIgno, string.Empty); // Reset IgnoFile
            Ignorlist.RemoveRange(0, Ignorlist.Count); // IgnoListe löschen
            LblIgnorliste.Text = "Ignorliste: ";
            for (var i = 0; i < Ignorlist.Count; i++) LblIgnorliste.Text += Ignorlist[i] + komma;

            glo_Va.EssensListeAktuell.Clear();
            for (var i = 0; i < Kopie.Count; i++) glo_Va.EssensListeAktuell.Add(Kopie[i]);

            LblEssensliste.Text = "Essensliste: ";
            for (var i = 0; i < glo_Va.EssensListeAktuell.Count; i++) LblEssensliste.Text += glo_Va.EssensListeAktuell[i].Name + komma;

            BtnNeuerVorschlag.IsEnabled = true;
            BtnAuswählen.IsEnabled = true;

            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + glo_Va.EssensListeAktuell.Count;
            lblHauptAnzeige.Text = "";

            if (lblHauptAnzeige.Text == "")
                BtnAuswählen.IsEnabled = false;
        }

        private void GenerateDisplay()
        {
            if (EssensListeFiltered.Count > 0)
                essen = EssensListeFiltered[zufall.Next(0, EssensListeFiltered.Count)];
            else if (glo_Va.EssensListeAktuell.Count > 0)
                essen = glo_Va.EssensListeAktuell[zufall.Next(0, glo_Va.EssensListeAktuell.Count)];
            else
                lblHauptAnzeige.Text = "Liste leer!";
        }

        private void MagRene_Execute()
        {
            if (!reneChecked)
            {
                BtnMagRene.BackgroundColor = Colors.Green;
                reneChecked = !reneChecked;
                BtnMagRene.TextColor = Colors.Black;
                BtnMagRene.Text = "Mag René";
                BtnMagRene.BorderColor = Colors.Black;

                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.MagRene == true).ToList();
            }

            else
            {
                reneChecked = !reneChecked;
                BtnMagRene.BackgroundColor = Colors.White;
                BtnMagRene.TextColor = Colors.Gray;
                BtnMagRene.BorderColor = Colors.Gray;
                BtnMagRene.Text = "Mag René";
            }
        }

        private void MagJessy_Execute()
        {
            if (!jessyChecked)
            {
                BtnMagJessy.BackgroundColor = Colors.Green;
                jessyChecked = !jessyChecked;
                BtnMagJessy.TextColor = Colors.Black;
                BtnMagJessy.Text = "Mag Jessy";
                BtnMagJessy.BorderColor = Colors.Black;
                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.MagJessy == true).ToList();
            }
            else
            {
                jessyChecked = !jessyChecked;
                BtnMagJessy.BackgroundColor = Colors.White;
                BtnMagJessy.TextColor = Colors.Gray;
                BtnMagJessy.BorderColor = Colors.Gray;
                BtnMagJessy.Text = "Mag Jessy";
            }
        }

        private void IsAufwaendig_Execute()
        {
            if (!aufwandChecked)
            {
                aufwandChecked = !aufwandChecked;
                BtnIsAufwaendig.BackgroundColor = Colors.Green;
                BtnIsAufwaendig.TextColor = Colors.Black;
                BtnIsAufwaendig.BorderColor = Colors.Black;
                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.IsAufwändig == true).ToList();
            }

            else
            {
                aufwandChecked = !aufwandChecked;
                BtnIsAufwaendig.BackgroundColor = Colors.White;
                BtnIsAufwaendig.TextColor = Colors.Gray;
                BtnIsAufwaendig.BorderColor = Colors.Gray;
            }
        }

        public void CheckConditions()
        {
            if (BtnIsAufwaendig.BackgroundColor == Colors.Green)
                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.IsAufwändig == true).ToList();
            else if (BtnMagJessy.BackgroundColor == Colors.Green)
                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.MagJessy == true).ToList();
            else if (BtnMagRene.BackgroundColor == Colors.Green)
                EssensListeFiltered = glo_Va.EssensListeAktuell.Where(e => e.MagRene == true).ToList();
            else EssensListeFiltered.Clear();

        }

        //public void PageMainPageNavigationExecute()
        //{
        //    navigationService.GoToMainPage(nameof(MainPage), new Dictionary<string, object>() { { "EssenslisteAktuell", glo_Va.EssensListeAktuell } });
        //}
        
        [RelayCommand]
        public void PageEssenslisteBearbeitenNavigationExecute()
        {
            navigationService.GoToEssenslisteBearbeiten(nameof(EssenslisteBearbeiten), new Dictionary<string, object>() { { "EssenslisteAktuell", glo_Va.EssensListeAktuell } });
        }
    }
}
