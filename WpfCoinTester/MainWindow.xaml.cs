using System.Windows;
using System.Windows.Input;
using System.Configuration;
using DataLayer;
using System.ComponentModel;

namespace WpfLiteCoinTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserManagementViewModel myViewModel;
        private readonly Listener messenger;

        private readonly IPersistenceService persistenceService = new SimplePersistenceService();
        private readonly LTCManager coinManager;

        public MainWindow()
        {
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            InitializeComponent();

            var hostname = ConfigurationManager.AppSettings["rabbitmqhostname"];

            this.messenger = new Listener(hostname, ConfigurationManager.AppSettings["rabbitmqusername"], ConfigurationManager.AppSettings["rabbitmqpassword"]);

            this.coinManager = new LTCManager(messenger, persistenceService);

            this.myViewModel = new UserManagementViewModel(this.coinManager, persistenceService, messenger, hostname);

            DataContext = myViewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.coinManager.Dispose();
            this.myViewModel.Dispose();

            base.OnClosing(e);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}