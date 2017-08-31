namespace WpfLiteCoinTester
{
    public delegate void WalletInfoEventHandler(WalletInfo.RootObject info);
    public delegate void UpdateBalanceEventHandler();
    public interface ICoinManager
    {
        event WalletInfoEventHandler UpdateWalletInfo;
        event UpdateBalanceEventHandler UpdateBalance;
        void SendLiteCoin(string tmpTxId, double actionLTCAmount, string actionAddress);
    }
}
