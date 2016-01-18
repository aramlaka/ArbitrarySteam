using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArbitrarySteam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Visibility logonVisBefore, gameSelectorVisBefore;

        private SteamAPI steam;
        private SteamGame currGame;

        private int currGameIndex;

        public MainWindow()
        {
            InitializeComponent();
            //SetMinWindowSize((Int32)System.Windows.SystemParameters.PrimaryScreenHeight / 3, (Int32)System.Windows.SystemParameters.PrimaryScreenWidth / 3);
            //SetMinWindowSize(0, 0);

        }

        //TODO: REMOVE THIS?
        private void SetMinWindowSize(Int32 height, Int32 width)
        {
            SetValue(MainWindow.MinHeightProperty, height);
            SetValue(MainWindow.MinWidthProperty, width);            
        }

        

        private void TextBoxLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!radioCustomURL.IsChecked.HasValue || !radioSteamID.IsChecked.HasValue) //IsChecked is a Nullable<bool>.  We can't continue if either are null.
            {
                return; 
            }

            //TODO: CHANGE THIS METHOD
            
            if((bool)radioCustomURL.IsChecked)
            {
                if(tbLink.Text.Length < "steamcommunity.com/id/".Length)
                {
                    tbLink.Text = "steamcommunity.com/id/";
                    tbLink.SelectionStart = tbLink.Text.Length + 1;
                }
            }
            else if((bool)radioSteamID.IsChecked)
            {
                if(tbLink.Text.Length < "steamcommunity.com/profiles/".Length)
                {
                    tbLink.Text = "steamcommunity.com/profiles/";
                    tbLink.SelectionStart = tbLink.Text.Length + 1;
                }
            }
        }

        private void DisplayInfoOrError(string message, Int32 msTime, bool isError = false)
        {
            tbErrorOrInfo.Text = message;
            tbErrorOrInfo.Visibility = System.Windows.Visibility.Visible;

            if(isError)
            {
                tbErrorOrInfo.Foreground = Brushes.Red;
            }

            System.Timers.Timer timer = new System.Timers.Timer(msTime);

            timer.Start();

            timer.Elapsed += (sender, args) =>
            {
                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate
                {                
                    tbErrorOrInfo.Text = string.Empty;
                    tbErrorOrInfo.Visibility = System.Windows.Visibility.Collapsed;

                    if (isError)
                    {
                        tbErrorOrInfo.Foreground = Brushes.White;
                    }

                }), System.Windows.Threading.DispatcherPriority.Input);

                timer.Dispose();
            };            
        }

        #region Button_Clicks

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (tbLink.Text == "steamcommunity.com/id/" || tbLink.Text == "steamcommunity.com/profiles/") //The user wants to continue, but hasn't entered anything in
            {                     
                DisplayInfoOrError("You need to enter something first!", 3000);

                return;
            }            

            if(!radioCustomURL.IsChecked.HasValue) //IsChecked is a Nullable<bool>.  We can't continue if it's null.
            {
                DisplayInfoOrError("radioCustomURL.IsChecked is null!", 4000, true);
                return;
            }
    


            steam = new SteamAPI(tbLink.Text, (bool)radioCustomURL.IsChecked);

            if(steam.NoSteamKey)
            {
                DisplayInfoOrError("No Steam key set!", 4000, true);
                return;
            }

            if(steam.User.BadProfile) //we were unable to find the steam account associated with the given URL
            {
                if(steam.User.Games.Count <= 0)
                {
                    DisplayInfoOrError("That Steam user doesn't have any games!", 4000);
                }
                else
                {
                    DisplayInfoOrError("That account doesn't exist, or we couldn't connect to Steam's servers!", 5000, true);
                }
               
                return;
            }      

            labelUserName.Content = steam.User.Name;

            NewGame(true);

            

            logon.Visibility = System.Windows.Visibility.Collapsed;
            gameSelector.Visibility = System.Windows.Visibility.Visible;           
        }

        private bool NewGame(bool isInitialCall)
        {
            currGameIndex = Utilities.rng.Next(steam.User.Games.Count);
            currGame = steam.User.Games.ElementAt(currGameIndex);

            currGame.Name = SteamAPI.GetAppNameFromId(currGame.AppID);

            if (currGame.Name == "App no longer supported") //Some "games" are alphas or betas that no longer work 
            { 
                NewGame(false);
            }

            //string message = isInitialCall ? "How about this game?" : "No?  What about this?";
            //DisplayInfoOrError(message, 2250);

            tbGameName.Text = currGame.Name;
            labelGameTime.Content = String.Format("You have {0} hours in this game.", (currGame.HoursPlayed / 60.0f).ToString("0.0")); //Steam's API returns time played in mins.  This converts to hours

            gameSelector.Background = new ImageBrush(new BitmapImage(new Uri(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/page_bg_generated_v6b.jpg", currGame.AppID))));
            gameImage.Source = new BitmapImage(new Uri(String.Format("http://cdn.akamai.steamstatic.com/steam/apps/{0}/header_292x136.jpg", currGame.AppID)));
            

            return true;

        }


        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {            
            logon.Visibility = System.Windows.Visibility.Visible;
            gameSelector.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {            
            if(logon.Visibility == System.Windows.Visibility.Visible || gameSelector.Visibility == System.Windows.Visibility.Visible)
            {
                logonVisBefore = logon.Visibility;
                gameSelectorVisBefore = gameSelector.Visibility;

                logon.Visibility = gameSelector.Visibility = System.Windows.Visibility.Collapsed;
                //TODO: SHOW SETTINGS
            }
            else
            {
                logon.Visibility = logonVisBefore;
                gameSelector.Visibility = gameSelectorVisBefore;
            }      
        }

        private void ButtonLaunchOrDownload_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(currGame.AppID)) //better safe than sorry
            {
                DisplayInfoOrError("No game is currently selected.  How did this happen?", 5000, true);
                return;
            }

            //TODO: CHANGE THIS TO USE SETTINGS FOR STEAM'S LOCATION
            string location = @"C:\Program Files (x86)\Steam\Steam.exe";
            Process.Start(location, String.Format("-applaunch {0}", currGame.AppID));

            /*
            buttonLaunchOrDownload.Content = null;

            Image image = new Image();
            image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "resources/images/download_64x64.png"));
            image.Width = 64;
            image.Height = 64;

            StackPanel stackPan = new StackPanel();
            stackPan.Orientation = Orientation.Horizontal;
            stackPan.Children.Clear();
            stackPan.Children.Add(image);


            buttonLaunchOrDownload.Content = stackPan;

            
            Uri res = new Uri(".../.../resources/images/download_64x64.png", UriKind.Relative);
            System.Windows.Resources.StreamResourceInfo stream = Application.GetRemoteStream(res);

            BitmapFrame temp = BitmapFrame.Create(stream.Stream);

            var brush = new ImageBrush();
            brush.ImageSource = temp;

            buttonLaunchOrDownload.Background = brush;//new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this) ,"resources/images/download_64x64.png")));
            

            ImageBrush brush1 = new ImageBrush();
            BitmapImage image = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "resources/images/download_64x64.png"));
            brush1.ImageSource = image;

            buttonLaunchOrDownload.Background = brush1;
             * */

            Console.WriteLine("Past background in buttonLaunchOrDownload_Click");
        }

        private void ButtonNewRandomGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame(false);
        }

        #endregion

        private void RadioCustomURL_Click(object sender, RoutedEventArgs e)
        {
            tbLink.Text = "steamcommunity.com/id/";
        }

        private void RadioSteamID_Click(object sender, RoutedEventArgs e)
        {
            tbLink.Text = "steamcommunity.com/profiles/";
        }

        private void GameImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(String.Format("http://store.steampowered.com/app/{0}", currGame.AppID));
            }
            catch(Exception ex)
            {
                DisplayInfoOrError(ex.Message, 5000, true);
            }
            
        }

        
       

    }
}
