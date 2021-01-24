using Clock.Common;
using Clock.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using BackgroundAgent;
using Windows.UI.Notifications;
using Windows.System.UserProfile;
using Windows.Phone.Devices.Power;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using System.Diagnostics;
using Windows.Data.Xml.Dom;
using Windows.System.Profile;
using Windows.Storage.Streams;
using System.Net;
using System.ComponentModel;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;





// Namespace
namespace Clock
{




    // PivotPage // Startseite
    public sealed partial class PivotPage : Page
    {





        // Allgemeine Variablen
        // ----------------------------------------------------------------------------------------------------------------------
        // Storage Daten festlegen
        StorageFolder SF = ApplicationData.Current.LocalFolder;
        StorageFile file;

        // Liste erstellen
        ObservableCollection<ClassLanguages> List_Languages = new ObservableCollection<ClassLanguages>();

        // Variablen für den Hintergrundtask
        private const string TASKNAMEUSERPRESENT = "ClockTileTask";
        private const string TASKNAMETIMER = "ClockTileTimer";
        private const string TASKENTRYPOINT = "BackgroundAgent.EntryPoint";

        // Resource loader
        ResourceLoader Resource = new ResourceLoader();

        // Einstellungen
        int SetDesign = 0;
        bool SetBattery = true;
        bool SetCreateNew = false;

        // Variablen
        string AppVersion = "";
        DateTime FirstRunTime = DateTime.Now;
        bool FullVersion = false;
        bool Run = false;
        string String_Settings;
        bool MenuOpen = false;

        // CultureInfo laden
        string Language = "en-us";

        // Timer erstellen
        DispatcherTimer Timer_Settings = new DispatcherTimer();

        // Variablen für zweites Tile
        public const string appbarTileId = "MySecondaryTile";
        // ----------------------------------------------------------------------------------------------------------------------





        // Zweites Tile an Startseite Pinnen
        // ----------------------------------------------------------------------------------------------------------------------
        // Button Tile erstellen
        private void Pin_Click(object sender, PointerRoutedEventArgs e)
        {
            pinToAppBar_Click();
        }



