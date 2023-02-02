using System;
using System.Net;
using System.Collections.Generic;
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
using System.Net.Http;
using System.Windows.Markup;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Win32;
using System.Net.NetworkInformation;

namespace eftMarket
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<itemsPrice> marketList = new List<itemsPrice>();
        public List<Items> items = new List<Items>();
        public int threadI;
        bool stop = false;
        public MainWindow()
        {
            InitializeComponent();
            listItems.Items.Clear();
        }

        private async void loadNames_Click(object sender, RoutedEventArgs e)
        {
            try { await LoadItems(); }
            catch (SystemException _e)
            {
                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Stop reading error...");
                MessageBox.Show(_e.Message, _e.TargetSite.Name, MessageBoxButton.OK);
                return;
            }
        }

        private async void loadPrices_Click(object sender, RoutedEventArgs e)
        {
            loadNames.IsEnabled = false;
            try
            {
                await LoadPrices();
            }
            catch (Exception _e)
            {

                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Stop reading error...");
                MessageBox.Show(_e.Message, _e.TargetSite.Name, MessageBoxButton.OK);
                marketList.Add(null);
                return;
            }

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txt.Text = openFileDialog.FileName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ping.IsEnabled = false;
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            for (int i = 0; i < 4; i++)
            {
                PingReply reply = pingSender.Send("api.tarkov.dev", timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    log.Items.Add($"<-----------------Ping{i}----------------->");
                    log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                $"{DateTime.Now.TimeOfDay.Milliseconds}] Address: {reply.Address}");
                    log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                $"{DateTime.Now.TimeOfDay.Milliseconds}] RoundTrip time: {reply.RoundtripTime}");
                    log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                $"{DateTime.Now.TimeOfDay.Milliseconds}] Time to live: {reply.Options.Ttl}");
                    log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                $"{DateTime.Now.TimeOfDay.Milliseconds}] Don't fragment: {reply.Options.DontFragment}");
                    log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                $"{DateTime.Now.TimeOfDay.Milliseconds}] Buffer size: {reply.Buffer.Length}");
                    log.Items.Add("<--------------------------------------->");
                }
            }
            ping.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            log.Items.Clear();
        }

        public async Task LoadItems()
        {

            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}" +
                        $":{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Start reading...");
            double a;
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Reading...");
            a = DateTime.Now.TimeOfDay.TotalMilliseconds;
            try
            {
                var json = File.OpenRead(txt.Text);
                List<Items> items = await JsonSerializer.DeserializeAsync<List<Items>>(json);
                json.Close();
                //MainWindow main = new MainWindow(items);
                this.items = items;
                progress.Maximum = items.Count;
                foreach (var i in items)
                {
                    progress.Value += 1;
                    textProgress.Content = $"{progress.Value}/{items.Count}";
                    listItems.Items.Add(i.normalized_name.Replace('-', ' '));
                }
            }
            catch (SystemException e)
            {
                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Stop reading error...");
                MessageBox.Show(e.Message, e.TargetSite.Name, MessageBoxButton.OK);
                return;
            }
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Read for end...      [{Math.Round(DateTime.Now.TimeOfDay.TotalMilliseconds - a)} ms]");
            loadPrices.IsEnabled = true;
        }

        object locker = new object();

        public async Task LoadPrices()
        {
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}" +
                        $":{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Start download prices...");
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Download...");
            progress.Maximum = this.items.Count;
            progress.Value = marketList.Count;
            double a = DateTime.Now.TimeOfDay.Milliseconds;
            try
            {
                

                for (int i  = marketList.Count; i < this.items.Count; i++)
                {
                    var data = new Dictionary<string, string>()
                    {

                        {"query", $"{{items(ids: \"{this.items.ElementAt(i).id}\") {{ sellFor {{ source, price }} }}}}"}
                    };

                    using (var httpClient = new HttpClient())
                    {

                        //Http response message
                        var httpResponse = httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);

                        //Response content
                        var responseContent = await httpResponse.Result.Content.ReadAsStringAsync();

                        string newContetnt = responseContent.ToString().Replace("{\"data\":{\"items\":[", "");
                        newContetnt = newContetnt.Replace("]}}", "");
                        var jsonC = File.Create($"Prices/{this.items.ElementAt(i).id}.prc");
                        jsonC.Close();
                        var jsonW = File.CreateText($"Prices/{this.items.ElementAt(i).id}.prc");
                        jsonW.Write(newContetnt);
                        jsonW.Close();
                        /*var jsonR = File.OpenRead("Temp.tmp");
                        marketList.Add(JsonSerializer.Deserialize<itemsPrice>(jsonR));
                        jsonR.Close();*/
                    }
                }

            }
            catch (SystemException e)
            {
                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Stop download error...");
                MessageBox.Show(e.Message, e.TargetSite.Name, MessageBoxButton.OK);
                return;
            }
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Download for end...      [{Math.Round(DateTime.Now.TimeOfDay.TotalMilliseconds - a)} ms]");
        }

        void threadLoadAndWrite()
        {
            lock (locker)
            {
                for (int i = threadI; i < threadI + 5; i++)
                {
                    var data = new Dictionary<string, string>()
                    {

                        {"query", $"{{items(ids: \"{this.items.ElementAt(i).id}\") {{ sellFor {{ source, price }} }}}}"}
                    };

                    using (var httpClient = new HttpClient())
                    {

                        //Http response message
                        var httpResponse = httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);

                        //Response content
                        var responseContent = httpResponse.Result.Content.ReadAsStringAsync();

                        string newContetnt = responseContent.ToString().Replace("{\"data\":{\"items\":[", "");
                        newContetnt = newContetnt.Replace("]}}", "");
                        var jsonC = File.Create($"Prices/{this.items.ElementAt(i).id}.prc");
                        jsonC.Close();
                        var jsonW = File.CreateText($"Prices/{this.items.ElementAt(i).id}.prc");
                        jsonW.Write(newContetnt);
                        jsonW.Close();
                        /*var jsonR = File.OpenRead("Temp.tmp");
                        marketList.Add(JsonSerializer.Deserialize<itemsPrice>(jsonR));
                        jsonR.Close();*/
                    }
                }

            }

        }

        public async Task Find()
        {
            removePrice();

            int[] MastHavePrice = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            int i = listItems.SelectedIndex;
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Find item: {listItems.SelectedItem}");
            if (i == -1)
            {
                MessageBox.Show("Элемент не выбран", "Error...", MessageBoxButton.OK);
                return;
            }
            foreach (var item in marketList[i].sellFor)
            {
                switch (item.source)
                {
                    case "prapor":
                        {
                            prapor.Content = item.price;
                            MastHavePrice[0] = (int)item.price;
                            break;
                        }
                    case "therapist":
                        {
                            therapist.Content = item.price;
                            MastHavePrice[1] = (int)item.price;
                            break;
                        }
                    case "fence":
                        {
                            fence.Content = item.price;
                            MastHavePrice[2] = (int)item.price;
                            break;
                        }
                    case "skier":
                        {
                            skier.Content = item.price;
                            MastHavePrice[3] = (int)item.price;
                            break;
                        }
                    case "peacekeeper":
                        {
                            peacekeeper.Content = item.price;
                            MastHavePrice[4] = (int)item.price;
                            break;
                        }
                    case "mechanic":
                        {
                            mechanic.Content = item.price;
                            MastHavePrice[5] = (int)item.price;
                            break;
                        }
                    case "ragman":
                        {
                            ragman.Content = item.price;
                            MastHavePrice[6] = (int)item.price;
                            break;
                        }
                    case "jaeger":
                        {
                            jaeger.Content = item.price;
                            MastHavePrice[7] = (int)item.price;
                            break;
                        }
                    case "fleaMarket":
                        {
                            fleaMarket.Content = item.price;
                            MastHavePrice[8] = (int)item.price;
                            break;
                        }
                    default:
                        break;
                }

                slotPrice.Content = (items[i].base_price / (items[i].height * items[i].width));

            }
            List<int> MastHavePriceTemp = MastHavePrice.ToList<int>();
            MastHavePriceTemp.Sort();
            for (int k = 0; k < MastHavePriceTemp.Count; k++)
            {
                if (MastHavePriceTemp[9] == MastHavePrice[k])
                {
                    switch (k)
                    {
                        case 0:
                            {
                                prapor.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 1:
                            {
                                therapist.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 2:
                            {
                                fence.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 3:
                            {
                                skier.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 4:
                            {
                                peacekeeper.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 5:
                            {
                                mechanic.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 6:
                            {
                                ragman.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 7:
                            {
                                jaeger.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        case 8:
                            {
                                fleaMarket.Background = Brushes.Green;
                                slotPrice.Content = (MastHavePriceTemp[9] / (items[k].height * items[k].width));
                                size.Content = (items[k].height * items[k].width);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(items[i].icon_link);
            myBitmapImage.EndInit();
            image.Source = myBitmapImage;
        }

        public void removePrice()
        {
            SolidColorBrush brushes = new SolidColorBrush(Color.FromArgb(100, 74, 74, 74));
            prapor.Content = "";
            therapist.Content = "";
            fence.Content = "";
            skier.Content = "";
            peacekeeper.Content = "";
            mechanic.Content = "";
            ragman.Content = "";
            jaeger.Content = "";
            fleaMarket.Content = "";
            slotPrice.Content = "";
            size.Content = "";
            prapor.Background = brushes;
            therapist.Background = brushes;
            fence.Background = brushes;
            peacekeeper.Background = brushes;
            mechanic.Background = brushes;
            ragman.Background = brushes;
            jaeger.Background = brushes;
            fleaMarket.Background = brushes;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            stop = true;
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                await Find();
            }
            catch (Exception _e)
            {

                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                                    $"{DateTime.Now.TimeOfDay.Minutes}:" +
                                    $"{DateTime.Now.TimeOfDay.Seconds}:" +
                                    $"{DateTime.Now.TimeOfDay.Milliseconds}] Find error...");
                MessageBox.Show(_e.Message, _e.TargetSite.Name, MessageBoxButton.OK);
                return;
            }
        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }


    /*public class loadPrices : MainWindow
    {
        ListBox log;
        ComboBox box;
        ProgressBar progress;
        string path;
        public loadPrices(ListBox log, ComboBox box, string path, ProgressBar progress)
        {
            this.log = log;
            this.box = box;
            this.path = path;
            this.progress = progress;
        }

        public async Task Load()
        {
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}" +
                        $":{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Start download prices...");
            double a;
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Download...");

            var data = new Dictionary<string, string>()
            {
                {"query", $"{{items(id: \"{this.items.ElementAt(0).id}\") {{ id, name, shortName }}"}
            };

            using (var httpClient = new HttpClient())
            {

                //Http response message
                var httpResponse = await httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);

                //Response content
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                log.Items.Add(responseContent);
            }
        }
    }

    /*public class loadItems
    {
        ListBox log;
        ComboBox box;
        ProgressBar progress;
        Button button;
        string path;
        public loadItems(ListBox log, ComboBox box, string path, ProgressBar progress, Button button)
        {
            this.log = log;
            this.box = box;
            this.path = path;
            this.progress = progress;
            this.button = button;
        }

        public async Task Load()
        {
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}" +
                        $":{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Start reading...");
            double a;
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Reading...");
            a = DateTime.Now.TimeOfDay.TotalMilliseconds;
            try
            {
                var json = File.OpenRead(path);
                List<Items> items = await JsonSerializer.DeserializeAsync<List<Items>>(json);
                json.Close();
                MainWindow main = new MainWindow(items);
                progress.Maximum = items.Count;
                foreach (var i in items)
                {
                    progress.Value += 1;
                    box.Items.Add(i.normalized_name.Replace('-', ' '));
                }
            }
            catch (SystemException e)
            {
                log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                            $"{DateTime.Now.TimeOfDay.Minutes}:" +
                            $"{DateTime.Now.TimeOfDay.Seconds}:" +
                            $"{DateTime.Now.TimeOfDay.Milliseconds}] Stop reading error...");
                MessageBox.Show(e.Message, e.TargetSite.Name, MessageBoxButton.OK);
                return;
            }
            log.Items.Add($"[{DateTime.Now.TimeOfDay.Hours}:" +
                        $"{DateTime.Now.TimeOfDay.Minutes}:" +
                        $"{DateTime.Now.TimeOfDay.Seconds}:" +
                        $"{DateTime.Now.TimeOfDay.Milliseconds}] Read for end...      [{Math.Round(DateTime.Now.TimeOfDay.TotalMilliseconds - a)} ms]");
            button.IsEnabled = true;
        }
    }*/
}
