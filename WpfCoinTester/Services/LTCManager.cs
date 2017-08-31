using DataLayer;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfLiteCoinTester
{
    public class LTCManager : CoinManager, IDisposable
    {
        private IPersistenceService persistenceService;

        AutoResetEvent sendConfirmationResetEvent = new AutoResetEvent(false);
        AutoResetEvent receivedInfoResetEvent = new AutoResetEvent(false);
        AutoResetEvent receiveListSinceBlockResetEvent = new AutoResetEvent(false);

        private readonly int minConfirmations = 24;

        public LTCManager(Listener messenger, IPersistenceService persistenceService) : base(messenger)
        {
            this.persistenceService = persistenceService;

            Task.Factory.StartNew(() =>
            {
                messenger.Listen(sendConfirmationResetEvent, ReceivedSendCoinConfirmation, "sendconfirmation");
            });

            Task.Factory.StartNew(() =>
            {
                messenger.Listen(receivedInfoResetEvent, ReceivedInfo, "info");
            });

            Task.Factory.StartNew(() =>
            {
                messenger.Listen(receiveListSinceBlockResetEvent, ReceiveListSinceBlock, "listsinceblock");
            });

            //TimingTasks.TimedCall(InvokeHeartbeat, 2);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            InvokeHeartbeat();
        }

        private void InvokeHeartbeat()
        {
            var lastBlock = this.persistenceService.GetLastBlock();

            var message = new Dictionary<string, IList<object>> { { "heartbeat", new object[] { lastBlock, minConfirmations } } };

            messenger.Publish("command", message);
        }

        protected override void ReceiveListSinceBlock(object arg1, byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);

            var listSinceBlockResult = JsonConvert.DeserializeObject<GetSinceBlock.RootObject>(message);

            this.persistenceService.UpdateHeartBeatTransaction(listSinceBlockResult);

            if (listSinceBlockResult == null) return;

            if (listSinceBlockResult.transactions != null)
            {
                this.persistenceService.CheckIncomingCoin(listSinceBlockResult.transactions.Where(t => t.confirmations >= minConfirmations).ToList());
            }

            base.ReceiveListSinceBlock(arg1, body);
        }

        public void ReceivedSendCoinConfirmation(object arg1, byte[] body)
        {
            var message = Encoding.UTF8.GetString(body);

            var info = message.Split(',');

            var oldTmpTxId = info[0];
            var txId = info[1];

            this.persistenceService.ReceivedSendCoinConfirmation(oldTmpTxId, txId);
        }

        public void Dispose()
        {
            this.sendConfirmationResetEvent.Set();
            this.receivedInfoResetEvent.Set();
            this.receiveListSinceBlockResetEvent.Set();
        }
    }
}
