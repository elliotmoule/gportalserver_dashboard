using CODE.Framework.Services.Client;
using CODE.Framework.Wpf.Mvvm;
using G_PortalServer.Contract;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        #region Ctor
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            DataContext = this;
            QueryCodeSavedVisibility = Visibility.Hidden;
            StatusBuilder(G_PortalServer.Status.Unset);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Load & Display
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            QueryCode = Properties.Settings.Default.QueryCode;
            if (!string.IsNullOrWhiteSpace(QueryCode))
                QueryCodeSavedVisibility = Visibility.Visible;
            GetServerDetails();
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, (s, ev) => TimerTick(s, ev), Application.Current.Dispatcher);
            _counter = new TimeSpan(0, 0, 30);
            _timer.Start();
            RefreshState = true;
        }

        private void GetServerDetails()
        {
            if (string.IsNullOrWhiteSpace(QueryCode))
            {
                StatusBuilder(G_PortalServer.Status.Unset);
                return;
            }
            RefreshState = false;
            StatusBuilder(G_PortalServer.Status.Loading);
            ToggleOnlineState(null);
            try
            {
                AsyncWorker.Execute(() =>
                {
                    GetGameServerResponse response = null;
                    var request = new GetGameServerRequest
                    {
                        QueryCode = QueryCode
                    };

                    ServiceClient.Call<IGameServerService>(s => { response = s.GetGameServer(request); });
                    return response;
                }, response =>
                {
                    if (response != null)
                    {
                        var server = response.Server;
                        CurrentPlayers = server.CurrentPlayers;
                        MaxPlayers = server.MaxPlayers;
                        Key = server.Key;
                        Port = server.Port;
                        IPAddress = server.IPAddress;
                        ServerName = server.Name;
                        ToggleOnlineState(server.Online);
                        PlayerText();
                        StatusBuilder(G_PortalServer.Status.Ready);
                        BuildGameAddress();
                    }
                    else
                    {
                        StatusBuilder(G_PortalServer.Status.Error);
                    }
                    RefreshState = true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StatusBuilder(G_PortalServer.Status.Error);
            }
        }

        private void BuildGameAddress()
        {
            if (!string.IsNullOrWhiteSpace(IPAddress) && Port > 0)
                GameAddress = $"{IPAddress}:{Port - 1}";
        }

        private void StatusBuilder(Status status)
        {
            Status = $"{status}...";
        }

        private void ToggleOnlineState(bool? online)
        {
            OnlineStatusTooltip = "Server is currently ";
            Online = online.HasValue ? (online.Value ? Brushes.Green : Brushes.Red) : Brushes.Blue;
            if (online.HasValue)
            {
                if (online.Value)
                {
                    Online = Brushes.Green;
                    OnlineStatusTooltip += "Online!";
                }
                else
                {
                    Online = Brushes.Red;
                    OnlineStatusTooltip += "Offline!";
                }
            }
            else
            {
                Online = Brushes.Blue;
                OnlineStatusTooltip += "Being Checked!";
            }
        }

        private void PlayerText()
        {
            Players = $"{CurrentPlayers}/{MaxPlayers}";
        }
        #endregion

        #region Actions
        private void TimerTick(object sender, EventArgs e)
        {
            Countdown = _counter.ToString("c");
            if (_counter != TimeSpan.Zero)
            {
                CountDown();
            }
            else
            {
                ResetTimer();
            }
        }

        private void CountDown()
        {
            _counter = _counter.Add(TimeSpan.FromSeconds(-1));
        }

        private void ResetTimer()
        {
            RefreshState = false;
            _timer.Stop();
            _counter = new TimeSpan(0, 0, 30);
            GetServerDetails();
            _timer.Start();
            RefreshState = true;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryCode))
            {
                MessageBox.Show("Please provide a query code.");
            }
            else
            {
                ResetTimer();
                GetServerDetails();
            }
        }

        private void Minimise_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            App.Current.Shutdown();
        }

        private void SetQueryCode(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.QueryCode = QueryCode;
            Properties.Settings.Default.Save();
            QueryCodeSavedVisibility = Visibility.Visible;
        }

        private void CopyIPAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(GameAddress))
                Clipboard.SetText(GameAddress);
        }

        private void QueryCode_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            QueryCodeSavedVisibility = Visibility.Hidden;
        }
        #endregion

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

        private string _gameAddress;

        public string GameAddress
        {
            get { return _gameAddress; }
            set
            {
                _gameAddress = value;
                NotifyChanged(nameof(GameAddress));
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

        private Visibility _queryCodeSavedVisibility;

        public Visibility QueryCodeSavedVisibility
        {
            get { return _queryCodeSavedVisibility; }
            set
            {
                _queryCodeSavedVisibility = value;
                NotifyChanged(nameof(QueryCodeSavedVisibility));
            }
        }

        private string _onlineStatusTooltip;

        public string OnlineStatusTooltip
        {
            get { return _onlineStatusTooltip; }
            set
            {
                _onlineStatusTooltip = value;
                NotifyChanged(nameof(OnlineStatusTooltip));
            }
        }
        #endregion

        #region Fields
        private DispatcherTimer _timer;
        private TimeSpan _counter;
        private string _status;
        #endregion
    }

    public enum Status
    {
        Loading,
        Ready,
        Error,
        Unset
    }
}