        // Zweites Tile erstellen
        async void pinToAppBar_Click()
        {
            if (!SecondaryTile.Exists(PivotPage.appbarTileId))
            {
                // Pin
                Uri square150x150Logo = new Uri("ms-appx:///Images/FirstTileSquare.png");
                string tileActivationArguments = PivotPage.appbarTileId + " was pinned at = " + DateTime.Now.ToLocalTime().ToString();
                string displayName = "click";

                TileSize newTileDesiredSize = TileSize.Wide310x150;

                SecondaryTile secondaryTile = new SecondaryTile(PivotPage.appbarTileId,
                                                                displayName,
                                                                tileActivationArguments,
                                                                square150x150Logo,
                                                                newTileDesiredSize);

                secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Images/FirstTileWide.png");
                await secondaryTile.RequestCreateAsync();

                // App schließen
                Application.Current.Exit();
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Wird beim ersten öffnen der Seite ausgeführt
        // ----------------------------------------------------------------------------------------------------------------------
        public PivotPage()
        {
            // Sprache auswählen
            SelectLanguage();

            // Komponenten laden
            this.InitializeComponent();

            // StatusBar verschwinden lassen
            hideStatusBar();

            // Back Button festlegen
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            // Sprachen erstellen
            string[] LangCodes = { "ms-my", "ca-es", "cs-cz", "da-dk", "de-de", "et-ee", "en-us", "es-es", "es-mx", "fr-fr", "hr-hr", "it-it", "lt-lt", "lv-lv", "hu-hu", "nl-nl", "nb-no", "pl-pl", "pt-br", "pt-pt", "ro-ro", "fi-fi", "sk-sk", "sv-se", "vi-vn", "tr-tr", "el-gr", "be-by", "bg-bg", "ru-ru", "he-il", "fa-ir", "hi-in", "th-th", "ko-kr", "zh-cn", "zh-tw", "ja-jp", "uk-ua" };
            string[] LangNames = { "Behasa Melayu", "català", "Čeština", "dansk", "deutsch", "Eesti", "English", "español (España)", "Español (México)", "Français", "hrvatski", "italiano", "Lietuvių", "Latviešu", "magyar", "Nederlands", "norsk", "polski", "português (Brasil)", "português (Portugal)", "română", "suomi", "Slovenský", "Svenska", "Tiếng Việt", "Türkçe", "Ελληνικά", "Беларуска", "Български", "русский", "עברית", "فارسی", "हिंदी", "ไทย", "한국어", "简体中文", "繁體中文", "日本語", "Український" };
            // Sprachen durchlaufen
            for (int i = 0; i < LangCodes.Count(); i++)
            {
                // Sprachen in Liste schreiben
                List_Languages.Add(new ClassLanguages(LangNames[i], LangCodes[i], ""));
            }
            // Sprachen in Listbox schreiben
            LBLanguages.ItemsSource = List_Languages;

            // Sprache einstellen
            Language = GlobalizationPreferences.Languages.First();

            // Neue Sprachdatei erstellen
            CreateLanguageFile();

            // Bilder nach Farbe tauschen
            SolidColorBrush tempBgColor = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            string BgColor = tempBgColor.Color.ToString();
            if (BgColor == "#FFFFFFFF")
            {
                ImgLanguage.Source = new BitmapImage(new Uri("/Images/Globe.Light.png", UriKind.Relative));
                ImgLogo.Source = new BitmapImage(new Uri("/Images/Logo.Light.png", UriKind.Relative));
                imgArrow.Source = new BitmapImage(new Uri("/Images/Arrow.Light.png", UriKind.Relative));
                imgPin.Source = new BitmapImage(new Uri("/Images/Pin.Light.png", UriKind.Relative));
                //imgBrush.Source = new BitmapImage(new Uri("/Images/Brush.Light.png", UriKind.Relative));
                //imgBrushDelete.Source = new BitmapImage(new Uri("/Images/Delete.Light.png", UriKind.Relative));
            }

            // Timer Daten erstellen
            Timer_Settings.Interval = new TimeSpan(0, 0, 0, 0, 100);
            Timer_Settings.Tick += Timer_Settings_Tick;
            Timer_Settings.Stop();
        }



        // StatusBar verschwinden lassen
        private async void hideStatusBar()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            await statusBar.HideAsync();
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Sprache auswählen
        //---------------------------------------------------------------------------------------------------------
        public async void SelectLanguage()
        {
            // Default Sprache ermitteln
            var DefaultCulture = CultureInfo.DefaultThreadCurrentCulture;

            // Prüfen ob Sprachdatei besteht
            var F_Language = await SF.CreateFileAsync("Language.txt", CreationCollisionOption.OpenIfExists);
            var Temp_Language = await FileIO.ReadTextAsync(F_Language);
            // Prüfen ob Sprachdatei besteht
            Language = Convert.ToString(Temp_Language);
            // Wenn Sprachdatei noch nicht bestht
            if (Language.Length > 0)
            {
                // Sprache festlegen
                try
                {
                    var culture = new CultureInfo(Language);
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                }
                catch
                { }
            }
        }
        //---------------------------------------------------------------------------------------------------------

        
        


        // Wird beim jedem öffnen der Seite ausgeführt
        // ----------------------------------------------------------------------------------------------------------------------
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Prüfen ob Tile bereits gepinnt
            if (SecondaryTile.Exists(PivotPage.appbarTileId))
            {
                spPin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                spPin.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }



            // Ordner öffnen oder erstellen
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);



            // Prüfen ob "Settings/Version" besteht
            var F_Version = await SF_Settings.CreateFileAsync("Version.txt", CreationCollisionOption.OpenIfExists);
            var Temp_Version = await FileIO.ReadTextAsync(F_Version);
            // Prüfen ob Versionsdatei besteht
            AppVersion = Convert.ToString(Temp_Version);
            // Wenn Versionsdatei noch nicht bestht
            if (AppVersion.Length < 1)
            {
                // Version erstellen
                AppVersion = "001000040000";
                // FirstRunTime erstellen
                await FileIO.WriteTextAsync(F_Version, AppVersion);
            }



            // Prüfen ob "Settings/RateReminder" besteht
            var F_Reminder = await SF_Settings.CreateFileAsync("Reminder.txt", CreationCollisionOption.OpenIfExists);
            var Temp_Reminder = await FileIO.ReadTextAsync(F_Reminder);
            // Prüfen ob Reminder besteht
            string String_Reminder = Convert.ToString(Temp_Reminder);
            // Wenn Reminder noch nicht bestht
            if (String_Reminder.Length < 1)
            {
                // Reminder erstellen
                DateTime DT = DateTime.Now;
                DT = DT.AddDays(4);
                String_Reminder = DT.Year + ";" + DT.Month + ";" + DT.Day + ";" + DT.Hour + ";" + DT.Minute;
                // Speichern
                await FileIO.WriteTextAsync(F_Reminder, String_Reminder);
            }
            // Reminder aufteilen
            string[] Split_Reminder = Regex.Split(String_Reminder, ";");
            // Versuchen Reminder in DateTime umzuwandeln
            try
            {
                // Reminder umwandeln
                DateTime DT_Reminder = new DateTime(Convert.ToInt32(Split_Reminder[0]), Convert.ToInt32(Split_Reminder[1]), Convert.ToInt32(Split_Reminder[2]), Convert.ToInt32(Split_Reminder[3]), Convert.ToInt32(Split_Reminder[4]), 0);
                //Prüfen of Benachrichtigung ausgegeben wird
                DateTime DT_Now = DateTime.Now;
                int result = DateTime.Compare(DT_Reminder, DT_Now);
                if (result < 0)
                {
                    //Bewertung öffnen
                    GrRate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    MenuOpen = true;
                }
            }
            catch
            { }



            // Prüfen ob "Settings/FirstRunTime" besteht
            var F_FirstRunTime = await SF_Settings.CreateFileAsync("FirstRunTime.txt", CreationCollisionOption.OpenIfExists);
            var Temp_FirstRunTime = await FileIO.ReadTextAsync(F_FirstRunTime);
            // Prüfen ob FirstRunTime besteht
            string Temp_String = Convert.ToString(Temp_FirstRunTime);
            // Wenn FirstRunTime besteht
            if (Temp_String.Length > 0)
            {
                // DateTime aufteilen
                string[] Split_Temp_String = Regex.Split(Temp_String, ":");
                // FirstRunTime in DateTime umwadeln
                FirstRunTime = new DateTime(Convert.ToInt32(Split_Temp_String[0]), Convert.ToInt32(Split_Temp_String[1]), Convert.ToInt32(Split_Temp_String[2]), Convert.ToInt32(Split_Temp_String[3]), Convert.ToInt32(Split_Temp_String[4]), 0);
            }
            // Wenn FirstRunTime nicht besteht
            else
            {
                // FirstRunTime erstellen
                await FileIO.WriteTextAsync(F_FirstRunTime, Convert.ToString(DateTime.Now.Year) + ":" + Convert.ToString(DateTime.Now.Month) + ":" + Convert.ToString(DateTime.Now.Day) + ":" + Convert.ToString(DateTime.Now.Hour) + ":" + Convert.ToString(DateTime.Now.Minute));
            }



            // Prüfen ob "Settings/FulVersion" besteht
            var F_FullVersion = await SF_Settings.CreateFileAsync("FullVersion.txt", CreationCollisionOption.OpenIfExists);
            var Temp_FullVersion = await FileIO.ReadTextAsync(F_FullVersion);
            // Prüfen ob FullVErsion besteht
            Temp_String = Convert.ToString(Temp_FullVersion);
            // Wenn FullVersion besteht
            if (Temp_String.Length > 0)
            {
                // FullVersin in Bool umwandeln
                FullVersion = Convert.ToBoolean(Temp_String);
            }
            // Wenn FullVersion nicht besteht
            else
            {
                // FullVersion erstellen
                await FileIO.WriteTextAsync(F_FullVersion, "False");
            }



            // Wenn App gerade gekauft wurde
            if (FullVersion == false)
            {
                // Prüfen ob Vollversion vorhanden
                LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
                if (licenseInformation.IsActive)
                {
                    // Wenn Vollversion vorhanden
                    if (licenseInformation.IsTrial == false)
                    {
                        // Vollversion erstellen
                        FullVersion = true;
                        await FileIO.WriteTextAsync(F_FullVersion, "True");
                    }
                }
            }
            // Bei Volversion
            if (FullVersion == true)
            {
                // Button Buy verschwinden lassen
                SPTrial.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            // Bei Testversion
            else
            {
                // Trial zeit ermitteln
                DateTime NowTime = DateTime.Now;
                DateTime ExpiredTime = FirstRunTime.AddDays(2);
                // Wenn Zeit abgelaufen ist
                if (NowTime > ExpiredTime)
                {
                    // Benachrichtigung ausgeben das Testzeit abgelaufen
                    CreateNotificationMessage(Resource.GetString("002_PeriodExpired"));
                    // Angeben das Zeit abgelaufen ist
                    TBTrialTime.Text = Resource.GetString("002_PeriodExpired");
                }
                // Wenn Zeit noch nicht abgelaufen ist
                else
                {
                    // Restliche Zeit errechnen
                    TimeSpan RestTime = ExpiredTime - NowTime;
                    // Restliche Zeit ausgeben
                    string StringRestTime = "";
                    if (RestTime.Days == 1)
                    {
                        StringRestTime += "1 " + Resource.GetString("002_Day") + " ";
                    }
                    else
                    {
                        StringRestTime += RestTime.Days + " " + Resource.GetString("002_Days") + " ";
                    }
                    if (RestTime.Hours == 1)
                    {
                        StringRestTime += "1 " + Resource.GetString("002_Hour") + " ";
                    }
                    else
                    {
                        StringRestTime += RestTime.Hours + " " + Resource.GetString("002_Hours") + " ";
                    }
                    if (RestTime.Minutes == 1)
                    {
                        StringRestTime += "1 " + Resource.GetString("002_Minute") + " ";
                    }
                    else
                    {
                        StringRestTime += RestTime.Minutes + " " + Resource.GetString("002_Minutes") + " ";
                    }
                    TBTrialTime.Text = StringRestTime;
                    // Angeben das App ausgeführt wird
                    Run = true;
                }
                // Versuchen Preis zu ermitteln
                try
                {
                    ListingInformation listing = await CurrentApp.LoadListingInformationAsync();
                    //TBPrice.Text = listing.FormattedPrice;
                }
                catch (Exception)
                {
                    TBPrice.Text = "";
                }
            }



            // Prüfen of "Settings/Settings" besteht
            var F_Settings = await SF_Settings.CreateFileAsync("Settings.dat", CreationCollisionOption.OpenIfExists);
            var Temp_Settings = await FileIO.ReadTextAsync(F_Settings);
            // Prüfen ob Settings besteht
            String_Settings = Convert.ToString(Temp_Settings);
            //  Wenn Settings nicht besteht
            if (String_Settings.Length < 1)
            {
                // Einstellungen erstellen
                CreateSettings();
            }
            //Einstellungen umsetzen
            string[] Split_Settings = Regex.Split(String_Settings, ";");
            // Einstellungen durchlaufen
            for (int i = 0; i < Split_Settings.Count(); i++)
            {
                // Einzelne Einstellung zerlegen
                string[] Split_Setting = Regex.Split(Split_Settings[i], "=");
                // Wenn Einstellung "Set_Folder"
                if (Split_Setting[0] == "SetDesign")
                {
                    SetDesign = Convert.ToInt32(Split_Setting[1]);
                    if (SetDesign == 0)
                    {
                        SpDesign0.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    }
                    else if (SetDesign == 1)
                    {
                        SpDesign1.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    }
                    else
                    {
                        SpDesign2.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    }
                }
                // Wenn Einstellung "SetBattery"
                if (Split_Setting[0] == "SetBattery")
                {
                    SetBattery = Convert.ToBoolean(Split_Setting[1]);
                    if (SetBattery == true)
                    {
                        BtnBatteryStatus.Content = Resource.GetString("002_Yes");
                    }
                    else
                    {
                        BtnBatteryStatus.Content = Resource.GetString("002_No");
                    }
                }
            }

            // Clock Tile Task erstellen
            CreateClockTask();

            // Disigns in der UI erstellen
            CreateDesigns();

            // Neuen Task erstellen
            TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
            GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Timer_Settings_Action = "CreateTile";
            Timer_Settings.Start();
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Hintergrundtask erstellen
        // ----------------------------------------------------------------------------------------------------------------------
        private static async void CreateClockTask()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();
            if (result == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                result == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                EnsureUserPresentTask();
                EnsureTimerTask();
            }
        }

        private static void EnsureUserPresentTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
                if (task.Value.Name == TASKNAMEUSERPRESENT)
                    return;
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = TASKNAMEUSERPRESENT;
            builder.TaskEntryPoint = TASKENTRYPOINT;
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
            builder.Register();
        }

