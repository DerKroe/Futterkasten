using System.Globalization;
using Futterkasten.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Futterkasten.ViewModel
{
    partial class MainPageViewModel : ObservableObject
    {
        private readonly double duration;
        private readonly List<string> gerichteAlle;
        private readonly List<Essen> Ignorlist;
        private const string komma = ", ";
        private bool löschen;
        private readonly string pathFileIgno = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Ignorliste.txt");
        private readonly string str_essensliste = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Essensliste.txt");
        private readonly Random zufall;
        private Essen essen; // Aktuell angezeigtes Essen
        private string listeLeerText = "Keine Einträge vorhanden";
        private bool IsAusgewählt_Btn_gedrückt; 
        private readonly string timeStampAnzeigetemp; // Debug-Zeitanzeige
        private List<Essen>? EssensListeFiltered; // Liste mit Filterung
        private readonly IDispatcherTimer timer;
        public static ObservableCollection<Essen> EssensListeAktuell;
        #region Variablen

        [ObservableProperty] private int countEssensliste;
        [ObservableProperty] private Button btnAuswählen;
        [ObservableProperty] private Label lblHauptAnzeige;
        [ObservableProperty] private Label lblIgnorList;
        [ObservableProperty] private Label lblEssensliste;
        [ObservableProperty] private Button btnNeuerVorschlag;
        [ObservableProperty] private Label lblEssenslisteAktuellCount;
        [ObservableProperty] private Label lblTimeStamp;
        [ObservableProperty] private int intFilteredListeCount;
        [ObservableProperty] private bool cbIsChecked_MagRene;
        [ObservableProperty] private bool cbIsChecked_MagJessy;
        [ObservableProperty] private bool cbIsChecked_IstAufwendig;
        [ObservableProperty] private Color horizontal_BackgroundColor;
        [ObservableProperty] private bool cbIsChecked_JessysInsuFood;

        #endregion
        public MainPageViewModel()
        {
            gerichteAlle = new List<string>();
            Load_Essensliste();
            timeStampAnzeigetemp = "";

            /////////////////////////////////////////////////////////
            duration = 3; // Sekunden ändern                    ////
            ///////////////////////////////////////////////////////// 
            IsAusgewählt_Btn_gedrückt = false;

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

            Horizontal_BackgroundColor = Colors.LightGray;
            zufall = new Random();
            Ignorlist = new List<Essen>();
            lblEssensliste = new Label();
            lblHauptAnzeige = new Label();
            btnAuswählen = new Button();
            btnNeuerVorschlag = new Button();
            lblEssenslisteAktuellCount = new Label();
            lblTimeStamp = new Label();
            lblIgnorList = new Label();
            Load_Ignorliste();
            GenerateDisplay();
            ShowDebugStuff();
        }

        //private void Save()
        //{
        //    essen.TimeStamp = DateTime.Now;
        //    Ignorlist.Add(essen);

        //    if (EssensListeAktuell.Count > 0)
        //        using (var fs = new FileStream(pathFileIgno, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        //        {
        //            var sw = new StreamWriter(fs);
        //            sw.WriteLine(essen.Name);
        //            sw.WriteLine(essen.TimeStamp.ToString(CultureInfo.CurrentCulture));
        //            sw.Close();
        //            fs.Close();
        //        }

        //    foreach (var essen_for in EssensListeAktuell)
        //        if (essen_for.Name == LblHauptAnzeige.Text)
        //        {
        //            EssensListeAktuell.Remove(essen_for);
        //            break;
        //        }
        //}

        [RelayCommand]
        private void Auswählen()
        {
            
            essen.TimeStamp = DateTime.Now;
            Ignorlist.Add(essen);

            if (EssensListeAktuell.Count > 0)
                using (var fs = new FileStream(pathFileIgno, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    var sw = new StreamWriter(fs);
                    sw.WriteLine(essen.Name);
                    sw.WriteLine(essen.TimeStamp.ToString(CultureInfo.CurrentCulture));
                    sw.Close();
                    fs.Close();
                }
            
            foreach (var essen_for in EssensListeAktuell)
                if (essen_for.Name == LblHauptAnzeige.Text)
                {
                    EssensListeAktuell.Remove(essen_for);
                    break;
                }

            Thread.Sleep(10);
            IsAusgewählt_Btn_gedrückt = true;

            #region AnzeigeGedöns_Auswählen

            LblIgnorList.Text = "Ignorliste: ";
            for (var i = 0; i < Ignorlist.Count; i++) LblIgnorList.Text += Ignorlist[i].Name + komma;

            LblEssensliste.Text = "Essensliste: ";
            for (var i = 0; i < EssensListeAktuell.Count; i++) LblEssensliste.Text += EssensListeAktuell[i].Name + komma;

            if (EssensListeAktuell.Count <= 0)
            {
                BtnAuswählen.IsEnabled = false;
                BtnNeuerVorschlag.IsEnabled = false;
            } //Button aktiv/deaktiv

            else
            {
                BtnAuswählen.IsEnabled = true;
                BtnNeuerVorschlag.IsEnabled = true;
            }//Button aktiv/deaktiv

            if (EssensListeAktuell.Count == 0)
            {
                LblHauptAnzeige.Text = listeLeerText;
                LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + EssensListeAktuell.Count;
                return;
            } // wenn Liste leer, dann ...

            GenerateDisplay();
            LblHauptAnzeige.Text = essen.Name;

            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + EssensListeAktuell.Count;

            #endregion
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (EssensListeAktuell.Count != gerichteAlle.Count && GlobalVariables.fileChanged == true)   //??????  -1 Fehler in Ausgabe Liste EssenBearbeitenPage
            {
                Load_Essensliste();
            }

            if (IsAusgewählt_Btn_gedrückt)
            {
                IsAusgewählt_Btn_gedrückt = false;
                return;
            }

            if (Ignorlist.Count > 0)
            {
                for (var i = 0; i < Ignorlist.Count; i++)
                {
                    if (DateTime.Now > Ignorlist[i].TimeStamp.AddSeconds(duration))
                    {
                        EssensListeAktuell.Add(Ignorlist[i]);
                        Ignorlist.Remove(Ignorlist[i]);
                        löschen = true;
                    }
                }
            }

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
            IsAusgewählt_Btn_gedrückt = false;

            ShowDebugStuff();
        }

        private void Load_Ignorliste()
        {
            var content = new Essen();
            using (var fs = new FileStream(pathFileIgno, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    content.Name = sr.ReadLine();
                    content.TimeStamp = DateTime.Parse(sr.ReadLine());//, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    LblIgnorList.Text += content.Name + komma;
                    Ignorlist.Add(content);
                }
            }
        }

        public void Load_Essensliste()
        {
            gerichteAlle.Clear();

            if (EssensListeAktuell == null)
                EssensListeAktuell = new ObservableCollection<Essen>();
            EssensListeAktuell.Clear();
            if (!File.Exists(str_essensliste))
            {
                var file = File.Create(str_essensliste);
                file.Close();
            }
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
                    if(j == 4 && values[j] == " +") essen.JessysInsuFood = true;
                    else if (j == 4 && values[j] == " -") essen.JessysInsuFood = false;
                }

                EssensListeAktuell.Add(essen);
            }
            GlobalVariables.fileChanged = false;
        }
        private void ShowDebugStuff() // unwichtig ... Debug-Daten anzeigen und aktualisieren lassen
        {
            for (var i = 0; i < Ignorlist.Count; i++) // IgnoList-Elemente aus Essensliste entfernen .... DEBUG
                EssensListeAktuell.Remove(Ignorlist[i]);

            for (var i = 0; i < EssensListeAktuell.Count; i++) // Anzeige Essensliste in UI .... DEBUG
                LblEssensliste.Text += EssensListeAktuell[i].Name + komma;

            LblTimeStamp.Text = "Now: " + DateTime.Now + timeStampAnzeigetemp;
            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + EssensListeAktuell.Count;
            LblIgnorList.Text = "Ignorliste: ";

            for (var i = 0; i < Ignorlist.Count; i++)
                LblIgnorList.Text += Ignorlist[i].Name + komma;

            LblEssensliste.Text = "Essensliste: ";
            for (var i = 0; i < EssensListeAktuell.Count; i++) LblEssensliste.Text += EssensListeAktuell[i].Name + komma;

            if (LblHauptAnzeige.Text == "")
            {
                BtnAuswählen.IsEnabled = false;
            }
            else
            {
                BtnAuswählen.IsEnabled = true;
                BtnNeuerVorschlag.IsEnabled = true;
            }
            //if (EssensListeAktuell.Count < 0)
            //{
            //    BtnAuswählen.IsEnabled = false;
            //    BtnNeuerVorschlag.IsEnabled = false;
            //}

            //else
            //{
            //    BtnAuswählen.IsEnabled = true;
            //    BtnNeuerVorschlag.IsEnabled = true;
            //}

            LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + EssensListeAktuell.Count;
        }

        [RelayCommand]
        public void NextAdvice()
        {
            Load_Essensliste();
            GenerateDisplay();
            CheckBoxCheck();

            if (EssensListeAktuell.Count == 0)
                LblHauptAnzeige.Text = listeLeerText;

            else
                LblHauptAnzeige.Text = essen.Name;

            if (LblHauptAnzeige.Text != "")
                BtnAuswählen.IsEnabled = true;
        }

        //private void BtnResetIgnorliste_Clicked(object sender, EventArgs e)
        //{
        //    var komma = ", ";
        //    File.WriteAllText(pathFileIgno, string.Empty); // Reset IgnoFile
        //    Ignorlist.RemoveRange(0, Ignorlist.Count); // IgnoListe löschen
        //    LblIgnorList.Text = "Ignorliste: ";

        //    for (var i = 0; i < Ignorlist.Count; i++)
        //        LblIgnorList.Text += Ignorlist[i] + komma;

        //    _ = new ObservableCollection<Essen>();
        //    EssensListeAktuell.Clear();

        //    for (var i = 0; i < new ObservableCollection<Essen>().Count; i++)
        //        EssensListeAktuell.Add(new ObservableCollection<Essen>()[i]);

        //    LblEssensliste.Text = "Essensliste: ";
        //    for (var i = 0; i < EssensListeAktuell.Count; i++) LblEssensliste.Text += EssensListeAktuell[i].Name + komma;

        //    BtnNeuerVorschlag.IsEnabled = true;
        //    BtnAuswählen.IsEnabled = true;
        //    LblEssenslisteAktuellCount.Text = "E.ListeAkt.: " + EssensListeAktuell.Count;
        //    LblHauptAnzeige.Text = "";

        //    if (LblHauptAnzeige.Text == "")
        //        BtnAuswählen.IsEnabled = false;
        //}

        private void GenerateDisplay() // DisplayAnzeige
        {
        if (EssensListeAktuell.Count > 0)
            LblHauptAnzeige.Text = essen.Name;

            else LblHauptAnzeige.Text = listeLeerText;

            if (EssensListeFiltered?.Count > 0)
                essen = EssensListeFiltered[zufall.Next(0, EssensListeFiltered.Count)];
            else if (EssensListeAktuell.Count > 0)
                essen = EssensListeAktuell[zufall.Next(0, EssensListeAktuell.Count)];
            else
                LblHauptAnzeige.Text = listeLeerText;
        }

        private void CheckBoxCheck() 
        {   
            Horizontal_BackgroundColor = Colors.LightGray;
            EssensListeFiltered?.Clear();

            EssensListeFiltered = EssensListeAktuell.Where(x => x.MagRene == CbIsChecked_MagRene 
                                                           && x.MagJessy == CbIsChecked_MagJessy 
                                                           && x.IsAufwändig == CbIsChecked_IstAufwendig 
                                                           && x.JessysInsuFood == CbIsChecked_JessysInsuFood).ToList();


            if (EssensListeFiltered.Count == 0 && (  CbIsChecked_IstAufwendig 
                                                  || CbIsChecked_MagJessy 
                                                  || CbIsChecked_MagRene
                                                  || CbIsChecked_JessysInsuFood ))
            {
                essen.Name = "Keine Filterauswahl vorhanden";
                Horizontal_BackgroundColor = Colors.Red;
            }

            IntFilteredListeCount = EssensListeFiltered.Count;
        }
    }
}
