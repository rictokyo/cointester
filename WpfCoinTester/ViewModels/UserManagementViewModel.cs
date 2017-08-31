using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WalletInfo;

namespace WpfLiteCoinTester
{

    public class UserManagementViewModel : ViewModelBase, IDisposable
    {
        double exchange = 29.54;
        private double LimitBuyLiteCoin { get; set; } = 5;
        private IUser selectedUser;
        private IEnumerable<IBalance> balances;
        private readonly ICoinManager coinManager;
        private double incomingLTCAmount;
        private bool suspendActivity;
        private double ltcAmount;
        private RootObject walletInfo;
        private readonly IPersistenceService persistenceService;
        private readonly INotificationService notificationService = new NotificationService();

        public UserManagementViewModel(ICoinManager coinManager, IPersistenceService persistenceService, RabbitMQMessenger messenger, string hostname)
        {
            this.persistenceService = persistenceService;

            this.coinManager = coinManager;

            this.coinManager.UpdateBalance += CoinManager_UpdateBalance;
            this.coinManager.UpdateWalletInfo += CoinManager_UpdateWalletInfo;

            DepositJpyCommand = new RelayCommand(CanDepositJpy, DepositJpy);

            OffsetIncomingLTCCommand = new RelayCommand(CanOffsetIncomingLTC, o =>
            {
                Task.Run(() => this.persistenceService.OffsetIncomingLTC(SelectedUser.Id)).Wait();
                Balances = this.persistenceService.GetBalances(SelectedUser);
                UpdateIncomingCoin();
            });

            BuyLitecoinCommand = new RelayCommand(CanBuyLiteCoin, BuyLiteCoin);
            SendLitecoinCommand = new RelayCommand(CanSendLiteCoin, o => Task.Run(() => SendLiteCoin(o)));

            HostName = hostname;
        }

        private void CoinManager_UpdateWalletInfo(RootObject info)
        {
            WalletInfo = info;
        }

        private void CoinManager_UpdateBalance()
        {
            Balances = this.persistenceService.GetBalances(SelectedUser);
            UpdateIncomingCoin();

            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateIncomingCoin()
        {
            IBalance inLtc = Balances.FirstOrDefault(b => string.Compare("IN-LTC", b.Type, StringComparison.InvariantCultureIgnoreCase) == 0);

            ActionIncomingLTCAmount = inLtc != null ? inLtc.Amount : 0;
        }

        public bool SuspendActivity { get { return this.suspendActivity; } set { this.suspendActivity = value; OnPropertyChanged(); } }

        private async Task SendLiteCoin(object obj)
        {
            SuspendActivity = true;

            if (await this.persistenceService.ValidateRequest(SelectedUser.Id, ActionLTCAmount, ActionAddress))
            {
                await SendCoin();
            }
            else
            {
                if (this.notificationService.AskForConfirmation("Do you want to continue?", "Duplicate send detected"))
                {
                    await SendCoin();
                }
            }

            SuspendActivity = false;

            CommandManager.InvalidateRequerySuggested();
        }

        private async Task SendCoin()
        {
            if (await this.persistenceService.UpdateUserLTCCount(SelectedUser.Id, ActionLTCAmount * -1))
            {
                var newGuid = Guid.NewGuid().ToString();

                if (await this.persistenceService.AddTransaction(newGuid, (int)TransactionStatus.PendingOut, ActionLTCAmount, ActionAddress, SelectedUser.Id))
                {
                    this.coinManager.SendLiteCoin(newGuid, ActionLTCAmount, ActionAddress);
                }
            }
        }

        private bool CanSendLiteCoin(object obj)
        {
            var ltcBalance = Balances.Where(b => string.Compare(b.Type, "LTC", StringComparison.InvariantCultureIgnoreCase) == 0).Select(s => s.Amount).FirstOrDefault();

            return !SuspendActivity && !string.IsNullOrEmpty(ActionAddress) && ActionLTCAmount > 0 && ActionLTCAmount < ltcBalance;
        }

        public string ActionAddress { get; set; }

        public double ActionLTCAmount { get { return this.ltcAmount; } set { this.ltcAmount = value; OnPropertyChanged(); } }

        private void BuyLiteCoin(object obj)
        {
            var amount = ActionAmount;
            var userId = SelectedUser.Id;
            this.persistenceService.Deposit(amount, userId, 1);

            this.persistenceService.Deposit(GetExchangeResult() * -1, userId, 2);

            Balances = this.persistenceService.GetBalances(SelectedUser);
        }

        private double GetExchangeResult()
        {
            var ltc = ActionAmount;
            var jpy = ltc * exchange;

            return jpy;
        }

        private bool CanBuyLiteCoin(object obj)
        {
            bool withinLimit = 0 < ActionAmount && ActionAmount < LimitBuyLiteCoin;
            var jpyBalance = Balances.Where(b => string.Compare(b.Type, "JPY", StringComparison.InvariantCultureIgnoreCase) == 0).Select(s => s.Amount).FirstOrDefault();

            return withinLimit && jpyBalance > GetExchangeResult();
        }

        private bool CanOffsetIncomingLTC(object obj)
        {
            return ActionIncomingLTCAmount > 0;
        }

        private void DepositJpy(object obj)
        {
            var amount = ActionAmount;
            var userId = SelectedUser.Id;

            this.persistenceService.Deposit(amount, userId, 2);
            Balances = this.persistenceService.GetBalances(SelectedUser);
        }

        public double ActionIncomingLTCAmount
        {
            get { return this.incomingLTCAmount; }
            set
            {
                this.incomingLTCAmount = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public double ActionAmount { get; set; }

        private bool CanDepositJpy(object obj)
        {
            return ActionAmount > 0;
        }

        public void Dispose()
        {
            this.coinManager.UpdateBalance -= CoinManager_UpdateBalance;
            this.coinManager.UpdateWalletInfo -= CoinManager_UpdateWalletInfo;
        }

        public IUser DefaultUser { get { return this.persistenceService.GetDefaultUser(); } }

        public IEnumerable<IUser> Users
        {
            get
            {
                var users = this.persistenceService.GetUsers();
                SelectedUser = users.FirstOrDefault();

                return users;
            }
        }

        public IUser SelectedUser
        {
            get { return this.selectedUser; }
            set
            {
                if (this.selectedUser == value) return;

                this.selectedUser = value;

                OnPropertyChanged();
                Balances = this.persistenceService.GetBalances(SelectedUser);

                UpdateIncomingCoin();
            }
        }

        public IEnumerable<IBalance> Balances
        {
            get
            {
                return this.balances;
            }
            set
            {
                this.balances = value;
                OnPropertyChanged();
            }
        }

        public ICommand DepositJpyCommand { get; private set; }
        public ICommand OffsetIncomingLTCCommand { get; private set; }
        public ICommand BuyLitecoinCommand { get; private set; }
        public ICommand SendLitecoinCommand { get; private set; }
        public RootObject WalletInfo
        {
            get { return this.walletInfo; }
            set { this.walletInfo = value; OnPropertyChanged(); }
        }

        public string HostName { get; private set; }
    }
}
