using System.Collections.Generic;
using System.Threading.Tasks;
using WpfLiteCoinTester;

namespace DataLayer
{
    public interface IPersistenceService
    {
        Task<bool> ValidateRequest(int id, double actionLTCAmount, string actionAddress);
        Task<bool> AddTransaction(string txid, int pendingOut, double actionLTCAmount, string actionAddress, int userId);
        void OffsetIncomingLTC(int userId);
        IList<IUser> GetUsers();
        IUser GetDefaultUser();
        IList<IBalance> GetBalances(IUser selectedUser);
        void Deposit(double amount, int userId, int type);
        Task<bool> UpdateUserLTCCount(int id, double actionLTCAmount);
        string GetLastBlock();
        void CheckIncomingCoin(IList<GetSinceBlock.Transaction> coinTransactions);
        void UpdateHeartBeatTransaction(GetSinceBlock.RootObject listSinceBlockResult);
        void ReceivedSendCoinConfirmation(string oldTmpTxId, string txId);
    }
}