        private static void EnsureTimerTask()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
                if (task.Value.Name == TASKNAMETIMER)
                    return;
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = TASKNAMETIMER;
            builder.TaskEntryPoint = TASKENTRYPOINT;
            builder.SetTrigger(new TimeTrigger(30, false));
            builder.Register();
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Einstellungen neu erstellen
        // ----------------------------------------------------------------------------------------------------------------------
        private async void CreateSettings()
        {
            // Settings String erstellen
            String_Settings = "SetDesign=" + SetDesign.ToString() + ";SetBattery=" + SetBattery.ToString() + ";SetCreateNew=" + SetCreateNew.ToString();

            //Einstellungen speichern
            try
            {
                var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
                var F_Settings = await SF_Settings.CreateFileAsync("Settings.dat", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(F_Settings, String_Settings);
            }
            catch
            {

            }
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Neue Sprachdatei erstellen
        // ----------------------------------------------------------------------------------------------------------------------
        private async void CreateLanguageFile()
        {
            // LanguageString zusammenfügen
            string LanguageString = "Language=" + Language + ";Day=" + Resource.GetString("002_Day") + ";Days=" + Resource.GetString("002_Days") + ";Hour=" + Resource.GetString("002_Hour") + ";Hours=" + Resource.GetString("002_Hours") + ";Minute=" + Resource.GetString("002_Minute") + ";Minutes=" + Resource.GetString("002_Minutes") + ";IsLoading=" + Resource.GetString("002_IsLoading") + ";NameBattery=" + Resource.GetString("002_NameBattery") + ";";

            // Ordner öffnen oder erstellen
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
            // Prüfen ob "LanguageString" besteht
            var F_LanguageString = await SF_Settings.CreateFileAsync("LanguageString.dat", CreationCollisionOption.OpenIfExists);
            // FirstRunTime erstellen
            await FileIO.WriteTextAsync(F_LanguageString, LanguageString);
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Designs erstellen
        // ----------------------------------------------------------------------------------------------------------------------
        private void CreateDesigns()
        {
            // CultureInfo laden
            CultureInfo cultureInfo = new CultureInfo(Language);

            // Batterieleistung ausgeben
            var BatteryInfo = Battery.GetDefault();
            TimeSpan BatteryTime = BatteryInfo.RemainingDischargeTime;
            string BatteryString = "";

            // Aktueller DateTime erstellen und Sekunden auf 0 stellen
            DateTime now = DateTime.Now;



            // Design 0 // TileSquareText02 // TileWideText01
            // Strings für die Tiles erstellen
            string Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
            string Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
            string Str2 = "";
            if (SetBattery == true)
            {
                // String der Batterieanzeige ändern
                BatteryString = "";
                if (BatteryTime.Days > 100)
                {
                    BatteryString = Resource.GetString("002_IsLoading");
                }
                else
                {
                    if (BatteryTime.Days > 0)
                    {
                        if (BatteryTime.Days == 1)
                        {
                            BatteryString = "1 " + Resource.GetString("002_Day") + " ";
                        }
                        else
                        {
                            BatteryString = BatteryTime.Days.ToString() + " " + Resource.GetString("002_Days") + " ";
                        }
                    }
                    if (BatteryTime.Hours == 1)
                    {
                        BatteryString += "1 " + Resource.GetString("002_Hour") + " ";
                    }
                    else
                    {
                        BatteryString += BatteryTime.Hours + " " + Resource.GetString("002_Hours") + " ";
                    }
                    if (BatteryTime.Minutes == 1)
                    {
                        BatteryString += "1 " + Resource.GetString("002_Minute") + " ";
                    }
                    else
                    {
                        BatteryString += BatteryTime.Minutes + " " + Resource.GetString("002_Minutes") + " ";
                    }
                }
                Str1 += "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                Str2 = BatteryInfo.RemainingChargePercent + "% " + BatteryString;
            }
            // Strings in Tiles ausgeben
            TbDesign0_Medium_Line1.Text = Str0;
            TbDesign0_Medium_Line2.Text = Str1;
            TbDesign0_Big_Line1.Text = Str0;
            TbDesign0_Big_Line2.Text = Str1;



            // Design 1 // TileSquareText02 // TileWideText03
            // Strings für die Tiles erstellen
            Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
            Str1 = "";
            Str2 = "";
            if (SetBattery == true)
            {
                // String der Batteriezeit verändern
                if (BatteryTime.Days > 100)
                {
                    BatteryString = Resource.GetString("002_IsLoading");
                }
                else
                {
                    BatteryString = ((BatteryTime.Days * 24) + BatteryTime.Hours).ToString() + ":" + BatteryTime.Minutes;
                }
                Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern) + "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                Str2 = Str0 + "\r\n" + Str1;
            }
            else
            {
                Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
                Str2 = Str0 + "\r\n" + Str1;
            }
            TbDesign1_Medium_Line1.Text = Str0;
            TbDesign1_Medium_Line2.Text = Str1;
            TbDesign1_Big_Line1.Text = Str2;



            // Strings für die Tiles erstellen
            // Design 2 // TileSquareText02 // TileWideBlockAndText01
            // Tag festlegen
            int DW = 0;
            if (now.DayOfWeek.ToString() == "Monday")
            {
                DW = 1;
            }
            else if (now.DayOfWeek.ToString() == "Tuesday")
            {
                DW = 2;
            }
            else if (now.DayOfWeek.ToString() == "Wednesday")
            {
                DW = 3;
            }
            else if (now.DayOfWeek.ToString() == "Thursday")
            {
                DW = 4;
            }
            else if (now.DayOfWeek.ToString() == "Friday")
            {
                DW = 5;
            }
            else if (now.DayOfWeek.ToString() == "Saturday")
            {
                DW = 6;
            }
            Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
            Str1 = cultureInfo.DateTimeFormat.DayNames[DW] + "  ";
            Str2 = now.ToString(cultureInfo.DateTimeFormat.MonthDayPattern);
            string Str3 = "";
            string Str4 = "";
            if (SetBattery == true)
            {
                // Batterie Prozenz anzeigen
                Str3 = Resource.GetString("002_NameBattery") + " " + BatteryInfo.RemainingChargePercent + "%";
                // String der Batteriezeit verändern
                if (BatteryTime.Days > 100)
                {
                    BatteryString = Resource.GetString("002_IsLoading");
                }
                else
                {
                    if (BatteryTime.Days > 0)
                    {
                        if (BatteryTime.Days == 1)
                        {
                            BatteryString = "1 " + Resource.GetString("002_Day") + " ";
                        }
                        else
                        {
                            BatteryString = BatteryTime.Days.ToString() + " " + Resource.GetString("002_Days") + " ";
                        }
                    }
                    if (BatteryTime.Hours == 1)
                    {
                        BatteryString += "1 " + Resource.GetString("002_Hour") + " ";
                    }
                    else
                    {
                        BatteryString += BatteryTime.Hours + " " + Resource.GetString("002_Hours") + " ";
                    }
                    if (BatteryTime.Minutes == 1)
                    {
                        BatteryString += "1 " + Resource.GetString("002_Minute") + " ";
                    }
                    else
                    {
                        BatteryString += BatteryTime.Minutes + " " + Resource.GetString("002_Minutes") + " ";
                    }
                }
                Str4 = BatteryString;
            }
            string Str5 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
            string Str6 = "";
            string[] Split5 = Regex.Split(Str5, " ");
            if (Split5.Count() > 1)
            {
                Str5 = Split5[0].Trim();
                Str6 = Split5[1].Trim();
            }
            // Ausgeben
            TbDesign2_Medium_Line1.Text = Str6;
            TbDesign2_Medium_Line2.Text = Str5;
            TbDesign2_Big_Line1.Text = Str1;
            TbDesign2_Big_Line2.Text = Str0;
            TbDesign2_Big_Line3.Text = Str2;
            TbDesign2_Big_Line4.Text = Str3;
            TbDesign2_Big_Line5.Text = Str4;
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Einstellungen
        // ----------------------------------------------------------------------------------------------------------------------
        // Batterie Status
        private void BtnBatteryStatus_Click(object sender, RoutedEventArgs e)
        {
            if (SetBattery == false)
            {
                try
                {
                    SetBattery = true;
                    CreateSettings();
                    BtnBatteryStatus.Content = Resource.GetString("002_Yes");
                    TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Timer_Settings_Action = "CreateTile";
                    Timer_Settings.Start();
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    SetBattery = false;
                    CreateSettings();
                    BtnBatteryStatus.Content = Resource.GetString("002_No");
                    TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Timer_Settings_Action = "CreateTile";
                    Timer_Settings.Start();
                }
                catch
                {
                }
            }
            CreateDesigns();
        }


        // Designs
        private void Design0_Click(object sender, PointerRoutedEventArgs e)
        {
            if (SetDesign != 0)
            {
                SetDesign = 0;
                CreateSettings();
                // Clock Tile updaten
                try
                {
                    SolidColorBrush tempBgColor = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
                    SpDesign0.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    SpDesign1.Background = tempBgColor;
                    SpDesign2.Background = tempBgColor;
                    TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Timer_Settings_Action = "CreateTile";
                    Timer_Settings.Start();
                }
                catch
                {
                }
            }
        }
        private void Design1_Click(object sender, PointerRoutedEventArgs e)
        {
            if (SetDesign != 1)
            {
                SetDesign = 1;
                CreateSettings();
                // Clock Tile updaten
                try
                {
                    SolidColorBrush tempBgColor = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
                    SpDesign1.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    SpDesign0.Background = tempBgColor;
                    SpDesign2.Background = tempBgColor;
                    TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Timer_Settings_Action = "CreateTile";
                    Timer_Settings.Start();
                }
                catch
                {
                }
            }
        }
        private void Design2_Click(object sender, PointerRoutedEventArgs e)
        {
            if (SetDesign != 2)
            {
                SetDesign = 2;
                CreateSettings();
                // Clock Tile updaten
                try
                {
                    SolidColorBrush tempBgColor = App.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
                    SpDesign2.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                    SpDesign0.Background = tempBgColor;
                    SpDesign1.Background = tempBgColor;
                    TbSettings.Text = Resource.GetString("002_PleaseWait") + "......";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    Timer_Settings_Action = "CreateTile";
                    Timer_Settings.Start();
                }
                catch
                {
                }
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // Sprachen Menü öffnen
        // ----------------------------------------------------------------------------------------------------------------------
        private void OpenLanguageMenu(object sender, PointerRoutedEventArgs e)
        {
            GrLanguage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            MenuOpen = true;
        }
        // ----------------------------------------------------------------------------------------------------------------------





        // About Buttons
        //---------------------------------------------------------------------------------------------------------
        // Button Bewerten
        private async void btnRate(object sender, PointerRoutedEventArgs e)
        {
            //Link zum Store
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            }
            catch
            { }
        }



        //Button Kaufen
        private async void btnBuy(object sender, PointerRoutedEventArgs e)
        {
            LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
            if (licenseInformation.IsTrial)
            {
                try
                {
                    await CurrentApp.RequestAppPurchaseAsync(false);
                    if (!licenseInformation.IsTrial && licenseInformation.IsActive)
                    {
                        // Benachrichtigung ausgeben, das App gekauft wurde
                        CreateNotificationMessage(Resource.GetString("002_BuySuccessfully"));
                        // Speichern das nun Vollversion
                        var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
                        var F_FullVersion = await SF_Settings.CreateFileAsync("FullVersion.txt", CreationCollisionOption.OpenIfExists);
                        FullVersion = true;
                        await FileIO.WriteTextAsync(F_FullVersion, "True");
                        // Button Buy verschwinden lassen
                        SPTrial.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        CreateNotificationMessage(Resource.GetString("002_StillTrial"));
                        BuyedFailed();
                    }
                }
                catch (Exception)
                {
                    CreateNotificationMessage(Resource.GetString("002_BuyFailed"));
                    BuyedFailed();
                }
            }
            else
            {
                CreateNotificationMessage(Resource.GetString("002_Buyed"));
            }
        }
        private async void BuyedFailed()
        {
            //Link zum Store
            //Uri storepage = new Uri(string.Format("ms-windows-store:PDP?PFN=a7fb0b98-bf57-4d6a-b8c5-d4974312252d_jpb59zg2490q0"), UriKind.Absolute);
            //await Launcher.LaunchUriAsync(storepage);
        }



        //Button Facebook
        private async void btnFacebook(object sender, PointerRoutedEventArgs e)
        {
            //Link zu Facebook
            await Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/xtrose.xtrose"));
        }


        //Button andere Apps
        private async void btnOther(object sender, PointerRoutedEventArgs e)
        {
            //Link zum Store
            Uri storepage = new Uri(string.Format("ms-windows-store://search?publisher={0}", "xtrose"), UriKind.Absolute);
            await Launcher.LaunchUriAsync(storepage);
        }


        // Buttons Rate Panel
        private async void BtnRateNever_click(object sender, RoutedEventArgs e)
        {
            // Ordner öffnen oder erstellen
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);

            // Prüfen ob "Settings/RateReminder" besteht
            var F_Reminder = await SF_Settings.CreateFileAsync("Reminder.txt", CreationCollisionOption.OpenIfExists);
            // Reminder erstellen
            DateTime DT = DateTime.Now;
            DT = DT.AddDays(1500);
            string String_Reminder = DT.Year + ";" + DT.Month + ";" + DT.Day + ";" + DT.Hour + ";" + DT.Minute;
            // Speichern
            await FileIO.WriteTextAsync(F_Reminder, String_Reminder);

            // Reminder schließen
            GrRate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void BtnRateLater_click(object sender, RoutedEventArgs e)
        {
            // Ordner öffnen oder erstellen
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);

            // Prüfen ob "Settings/RateReminder" besteht
            var F_Reminder = await SF_Settings.CreateFileAsync("Reminder.txt", CreationCollisionOption.OpenIfExists);
            // Reminder erstellen
            DateTime DT = DateTime.Now;
            DT = DT.AddDays(4);
            string String_Reminder = DT.Year + ";" + DT.Month + ";" + DT.Day + ";" + DT.Hour + ";" + DT.Minute;
            // Speichern
            await FileIO.WriteTextAsync(F_Reminder, String_Reminder);

            // Reminder schließen
            GrRate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void BtnRateRate_click(object sender, RoutedEventArgs e)
        {
            // Link zum Store
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
            }
            catch
            { }
            
            // Ordner öffnen oder erstellen
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);

            // Prüfen ob "Settings/RateReminder" besteht
            var F_Reminder = await SF_Settings.CreateFileAsync("Reminder.txt", CreationCollisionOption.OpenIfExists);
            // Reminder erstellen
            DateTime DT = DateTime.Now;
            DT = DT.AddDays(1500);
            string String_Reminder = DT.Year + ";" + DT.Month + ";" + DT.Day + ";" + DT.Hour + ";" + DT.Minute;
            // Speichern
            await FileIO.WriteTextAsync(F_Reminder, String_Reminder);

            // Reminder schließen
            GrRate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }


        // Sprache umstellen
        // Sprache auswählen
        bool DoSelectLanguage = true;
        private async void LanguagesChange(object sender, SelectionChangedEventArgs e)
        {
            // Wenn ausgeführt wird
            if (DoSelectLanguage)
            {
                // Warnung erstellen
                var Message = new MessageDialog(Resource.GetString("002_SelectLanguageString"), Resource.GetString("002_Warning"));
                Message.Commands.Add(new UICommand(Resource.GetString("002_Yes"), new UICommandInvokedHandler(this.MessageSelectLanguageInvokedHandler)));
                Message.Commands.Add(new UICommand(Resource.GetString("002_No"), new UICommandInvokedHandler(this.MessageSelectLanguageInvokedHandler)));
                Message.DefaultCommandIndex = 0;
                Message.CancelCommandIndex = 1;
                await Message.ShowAsync();
            }
        }
        // Ausführen nach bestätigung
        private async void MessageSelectLanguageInvokedHandler(IUICommand command)
        {
            // Wenn OK gedrückt wurde
            if (command.Label == Resource.GetString("002_Yes"))
            {
                // Ausgewählen Index ermitteln
                int SelectedI = LBLanguages.SelectedIndex;
                // Ausgewählte Sprache ermitteln
                Language = List_Languages[SelectedI].code;
                // Neue Sprachdatei erstellen
                var F_Language = await SF.CreateFileAsync("Language.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(F_Language, Language);
                // Sprache festlegen
                try
                {
                    var culture = new CultureInfo(Language);
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture.Name;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                    Application.Current.Exit();
                }
                catch
                { }
            }
            // Wenn schließen gedrückt wurde
            else
            { }

            // Angeben das auswahl nicht ausgeführt wird
            DoSelectLanguage = false;
            // Listbox auswahl aufheben
            try
            {
                LBLanguages.SelectedIndex = -1;
            }
            catch
            { }
            // Angeben das auswahl wieder ausgeführt wird
            DoSelectLanguage = true;
        }
        //---------------------------------------------------------------------------------------------------------





        // Benachrichtigungen
        //-----------------------------------------------------------------------------------------------------------------
        // Standard Benachrichtung // Button schließen
        async void CreateNotificationMessage(string Msg)
        {
            var Message = new MessageDialog(Msg);
            Message.Commands.Add(new UICommand(Resource.GetString("002_Close"), new UICommandInvokedHandler(this.MessageErrorInvokedHandler)));
            Message.DefaultCommandIndex = 0;
            Message.CancelCommandIndex = 1;
            await Message.ShowAsync();
        }
        // Ausführen nach bestätigung
        private void MessageErrorInvokedHandler(IUICommand command)
        {
            // Wenn OK gedrückt wurde
            if (command.Label == Resource.GetString("002_Close"))
            {
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        // Timer Settings
        //-----------------------------------------------------------------------------------------------------------------
        // Variablen
        string Timer_Settings_Action = "None";
        // Methode
        private void Timer_Settings_Tick(object sender, object e)
        {
            // Wenn Tile neu erstellt wird
            if (Timer_Settings_Action == "CreateTile")
            {
                // Neuen Task erstellen
                CreateNewTask();
                // Timer zurücksetzen
                Timer_Settings_Action = "None";
                GrSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }


            //Auf Vollversion prüfen
            if (Timer_Settings_Action == "LoadWebsite")
            {
                //Prüfen ob Connection Time Out
                bool ConnectionTimeOut = false;
                if (CheckUpdateStart == DateTime.MinValue)
                {
                    CheckUpdateStart = DateTime.Now;
                }
                else
                {
                    if (CheckUpdateStart.AddSeconds(10) < DateTime.Now)
                    {
                        ConnectionTimeOut = true;
                    }
                }
                //Preüfen ob Time out
                if (ConnectionTimeOut == false)
                {
                    //Prüfen ob Quelle geladen
                    if (Source != "")
                    {
                        //wenn feedtemp = feed, Quelltext komplett geladen
                        if (Source == CheckSum)
                        {
                            //Timer Status löschen
                            Timer_Settings_Action = "none";
                            //feedtemp löschen
                            CheckSum = "";
                            //Timer Stoppen
                            Timer_Settings.Stop();
                            //Listbox erstellen
                            CheckIfFullVersion();
                        }
                        //wenn feedtemp != feed, Quelltext noch nicht komplett geladen
                        else
                        {
                            //feedtemp zu aktuellem Feed machen
                            CheckSum = Source;
                        }
                    }
                }
                //Bei TimeOut
                else
                {
                    //TimeOut Benachrichtigung ausgeben
                    CreateNotificationMessage(Resource.GetString("002_NoUpdate"));
                    //feedtemp löschen
                    CheckSum = "";
                    GrSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    Timer_Settings_Action = "None";
                    Timer_Settings.Stop();
                }
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        // Check for updates
        //-----------------------------------------------------------------------------------------------------------------
        //Variabeln, auf Update prüfen
        string SourceWebsite = "http://h2236721.stratoserver.net/AppData/WindowsPhone/8_1_Music/OnlineComponents/IDQuery.php";
        string Source = "";
        string CheckSum = "";
        DateTime CheckUpdateStart = DateTime.MinValue;
        //Button, auf Update Prüfen
        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            //Zeit zurücksetzen
            CheckUpdateStart = DateTime.MinValue;
            //Timer starten
            Timer_Settings.Start();
            //Timer Status angeben
            Timer_Settings_Action = "LoadWebsite";
            TbSettings.Text = Resource.GetString("002_PleaseWait");
            GrSettings.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //Seite versuchen zu erreichen
            GetSourceCode();
        }


        //Webseite versuchen zu erreichen
        public void GetSourceCode()
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(SourceWebsite);
                request.BeginGetResponse(new AsyncCallback(HandleResponse), request);
            }
            catch
            {
                //Wenn Webseite nicht erreichbar, Fehlermeldung ausgeben
                CreateNotificationMessage(Resource.GetString("002_NoUpdate"));
                //Timer und Grid zurüchsetzen
                GrSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Timer_Settings_Action = "none";
                Timer_Settings.Stop();
            }
        }


        //Quelltext in String speichern
        public void HandleResponse(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;

            try
            {
                if (request != null)
                {
                    using (WebResponse response = request.EndGetResponse(result))
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            //Quelltext laden
                            Source = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
            }
        }


        //Quelle Verarbeiten, auf Update prüfen
        async void CheckIfFullVersion()
        {
            //Variabeln
            bool UpdateAvailable = false;
            //Wenn Quelle verfügbar und keine Vollversion
            if (Source != "" & FullVersion == false)
            {
                //Anonymous ID ermitteln
                HardwareToken myToken = HardwareIdentification.GetPackageSpecificToken(null);
                IBuffer hardwareId = myToken.Id;
                string MyUserID = hardwareId.ToString();
                
                //Quelle zerlegen
                string[] SourceSplit = Regex.Split(Source, ";;;");
                //IDs durchlaufen und Prüfen
                for (int i = 0; i < SourceSplit.Count(); i++)
                {
                    //Wenn ID vorhanden
                    if (SourceSplit[i].Trim() == MyUserID)
                    {
                        //Vollversion erstellen
                        FullVersion = true;
                        UpdateAvailable = true;
                        // Prüfen ob "Settings/FulVersion" besteht
                        var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
                        var F_FullVersion = await SF_Settings.CreateFileAsync("FullVersion.txt", CreationCollisionOption.OpenIfExists);
                        await FileIO.WriteTextAsync(F_FullVersion, "True");
                        //Benachrichtigung ausgeben
                        CreateNotificationMessage(Resource.GetString("002_NoteUpdateFullVersion"));
                        break;
                    }
                }
            }
            //Wenn kein Update verfügbar
            if (UpdateAvailable == false)
            {
                CreateNotificationMessage(Resource.GetString("002_NoUpdate"));
            }
            //Timer zurückstellen
            CheckSum = "";
            GrSettings.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Timer_Settings_Action = "none";
            Timer_Settings.Stop();
        }
        //-----------------------------------------------------------------------------------------------------------------





        // Back Button
        //-----------------------------------------------------------------------------------------------------------------
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            // Wenn Menüs offen
            if (MenuOpen)
            {
                GrRate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                GrLanguage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                MenuOpen = false;
                e.Handled = true;
            }
            // Wenn keine Menüs offen
            else
            {
                // App schließen
                Application.Current.Exit();
            }
        }
        //-----------------------------------------------------------------------------------------------------------------





        // Tile neu erstellen
        //---------------------------------------------------------------------------------------------------------
        private async void CreateNewTask()
        {
            // Prüfen ob Tile bereits gepinnt
            if (SecondaryTile.Exists(PivotPage.appbarTileId))
            {

                // Ordner öffnen oder erstellen
                StorageFolder SF = ApplicationData.Current.LocalFolder;
                var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);



                // Sprach Variablen // Werden aus einer Datei geladen
                string Language = "en-us";
                string Days = "Days";
                string Day = "Day";
                string Hours = "Hours";
                string Hour = "Hours";
                string Minutes = "Minutes";
                string Minute = "Minute";
                string IsLoading = "is loading";
                string NameBattery = "Battery";



                // Prüfen of "Settings/LanguageString" besteht
                var F_LanguageString = await SF_Settings.CreateFileAsync("LanguageString.dat", CreationCollisionOption.OpenIfExists);
                var Temp_LanguageString = await FileIO.ReadTextAsync(F_LanguageString);
                // Prüfen ob Settings besteht
                string String_LanguageString = Convert.ToString(Temp_LanguageString);
                //Einstellungen umsetzen
                string[] Split_LanguageString = Regex.Split(Temp_LanguageString, ";");
                // Einstellungen durchlaufen
                for (int i = 0; i < Split_LanguageString.Count(); i++)
                {
                    // Einzelne Einstellung zerlegen
                    string[] Split_Setting = Regex.Split(Split_LanguageString[i], "=");
                    // Wenn Einstellung "Language"
                    if (Split_Setting[0] == "Language")
                    {
                        Language = Split_Setting[1];
                    }
                    // Wenn Einstellung "Day"
                    else if (Split_Setting[0] == "Day")
                    {
                        Day = Split_Setting[1];
                    }
                    // Wenn Einstellung "Days"
                    else if (Split_Setting[0] == "Days")
                    {
                        Days = Split_Setting[1];
                    }
                    // Wenn Einstellung "Hour"
                    else if (Split_Setting[0] == "Hour")
                    {
                        Hour = Split_Setting[1];
                    }
                    // Wenn Einstellung "Hours"
                    else if (Split_Setting[0] == "Hours")
                    {
                        Hours = Split_Setting[1];
                    }
                    // Wenn Einstellung "Minute"
                    else if (Split_Setting[0] == "Minute")
                    {
                        Minute = Split_Setting[1];
                    }
                    // Wenn Einstellung "Minutes"
                    else if (Split_Setting[0] == "Minutes")
                    {
                        Minutes = Split_Setting[1];
                    }
                    // Wenn Einstellung "IsLoading"
                    else if (Split_Setting[0] == "IsLoading")
                    {
                        IsLoading = Split_Setting[1];
                    }
                    // Wenn Einstellung "NameBattery"
                    else if (Split_Setting[0] == "NameBattery")
                    {
                        NameBattery = Split_Setting[1];
                    }
                }



                // Tile Updater erstellen
                var tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(PivotPage.appbarTileId);
                var plannedUpdated = tileUpdater.GetScheduledTileNotifications();

                // CultureInfo laden
                //string language = GlobalizationPreferences.Languages.First();
                CultureInfo cultureInfo = new CultureInfo("en-US");
                try
                {
                    cultureInfo = new CultureInfo(Language);
                }
                catch
                {
                }

                // Batterieleistung ausgeben
                var BatteryInfo = Battery.GetDefault();
                TimeSpan BatteryTime = BatteryInfo.RemainingDischargeTime;
                string BatteryString = "";

                // Temp DateTime erstellen
                DateTime temp = DateTime.Now;
                // Aktueller DateTime erstellen und Sekunden auf 0 stellen
                DateTime now = new DateTime(temp.Year, temp.Month, temp.Day, temp.Hour, temp.Minute, 0);



                // Zeitrahmen erstellen, bis zu der die Uhr upgedatet wird wird
                DateTime planTill = now.AddMinutes(90);



                // Wenn Task ausgeführt wird
                if (FullVersion | Run)
                {

                    // Design 0 // TileSquareText02 // TileWideText01
                    if (SetDesign == 0)
                    {
                        // Zeit erstellen
                        DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);

                        // Strings für die Tiles erstellen
                        string Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                        string Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
                        string Str2 = "";
                        if (SetBattery == true)
                        {
                            // String der Batterieanzeige ändern
                            BatteryString = "";
                            if (BatteryTime.Days > 100)
                            {
                                BatteryString = IsLoading;
                            }
                            else
                            {
                                if (BatteryTime.Days > 0)
                                {
                                    if (BatteryTime.Days == 1)
                                    {
                                        BatteryString = "1 " + Day + " ";
                                    }
                                    else
                                    {
                                        BatteryString = BatteryTime.Days.ToString() + " " + Days + " ";
                                    }
                                }
                                if (BatteryTime.Hours == 1)
                                {
                                    BatteryString += "1 " + Hour + " ";
                                }
                                else
                                {
                                    BatteryString += BatteryTime.Hours + " " + Hours + " ";
                                }
                                if (BatteryTime.Minutes == 1)
                                {
                                    BatteryString += "1 " + Minute + " ";
                                }
                                else
                                {
                                    BatteryString += BatteryTime.Minutes + " " + Minutes + " ";
                                }
                            }
                            Str1 += "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                            Str2 = BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                        }

                        // XML für die Tiles erstellen
                        const string xml = @"<tile><visual>
                                        <binding branding='none' template=""TileSquareText02""><text id=""1"">{0}</text><text id=""2"">{1}</text></binding>
                                        <binding branding='none'  template=""TileWideText01""><text id=""1"">{0}</text><text id=""2"">{1}</text><text id=""3"">{2}</text><text id=""4""></text><text id=""5""></text></binding>
                                   </visual></tile>";

                        // Tiles erstellen
                        var tileXmlNow = string.Format(xml, Str0, Str1, Str2);
                        XmlDocument documentNow = new XmlDocument();
                        documentNow.LoadXml(tileXmlNow);

                        // Tile Updaten
                        tileUpdater.Update(new TileNotification(documentNow) { ExpirationTime = now.AddMinutes(1) });

                        // Tile Update Plan erstellen
                        for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
                        {
                            Debug.WriteLine(startPlanning);
                            Debug.WriteLine(planTill);

                            try
                            {
                                // Strings für die Tiles erstellen
                                Str0 = startPlanning.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                                Str1 = startPlanning.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
                                Str2 = "";
                                if (SetBattery == true)
                                {
                                    // Batteriezeit verringern
                                    BatteryTime = BatteryTime.Add(new TimeSpan(0, -1, 0));
                                    // Sting der Batterieanzeige ändern
                                    BatteryString = "";
                                    if (BatteryTime.Days > 100)
                                    {
                                        BatteryString = IsLoading;
                                    }
                                    else
                                    {
                                        if (BatteryTime.Days > 0)
                                        {
                                            if (BatteryTime.Days == 1)
                                            {
                                                BatteryString = "1 " + Day + " ";
                                            }
                                            else
                                            {
                                                BatteryString = BatteryTime.Days.ToString() + " " + Days + " ";
                                            }
                                        }
                                        if (BatteryTime.Hours == 1)
                                        {
                                            BatteryString += "1 " + Hour + " ";
                                        }
                                        else
                                        {
                                            BatteryString += BatteryTime.Hours + " " + Hours + " ";
                                        }
                                        if (BatteryTime.Minutes == 1)
                                        {
                                            BatteryString += "1 " + Minute + " ";
                                        }
                                        else
                                        {
                                            BatteryString += BatteryTime.Minutes + " " + Minutes + " ";
                                        }
                                    }
                                    Str1 += "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                                    Str2 = BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                                }

                                // Tiles erstellen
                                var tileXml = string.Format(xml, Str0, Str1, Str2);
                                XmlDocument document = new XmlDocument();
                                document.LoadXml(tileXml);

                                // Tiles Plan erstellen
                                ScheduledTileNotification scheduledNotification = new ScheduledTileNotification(document, new DateTimeOffset(startPlanning)) { ExpirationTime = startPlanning.AddMinutes(1) };
                                tileUpdater.AddToSchedule(scheduledNotification);

                                Debug.WriteLine("schedule for: " + startPlanning);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("exception: " + e.Message);
                            }
                        }
                    }





                    // Design 1 // TileSquareText02 // TileWideText03
                    else if (SetDesign == 1)
                    {
                        // Zeit erstellen
                        DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);

                        // Strings für die Tiles erstellen
                        string Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                        string Str1 = "";
                        string Str2 = "";
                        if (SetBattery == true)
                        {
                            // String der Batteriezeit verändern
                            if (BatteryTime.Days > 100)
                            {
                                BatteryString = IsLoading;
                            }
                            else
                            {
                                BatteryString = ((BatteryTime.Days * 24) + BatteryTime.Hours).ToString() + ":" + BatteryTime.Minutes;
                            }
                            Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern) + "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                            Str2 = Str0 + "\r\n" + Str1;
                        }
                        else
                        {
                            Str1 = now.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
                            Str2 = Str0 + "\r\n" + Str1;
                        }

                        // XML für die Tiles erstellen
                        const string xml = @"<tile><visual>
                                        <binding branding='none' template=""TileSquareText02""><text id=""1"">{0}</text><text id=""2"">{1}</text></binding>
                                        <binding branding='none'  template=""TileWideText03""><text id=""1"">{2}</text></binding>
                                   </visual></tile>";

                        // Tiles erstellen
                        var tileXmlNow = string.Format(xml, Str0, Str1, Str2);
                        XmlDocument documentNow = new XmlDocument();
                        documentNow.LoadXml(tileXmlNow);

                        // Tile Updaten
                        tileUpdater.Update(new TileNotification(documentNow) { ExpirationTime = now.AddMinutes(1) });

                        // Tile Update Plan erstellen
                        for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
                        {
                            Debug.WriteLine(startPlanning);
                            Debug.WriteLine(planTill);

                            try
                            {
                                // Strings für die Tiles erstellen
                                Str0 = startPlanning.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                                Str1 = "";
                                Str2 = "";
                                if (SetBattery == true)
                                {
                                    // Batteriezeit verringern
                                    BatteryTime = BatteryTime.Add(new TimeSpan(0, -1, 0));
                                    // String der Batteriezeit verändern
                                    if (BatteryTime.Days > 100)
                                    {
                                        BatteryString = IsLoading;
                                    }
                                    else
                                    {
                                        BatteryString = ((BatteryTime.Days * 24) + BatteryTime.Hours).ToString() + ":" + BatteryTime.Minutes;
                                    }
                                    Str1 = startPlanning.ToString(cultureInfo.DateTimeFormat.LongDatePattern) + "\r\n" + BatteryInfo.RemainingChargePercent + "% " + BatteryString;
                                    Str2 = Str0 + "\r\n" + Str1;
                                }
                                else
                                {
                                    Str1 = startPlanning.ToString(cultureInfo.DateTimeFormat.LongDatePattern);
                                    Str2 = Str0 + "\r\n" + Str1;
                                }

                                // Tiles erstellen
                                var tileXml = string.Format(xml, Str0, Str1, Str2);
                                XmlDocument document = new XmlDocument();
                                document.LoadXml(tileXml);

                                // Tiles Plan erstellen
                                ScheduledTileNotification scheduledNotification = new ScheduledTileNotification(document, new DateTimeOffset(startPlanning)) { ExpirationTime = startPlanning.AddMinutes(1) };
                                tileUpdater.AddToSchedule(scheduledNotification);

                                Debug.WriteLine("schedule for: " + startPlanning);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("exception: " + e.Message);
                            }
                        }
                    }





                    // Design 2 // TileSquareText02 // TileWideBlockAndText01
                    else if (SetDesign == 2)
                    {
                        // Zeit erstellen
                        DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);

                        // Strings für die Tiles erstellen
                        int DW = 0;
                        if (now.DayOfWeek.ToString() == "Monday")
                        {
                            DW = 1;
                        }
                        else if (now.DayOfWeek.ToString() == "Tuesday")
                        {
                            DW = 2;
                        }
                        else if (now.DayOfWeek.ToString() == "Wednesday")
                        {
                            DW = 3;
                        }
                        else if (now.DayOfWeek.ToString() == "Thursday")
                        {
                            DW = 4;
                        }
                        else if (now.DayOfWeek.ToString() == "Friday")
                        {
                            DW = 5;
                        }
                        else if (now.DayOfWeek.ToString() == "Saturday")
                        {
                            DW = 6;
                        }
                        string Str0 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                        string Str1 = cultureInfo.DateTimeFormat.DayNames[DW] + "  ";
                        string Str2 = now.ToString(cultureInfo.DateTimeFormat.MonthDayPattern);
                        string Str3 = "";
                        string Str4 = "";
                        if (SetBattery == true)
                        {
                            // Batterie Prozenz anzeigen
                            Str3 = NameBattery + " " + BatteryInfo.RemainingChargePercent + "%";
                            // String der Batteriezeit verändern
                            if (BatteryTime.Days > 100)
                            {
                                BatteryString = IsLoading;
                            }
                            else
                            {
                                if (BatteryTime.Days > 0)
                                {
                                    if (BatteryTime.Days == 1)
                                    {
                                        BatteryString = "1 " + Day + " ";
                                    }
                                    else
                                    {
                                        BatteryString = BatteryTime.Days.ToString() + " " + Days + " ";
                                    }
                                }
                                if (BatteryTime.Hours == 1)
                                {
                                    BatteryString += "1 " + Hour + " ";
                                }
                                else
                                {
                                    BatteryString += BatteryTime.Hours + " " + Hours + " ";
                                }
                                if (BatteryTime.Minutes == 1)
                                {
                                    BatteryString += "1 " + Minute + " ";
                                }
                                else
                                {
                                    BatteryString += BatteryTime.Minutes + " " + Minutes + " ";
                                }
                            }
                            Str4 = BatteryString;
                        }
                        string Str5 = now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                        string Str6 = "";
                        string[] Split5 = Regex.Split(Str5, " ");
                        if (Split5.Count() > 1)
                        {
                            Str5 = Split5[0].Trim();
                            Str6 = Split5[1].Trim();
                        }

                        // XML für die Tiles erstellen
                        const string xml = @"<tile><visual>
                                        <binding branding='none' template=""TileSquareBlock""><text id=""1"">{5}</text><text id=""2"">{6}</text></binding>
                                        <binding branding='none'  template=""TileWideBlockAndText01""><text id=""1"">{0}</text><text id=""2"">{1}</text><text id=""3"">{2}</text><text id=""4"">{3}</text><text id=""5"">{4}</text><text id=""6""></text></binding>
                                   </visual></tile>";

                        // Tiles erstellen
                        var tileXmlNow = string.Format(xml, Str0, Str1, Str2, Str3, Str4, Str5, Str6);
                        XmlDocument documentNow = new XmlDocument();
                        documentNow.LoadXml(tileXmlNow);

                        // Tile Updaten
                        tileUpdater.Update(new TileNotification(documentNow) { ExpirationTime = now.AddMinutes(1) });

                        // Tile Update Plan erstellen
                        for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
                        {
                            Debug.WriteLine(startPlanning);
                            Debug.WriteLine(planTill);

                            try
                            {
                                // Strings für die Tiles erstellen
                                Str0 = startPlanning.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                                Str1 = startPlanning.DayOfWeek.ToString();
                                Str2 = startPlanning.ToString(cultureInfo.DateTimeFormat.MonthDayPattern);
                                Str3 = "";
                                Str4 = "";
                                if (SetBattery == true)
                                {
                                    // Batterie Prozenz anzeigen
                                    Str3 = NameBattery + " " + BatteryInfo.RemainingChargePercent + "%";
                                    // Batteriezeit verringern
                                    BatteryTime = BatteryTime.Add(new TimeSpan(0, -1, 0));
                                    // String der Batteriezeit verändern
                                    if (BatteryTime.Days > 100)
                                    {
                                        BatteryString = IsLoading;
                                    }
                                    else
                                    {
                                        if (BatteryTime.Days > 0)
                                        {
                                            if (BatteryTime.Days == 1)
                                            {
                                                BatteryString = "1 " + Day + " ";
                                            }
                                            else
                                            {
                                                BatteryString = BatteryTime.Days.ToString() + " " + Days + " ";
                                            }
                                        }
                                        if (BatteryTime.Hours == 1)
                                        {
                                            BatteryString += "1 " + Hour + " ";
                                        }
                                        else
                                        {
                                            BatteryString += BatteryTime.Hours + " " + Hours + " ";
                                        }
                                        if (BatteryTime.Minutes == 1)
                                        {
                                            BatteryString += "1 " + Minute + " ";
                                        }
                                        else
                                        {
                                            BatteryString += BatteryTime.Minutes + " " + Minutes + " ";
                                        }
                                    }
                                    Str4 = BatteryString;
                                }
                                Str5 = startPlanning.ToString(cultureInfo.DateTimeFormat.ShortTimePattern);
                                Str6 = "";
                                Split5 = Regex.Split(Str5, " ");
                                if (Split5.Count() > 1)
                                {
                                    Str5 = Split5[0].Trim();
                                    Str6 = Split5[1].Trim();
                                }

                                // Tiles erstellen
                                var tileXml = string.Format(xml, Str0, Str1, Str2, Str3, Str4, Str5, Str6);
                                XmlDocument document = new XmlDocument();
                                document.LoadXml(tileXml);

                                // Tiles Plan erstellen
                                ScheduledTileNotification scheduledNotification = new ScheduledTileNotification(document, new DateTimeOffset(startPlanning)) { ExpirationTime = startPlanning.AddMinutes(1) };
                                tileUpdater.AddToSchedule(scheduledNotification);

                                Debug.WriteLine("schedule for: " + startPlanning);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("exception: " + e.Message);
                            }
                        }
                    }
                }



                // Wenn Task nicht ausgeführt wird
                else
                {
                    // Benachrichtigung ausgeben
                    CreateNotificationMessage(Resource.GetString("002_PeriodExpired"));
                }
            }
        }
        //---------------------------------------------------------------------------------------------------------





    }
}
