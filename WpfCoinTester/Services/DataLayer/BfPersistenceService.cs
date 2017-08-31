using System;
using System.Collections.Generic;
using GetSinceBlock;
using WpfLiteCoinTester;
using WpfLiteCoinTester.BfEntities;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer
{
    public class BfPersistenceService : DefaultPersistenceService<bfEntities1>
    {
        public override async Task<bool> AddTransaction(string txid, int pendingOut, double actionLTCAmount, string actionAddress, int userId)
        {
            throw new NotImplementedException();
        }

        public override void CheckIncomingCoin(IList<Transaction> coinTransactions)
        {
            throw new NotImplementedException();
        }

        public override void Deposit(double amount, int userId, int type)
        {
            throw new NotImplementedException();
        }

        public override IList<IBalance> GetBalances(IUser selectedUser)
        {
            throw new NotImplementedException();
        }

        public override IUser GetDefaultUser()
        {
            throw new NotImplementedException();
        }

        public override string GetLastBlock()
        {
            var lastblock = GetLastBlockTransaction();

            if (lastblock != null)
            {
                return lastblock.hash;
            }

            return string.Empty;
        }

        public override IList<IUser> GetUsers()
        {
            throw new NotImplementedException();
        }

        public override void OffsetIncomingLTC(int userId)
        {
            throw new NotImplementedException();
        }

        public override void ReceivedSendCoinConfirmation(string oldTmpTxId, string txId)
        {
            throw new NotImplementedException();
        }

        public override void UpdateHeartBeatTransaction(RootObject listSinceBlockResult)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> UpdateUserLTCCount(int id, double actionLTCAmount)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ValidateRequest(int id, double actionLTCAmount, string actionAddress)
        {
            throw new NotImplementedException();
        }

        private BFBitcoinTransaction GetLastBlockTransaction()
        {
            string lastBlock = string.Empty;

            var bf = GetDefaultUser();

            if (bf != null)
            {
                using (var ctx = GetEntityModel())
                {
                    var lastblock = ctx.BFBitcoinTransactions.FirstOrDefault(t => string.Compare(t.status, "nothing_to_do", StringComparison.InvariantCultureIgnoreCase) == 0 &&
                    string.Compare(t.action, "lastblock_lte1", StringComparison.InvariantCultureIgnoreCase) == 0);

                    return lastblock;
                }
            }

            return null;
        }
        
    }
}
