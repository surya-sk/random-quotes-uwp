using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RandomQuotesUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuotePage : Page
    {
        Quotes quotes = new Quotes();
        HttpClient client;
        DispatcherTimer dispatcherTimer;
        string url = "https://yourrandomquotes.herokuapp.com/quote";//tp://localhost:8081/quote";

        public QuotePage()
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




        //private async Task UpdateQuote()
        //{
        //    //bool done = false;

        //    //while (!done)
        //    //{
        //    //    done = await DeserializeJson();
        //    //    Quote.Text = quotes.quote;
        //    //    Author.Text = "-" + quotes.by;
        //    //    if (quotes.quote.Length > 20)
        //    //    {
        //    //        Quote.TextWrapping = TextWrapping.Wrap;
        //    //    }
        //    //}
        //}

        /// <summary>
        /// Returns the json string from the url
        /// </summary>
        /// <returns>the string in JSON format</returns>
        public async Task<Quotes> GetStringFromURL()
        {
            //string url = "https://yourrandomquotes.herokuapp.com/quote";
            //ttpClient client = new HttpClient();

            //client.BaseAddress = new Uri(url);
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            System.Net.Http.HttpResponseMessage result = await client.GetAsync(url).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                string jsonString = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                quotes = JsonConvert.DeserializeObject<Quotes>(jsonString);

                //Quote.Text = quotes.quote;
                //Author.Text = quotes.by;

            }




            return quotes;

        }

        async Task<Quotes> GetResponse()
        {

            quotes = await GetStringFromURL().ConfigureAwait(false);

            return quotes;
        }

        /// <summary>
        /// Parsing the JSON file
        /// </summary>
        //private async Task<bool> DeserializeJson()
        //{
        //    bool done = false;
        //    bool status = false;

        //    while (!done)
        //    {
        //        quotes = JsonConvert.DeserializeObject<Quotes>(GetStringFromURL());
        //        status = quotes != null;
        //        await Task.Yield();
        //        return status;
        //    }

        //    return status;
        //}



        /// <summary>
        /// A class that helps in parsing the JSON strings to simplify access in other methods
        /// </summary>
        public class Quotes
        {
            public string quote { get; set; }
            public string by { get; set; }
        }
        /// <summary>
        /// When clicked, gets a new quote
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewQuote_Click(object sender, RoutedEventArgs e)
        {
            //
        }




        /// <summary>
        /// Displays a new quote
        /// </summary>
        private void LoadQuote()
        {
            quotes = GetResponse().Result;
            Quote.Text = quotes.quote;
            Author.Text = quotes.by;

            if (quotes.quote.Length < 300)
            {
                Quote.TextWrapping = TextWrapping.Wrap;
            }
        }

        private void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcher_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();
            
        }

        private void dispatcher_Tick(object sender, object e)
        {
            LoadQuote();
            UpdateLiveTile();
                
        }
        

        /// <summary>
        /// Reads the quote aloud
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Read_Click(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(quotes.quote);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
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
                        Text = quotes.by,
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
                        Text = quotes.by,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },
                    new AdaptiveText()
                    {
                        Text = quotes.quote,
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

    }
}
