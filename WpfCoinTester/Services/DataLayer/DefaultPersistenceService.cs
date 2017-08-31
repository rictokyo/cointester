using System.Collections.Generic;
using System.Data.Entity;
using GetSinceBlock;
using WpfLiteCoinTester;
using System.Threading.Tasks;

namespace DataLayer
{

    public abstract class DefaultPersistenceService<T> : IPersistenceService where T : DbContext, new()
    {
        private readonly object lockObj = new object();

        public abstract Task<bool> AddTransaction(string txid, int pendingOut, double actionLTCAmount, string actionAddress, int userId);
        public abstract void CheckIncomingCoin(IList<Transaction> coinTransactions);
        public abstract void Deposit(double amount, int userId, int type);
        public abstract IList<IBalance> GetBalances(IUser selectedUser);
        public abstract IUser GetDefaultUser();
        public abstract string GetLastBlock();
        public abstract IList<IUser> GetUsers();
        public abstract void OffsetIncomingLTC(int userId);
        public abstract void ReceivedSendCoinConfirmation(string oldTmpTxId, string txId);
        public abstract void UpdateHeartBeatTransaction(RootObject listSinceBlockResult);
        public abstract Task<bool> UpdateUserLTCCount(int id, double actionLTCAmount);
        public abstract Task<bool> ValidateRequest(int id, double actionLTCAmount, string actionAddress);

        protected virtual T GetEntityModel()
        {
            return new T();
        }

        protected virtual void SaveChanges(T ctx)
        {
            lock (lockObj)
            {
                ctx.SaveChanges();
            }
        }
    }
}
