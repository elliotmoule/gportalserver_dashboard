using Newtonsoft.Json;
using Service.GameServer;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace G_PortalServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            QueryCode = Properties.Settings.Default.QueryCode;
            GetServerDetailsAsync();
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, (s, ev) => TimerTick(s, ev), Application.Current.Dispatcher);
            _counter = new TimeSpan(0, 0, 30);
            _timer.Start();
            RefreshState = true;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Countdown = _counter.ToString("c");
            if (_counter != TimeSpan.Zero)
            {
                _counter = _counter.Add(TimeSpan.FromSeconds(-1));
            }
            else
            {
                RefreshState = false;
                _timer.Stop();
                _counter = new TimeSpan(0, 0, 30);
                GetServerDetailsAsync();
                _timer.Start();
                RefreshState = true;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private async Task GetServerDetailsAsync()
        {
            StatusBuilder(G_PortalServer.Status.Loading);
            try
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"https://api.g-portal.us/gameserver/query/{QueryCode}");
                GameServerInformation server = null;

                if (response != null && response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    server = JsonConvert.DeserializeObject<GameServerInformation>(result);
                    StatusBuilder(G_PortalServer.Status.Ready);
                }

                if (server != null)
                {
                    CurrentPlayers = server.CurrentPlayers;
                    MaxPlayers = server.MaxPlayers;
                    Key = server.Key;
                    Port = server.Port;
                    IPAddress = server.IPAddress;
                    ServerName = server.Name;
                    ToggleOnlineState(server.Online);
                    PlayerText();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StatusBuilder(G_PortalServer.Status.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetServerDetailsAsync();
        }

        private string _status;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StatusBuilder(Status status)
        {
            Status = $"{status}...";
        }

        private void ToggleOnlineState(bool online)
        {
            Online = online ? Brushes.Green : Brushes.Red;
        }

        private void PlayerText()
        {
            Players = $"{CurrentPlayers}/{MaxPlayers}";
        }

        private TimeSpan _counter;

        #region Properties
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyChanged(nameof(Status));
            }
        }

        private Brush _online = Brushes.Red;

        public Brush Online
        {
            get { return _online; }
            set
            {
                _online = value;
                NotifyChanged(nameof(Online));
            }
        }

        private string _players;

        public string Players
        {
            get { return _players; }
            set
            {
                _players = value;
                NotifyChanged(nameof(Players));
            }
        }

        private int _maxPlayers;

        public int MaxPlayers
        {
            get { return _maxPlayers; }
            set
            {
                _maxPlayers = value;
                NotifyChanged(nameof(MaxPlayers));
            }
        }

        private int _currentPlayers;

        public int CurrentPlayers
        {
            get { return _currentPlayers; }
            set
            {
                _currentPlayers = value;
                NotifyChanged(nameof(CurrentPlayers));
            }
        }

        private string _ipAddress;

        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                NotifyChanged(nameof(IPAddress));
            }
        }

        private int _port;

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                NotifyChanged(nameof(Port));
            }
        }

        private string _key;

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                NotifyChanged(nameof(Key));
            }
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                NotifyChanged(nameof(ServerName));
            }
        }

        private string _countdown;

        public string Countdown
        {
            get { return _countdown; }
            set
            {
                _countdown = value;
                NotifyChanged(nameof(Countdown));
            }
        }

        private bool _refreshState;

        public bool RefreshState
        {
            get { return _refreshState; }
            set
            {
                _refreshState = value;
                NotifyChanged(nameof(RefreshState));
            }
        }

        private string _queryCode;

        public string QueryCode
        {
            get { return _queryCode; }
            set
            {
                _queryCode = value;
                NotifyChanged(nameof(QueryCode));
            }
        }
        #endregion

        private void Minimise(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            App.Current.Shutdown();
        }

        private void SetQueryCode(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.QueryCode = QueryCode;
            Properties.Settings.Default.Save();
        }
    }

    public enum Status
    {
        Loading,
        Ready,
        Error
    }
}
