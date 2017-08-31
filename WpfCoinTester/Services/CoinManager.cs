using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace WpfLiteCoinTester
{
    public abstract class CoinManager : ICoinManager
    {
        protected readonly RabbitMQMessenger messenger;
        public event WalletInfoEventHandler UpdateWalletInfo;
        public event UpdateBalanceEventHandler UpdateBalance;

        public CoinManager(RabbitMQMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void SendLiteCoin(string tmpTxId, double actionLTCAmount, string actionAddress)
        {
            var message = new Dictionary<string, IList<object>> { { "sendcoin", new object[] { actionAddress, tmpTxId, actionLTCAmount } } };

            messenger.Publish("command", message);
        }

        protected virtual void ReceiveListSinceBlock(object arg1, byte[] body)
        {
            UpdateBalance?.Invoke();
        }

        protected virtual void ReceivedInfo(object arg1, byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);

            var info = JsonConvert.DeserializeObject<WalletInfo.RootObject>(message);

            if (info == null) return;

            if(info.keypoolsize < 2000)
            {
                var kpmessage = new Dictionary<string, IList<object>> { { "keypool", new object[] { 2000 } } };

                messenger.Publish("command", kpmessage);
            }

            UpdateWalletInfo?.Invoke(info);
        }
    }
}
