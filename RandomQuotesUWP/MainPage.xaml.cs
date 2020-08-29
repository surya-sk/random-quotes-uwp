using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Composition;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RandomQuotesUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Quotes quotes = new Quotes();
        HttpClient client;
        DispatcherTimer dispatcherTimer;
        bool ReadQuote;
        string url = "https://yourrandomquotes.herokuapp.com/quote";//tp://localhost:8081/quote";
        public MainPage()
        {
            this.InitializeComponent();
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            LoadQuote();
            UpdateLiveTile();
            DispatcherTimerSetup();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }


        /// <summary>
        /// Reads the quote aloud
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadAloud()
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(quotes.Quote);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }


        /// <summary>
        /// Returns the json string from the url
        /// </summary>
        /// <returns>the string in JSON format</returns>
        public async Task<Quotes> GetStringFromURL()
        {
            System.Net.Http.HttpResponseMessage result = await client.GetAsync(url).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                string jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                quotes = JsonConvert.DeserializeObject<Quotes>(jsonString);
            }
            return quotes;
        }

        async Task<Quotes> GetResponse()
        {

            quotes = await GetStringFromURL().ConfigureAwait(false);

            return quotes;
        }

        /// <summary>
        /// A class that helps in parsing the JSON strings to simplify access in other methods
        /// </summary>
        public class Quotes : INotifyPropertyChanged
        {
            private string quote;
            private string by;

            public string By
            { get => by; set
                {
                    by = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(By)));
                }
            }
            public string Quote { get => quote; set
                {
                    quote = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quote)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        /// <summary>
        /// Displays a new quote
        /// </summary>
        private void LoadQuote()
        {
            quotes = GetResponse().Result;
            DataContext = new Quotes() { By = quotes.By, Quote = quotes.Quote };
            if (quotes.Quote.Length < 300)
            {
                Quote.TextWrapping = TextWrapping.Wrap;
            }
            if(ReadQuote)
            {
                ReadAloud();
            }
        }

        /// <summary>
        /// Start a timer to display a new quote using intervals
        /// </summary>
        private void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcher_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 60);
            dispatcherTimer.Start();

        }

        private void dispatcher_Tick(object sender, object e)
        {
            LoadQuote();
            UpdateLiveTile();
        }

        /// <summary>
        /// Updates the live tile to show the current quote
        /// </summary>
        private void UpdateLiveTile()
        {
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {

                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        DisplayName = "Quotes",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Quote By",
                        HintWrap = true,
                        HintMaxLines = 2
                    },
                    new AdaptiveText()
                    {
                        Text = quotes.By,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = "Random Quotes UWP",
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Current Quote"
                    },
                    new AdaptiveText()
                    {
                        Text = quotes.By,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = quotes.Quote,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                        HintMinLines = 2,
                        HintMaxLines = 4,
                        HintWrap = true
                    }
                }
                        }
                    },
                }
            };

            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }

        private void ReadAloudButton_Checked(object sender, RoutedEventArgs e)
        {
            ReadQuote = true;
            ReadAloud();
        }

        private void ReadAloudButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ReadQuote = false;
        }
    }
}
