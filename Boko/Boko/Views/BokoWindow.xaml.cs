using Boko.Models;
using Boko.Utilities;
using ff14bot;
using ff14bot.Managers;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using TrelloNet;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;

namespace Boko.Views
{
    public partial class BokoWindow : Window
    {
        public BokoWindow()
        {
            InitializeComponent();

            SelectTheme();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #region Theme Switch

        private void SelectTheme()
        {
            switch (MainSettingsModel.Instance.Theme)
            {
                case SelectedTheme.Pink:
                    Pink();
                    break;

                case SelectedTheme.Blue:
                    Blue();
                    break;

                case SelectedTheme.Green:
                    Green();
                    break;

                case SelectedTheme.Red:
                    Red();
                    break;

                case SelectedTheme.Yellow:
                    Yellow();
                    break;

                default:
                    Pink();
                    break;
            }
        }

        private void Pink()
        {
            Resources.MergedDictionaries.Clear();
            AddResourceDictionary("/Boko;component/Views/Styles/BokoStyles.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BaseColors.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/PinkTheme.xaml");
        }

        private void Blue()
        {
            Resources.MergedDictionaries.Clear();
            AddResourceDictionary("/Boko;component/Views/Styles/BokoStyles.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BaseColors.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BlueTheme.xaml");
        }

        private void Yellow()
        {
            Resources.MergedDictionaries.Clear();
            AddResourceDictionary("/Boko;component/Views/Styles/BokoStyles.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BaseColors.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/YellowTheme.xaml");
        }

        private void Red()
        {
            Resources.MergedDictionaries.Clear();
            AddResourceDictionary("/Boko;component/Views/Styles/BokoStyles.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BaseColors.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/RedTheme.xaml");
        }

        private void Green()
        {
            Resources.MergedDictionaries.Clear();
            AddResourceDictionary("/Boko;component/Views/Styles/BokoStyles.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/BaseColors.xaml");
            AddResourceDictionary("/Boko;component/Views/Styles/GreenTheme.xaml");
        }

        #endregion Theme Switch

        private void AddResourceDictionary(string source)
        {
            var resourceDictionary = Application.LoadComponent(new Uri(source, UriKind.Relative)) as ResourceDictionary;
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            MainSettingsModel.Instance.Save();
            Close();
        }

        private void CmbSwitchTheme(object sender, SelectionChangedEventArgs e)
        {
            SelectTheme();
        }

        #region Export Settings Region

        private void TrelloLink(object sender, RoutedEventArgs e)
        {
            Process.Start("https://trello.com/b/KrJDSjp8");
        }

        private void ExportSettings(object sender, RoutedEventArgs routedEventArgs)
        {
            var priorAuth = false;

            var name = "Boko";
            var rbLogLocation = new OpenFileDialog
            {
                Title = @"Please select the log file(s) where the issue exists. Usually \RB\Logs",
                Multiselect = true,
                InitialDirectory = "/Logs"
            };

            ITrello trello = new Trello("b09ce954a206f0165506513795959840");

            if (MainSettingsModel.Instance.TrelloToken == "" || MainSettingsModel.Instance.TrelloTokenData == null || DateTime.Now.Date > MainSettingsModel.Instance.TrelloTokenData.DateCreated.Date + TimeSpan.FromDays(29))
            {
                var url = trello.GetAuthorizationUrl("Boko", Scope.ReadWrite, Expiration.ThirtyDays);
                Process.Start(url.ToString());

                BeginTrelloAuth:
                var token = Interaction.InputBox("Please enter the Trello Access Token you recieved.");
                if (token.Any(char.IsWhiteSpace))
                    token = Regex.Replace(token, @"\s+", "");
                if (token == "")
                {
                    var result = MessageBox.Show("You didn't enter anything for the Trello Key. Abandon Report?", "Trello Auth Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                            return;

                        case MessageBoxResult.Yes:
                            return;

                        case MessageBoxResult.No:
                            goto BeginTrelloAuth;
                    }
                }

                var tokenData = trello.Tokens.WithToken(token);
                MainSettingsModel.Instance.TrelloToken = token;
                MainSettingsModel.Instance.TrelloTokenData = tokenData;
                FormManager.SaveFormInstances();

                trello.Authorize(token);
                Process.Start("https://trello.com/invite/b/KrJDSjp8/0e335c0fad7fdea3708ee73d291e35c7");
                MessageBox.Show("Thank you for authorizing your account!", "Thanks!", MessageBoxButton.OK);
                goto SkipRegularTokenCheck;
            }

            if (MainSettingsModel.Instance.TrelloToken != "")
                trello.Authorize(MainSettingsModel.Instance.TrelloToken);
            priorAuth = true;

            SkipRegularTokenCheck:

            var bugReportList = new ListId("584020ad8f3cb7d794f3dbcd");

            try
            {
                var newBugCard = trello.Cards.Add(new NewCard("New Bug Report", bugReportList));

                CardName:
                newBugCard.Name = Interaction.InputBox("What was the general Bug type? (Rotation, Exception, Crash, ect)");
                if (newBugCard.Name == "")
                {
                    var tryAgain = MessageBox.Show("You didn't enter anything. Abandon Report?", "Trello Name Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (tryAgain)
                    {
                        case MessageBoxResult.Cancel:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.Yes:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.No:
                            goto CardName;
                    }
                }

                CardDescription:
                newBugCard.Desc = Interaction.InputBox("Please describe the Bug in as much detail as possible.");
                if (newBugCard.Desc == "")
                {
                    var tryAgain = MessageBox.Show("You didn't enter anything. Abandon Report?", "Trello Description Failure", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (tryAgain)
                    {
                        case MessageBoxResult.Cancel:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.Yes:
                            trello.Cards.Delete(newBugCard);
                            return;

                        case MessageBoxResult.No:
                            goto CardDescription;
                    }
                }

                trello.Cards.Update(newBugCard);

                try
                {
                    var file = new StreamWriter("Bug Report Description");
                    file.WriteLine(newBugCard.Desc);
                    file.Close();

                    rbLogLocation.ShowDialog();
                    string[] myFile;
                    if (!string.IsNullOrEmpty(rbLogLocation.FileName))
                        myFile = rbLogLocation.FileNames;
                    else goto cleanup;

                    using (var archive = ZipFile.Open($"{name} Bug Report.zip", ZipArchiveMode.Create))
                    {
                        foreach (var rbLog in myFile)
                        {
                            var filename = rbLog;
                            archive.CreateEntryFromFile(rbLog, Path.GetFileName(filename));
                        }
                        archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + "/Boko/Chocobo_Settings.json", "Chocobo_Settings.json");
                        archive.CreateEntryFromFile(@"Settings/" + Core.Me.Name + "/Boko/Main_Settings.json", "Main_Settings.json");
                        archive.CreateEntryFromFile("Bug Report Description", "Bug Report Description.txt");
                    }
                }
                catch (Exception e)
                {
                    Logger.BokoLog(e.ToString());
                }

                trello.Cards.AddAttachment(newBugCard, new FileAttachment($"{name} Bug Report.zip", ($"{name} Bug Report")));

                var omninewb = trello.Members.WithId("58402025bbbfb2e3afe186fe");

                trello.Cards.AddMember(newBugCard, omninewb);
            }
            catch (Exception e)
            {
                Logger.BokoLog("Chances are, you closed the authorization window before you actually authorized Boko to post. Try to re-submit the error report.");
                Core.OverlayManager.AddToast(() => "Error: Re-Submit Error Report.", TimeSpan.FromMilliseconds(750), Color.FromRgb(110, 225, 214), Colors.White, new FontFamily("High Tower Text Italic"), new FontWeight(), 52);
                Console.WriteLine(e);
                throw;
            }

            cleanup:
            if (File.Exists("Bug Report Description"))
                File.Delete("Bug Report Description");
            if (File.Exists($"{name} Bug Report.zip"))
                File.Delete($"{name} Bug Report.zip");
            if (File.Exists($"{name} Bug Report"))
                File.Delete($"{name} Bug Report");

            if (priorAuth)
                Process.Start("https://trello.com/b/KrJDSjp8");
        }

        #endregion Export Settings Region
    }
}