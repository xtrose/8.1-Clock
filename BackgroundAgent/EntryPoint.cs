using Windows.ApplicationModel.Background;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.Phone.Devices.Power;
using Windows.Storage;
using Windows.UI.StartScreen;






// Namespace
namespace BackgroundAgent
{





    // Hintergrundtask
    public sealed class EntryPoint : IBackgroundTask
    {




        // Ausgeführter Code
        // ----------------------------------------------------------------------------------------------------------------------
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();


            // Ordner öffnen oder erstellen
            StorageFolder SF = ApplicationData.Current.LocalFolder;
            var SF_Settings = await SF.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);



            // Variablen für zweites Tile
            string appbarTileId = "MySecondaryTile";



            // Design Auswahl
            string String_Settings = "";
            int SetDesign = 2;
            bool SetBattery = true;
            DateTime FirstRunTime = DateTime.Now;
            bool FullVersion = false;
            bool Run = false;
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



            // Prüfen of "Settings/Settings" besteht
            var F_Settings = await SF_Settings.CreateFileAsync("Settings.dat", CreationCollisionOption.OpenIfExists);
            var Temp_Settings = await FileIO.ReadTextAsync(F_Settings);
            // Prüfen ob Settings besteht
            String_Settings = Convert.ToString(Temp_Settings);
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
                }
                // Wenn Einstellung "SetBattery"
                if (Split_Setting[0] == "SetBattery")
                {
                    SetBattery = Convert.ToBoolean(Split_Setting[1]);
                }
            }



            // Prüfen of "Settings/LanguageString" besteht
            var F_LanguageString = await SF_Settings.CreateFileAsync("LanguageString.dat", CreationCollisionOption.OpenIfExists);
            var Temp_LanguageString = await FileIO.ReadTextAsync(F_LanguageString);
            // Prüfen ob Settings besteht
            string String_LanguageString = Convert.ToString(Temp_LanguageString);
            //Einstellungen umsetzen
            string[] Split_LanguageString = Regex.Split(String_LanguageString, ";");
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



            // Bei Volversion
            if (FullVersion == false)
            {
                // Trial zeit ermitteln
                DateTime NowTime = DateTime.Now;
                DateTime ExpiredTime = FirstRunTime.AddDays(2);
                // Wenn Zeit noch nicht abgelaufen ist
                if (NowTime < ExpiredTime)
                {
                    // Angeben das App ausgeführt wird
                    Run = true;
                }
            }



            // Wenn zweites Tile gepinnt wurde
            if (SecondaryTile.Exists(appbarTileId))
            {



                // Tile Updater erstellen
                var tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(appbarTileId);
                var plannedUpdated = tileUpdater.GetScheduledTileNotifications();

                // CultureInfo laden
                //string language = GlobalizationPreferences.Languages.First();
                CultureInfo cultureInfo = new CultureInfo("en-us");
                try
                {
                    cultureInfo = new CultureInfo(Language);
                }
                catch
                { }

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
                        //if (plannedUpdated.Count > 0)
                        //    updateTime = plannedUpdated.Select(x => x.DeliveryTime.DateTime).Union(new[] { updateTime }).Max();

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
                        //if (plannedUpdated.Count > 0)
                        //    updateTime = plannedUpdated.Select(x => x.DeliveryTime.DateTime).Union(new[] { updateTime }).Max();

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
                        // if (plannedUpdated.Count > 0)
                        //    updateTime = plannedUpdated.Select(x => x.DeliveryTime.DateTime).Union(new[] { updateTime }).Max();

                        // Strings für die Tiles erstellen
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
                        string[] Split5 = Regex.Split(Str5, "AM");
                        if (Split5.Count() > 1)
                        {
                            Str5 = Split5[0].Trim();
                            Str6 = "AM";
                        }
                        else
                        {
                            Split5 = Regex.Split(Str5, "PM");
                            if (Split5.Count() > 1)
                            {
                                Str5 = Split5[0].Trim();
                                Str6 = "PM";
                            }
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
                                Str1 = startPlanning.DayOfWeek.ToString() + " ";
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
            }



            // Wenn Task nicht ausgeführt wird
            else
            {

            }

            deferral.Complete();
        }
        // ----------------------------------------------------------------------------------------------------------------------
    }
}
