using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace TestClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        private int _selectedId;

        public MainWindow()
        {
            InitializeComponent();
            var timer = new DispatcherTimer {Interval = new TimeSpan(0, 5, 0)};
            timer.Tick += TimerTick;
            timer.Start();
            UpdateList();
        }

        private void TimerTick(object source, EventArgs e)
        {
            UpdateList();
        }

        private void UpdateList()
        {
            var url = ConfigurationManager.AppSettings["movieUrl"] + "api/Movies/";
            try
            {
                var webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                var str = webClient.DownloadString(url);
                var jarray = JArray.Parse(str);
                var lst = jarray.Select(item => new SelectItem
                {
                    Id = Convert.ToInt32(item["Id"].ToString()),
                    Date = Convert.ToDateTime(item["Date"]),
                    Name = item["Name"].ToString()
                }).ToList();
                lst.Sort((item1, item2)=>item1.Date.CompareTo(item2.Date));
                MovieList.ItemsSource = lst;
                foreach (var o in MovieList.Items)
                {
                    if (!(o is SelectItem) || (o as SelectItem).Id != _selectedId) continue;
                    MovieList.SelectedItem = o;
                    break;
                }
            }
            catch (WebException exception)
            {
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Продажа билетов");
            }
        }

        private void movieList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var url = ConfigurationManager.AppSettings["movieUrl"] + "api/Tickets/";
            try
            {
                var item = MovieList.SelectedItem;
                if (!(item is SelectItem)) return;
                var selectedId = ((SelectItem) MovieList.SelectedItem).Id;
                // если выбран новый фильм, запрос кол-ва билетов
                if (selectedId == _selectedId) return;
                _selectedId = selectedId;
                var webClient = new WebClient();
                
                webClient.QueryString.Add("id", _selectedId.ToString());
                var str = webClient.DownloadString(url);
                LabelCount.Content = str;
                TextCount.Text = "0";
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Продажа билетов");
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            var url = ConfigurationManager.AppSettings["movieUrl"] + "api/Tickets/";
            try
            {
                int count;
                int.TryParse(TextCount.Text, out count);
                if(count<=0) throw new Exception("Количество должно быть больше 0");
                using (var webClient = new WebClient())
                {
                    var pars = new NameValueCollection
                    {
                        {"Count", TextCount.Text},
                        {"MovieId", _selectedId.ToString()}
                    };
                    var response = webClient.UploadValues(url, pars);
                    var answer = Encoding.UTF8.GetString(response);
                    LabelResult.Content = answer;
                }
                var timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 3)};
                timer.Tick += (sndr, ev) =>
                {
                    LabelResult.Content = "";
                    timer.Stop();
                    timer = null;
                    ClearFields();
                };
                timer.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Продажа билетов");
            }
        }

        private void ClearFields()
        {
            _selectedId = 0;
            MovieList.SelectedIndex = -1;
            LabelCount.Content = "0";
            TextCount.Text = "0";
        }
    }
}
