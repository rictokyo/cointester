using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfLiteCoinTester;
using WpfLiteCoinTester.BfEntities;

namespace DataLayer
{
    public sealed class SimplePersistenceService : DefaultPersistenceService<BfLTEEntities1>
    {

        public override async Task<bool> ValidateRequest(int id, double actionLTCAmount, string actionAddress)
        {
            return await Task.Run(() =>
            {
                var secondsAgo = DateTime.Now.Subtract(TimeSpan.FromSeconds(5));

                using (var ctx = GetEntityModel())
                {
                    var recentTransactions = ctx.transactions.Where(t => t.lastUpdate > secondsAgo);

                    return !recentTransactions.Any(r => r.userId == id && r.amount == actionLTCAmount && string.Compare(r.address, actionAddress, StringComparison.InvariantCultureIgnoreCase) == 0);
                }
            });
        }

        public override async Task<bool> AddTransaction(string txid, int pendingOut, double actionLTCAmount, string actionAddress, int userId)
        {
            return await Task.Run(() =>
            {
                using (var ctx = GetEntityModel())
                {
                    var user = ctx.users.FirstOrDefault(u => u.uid == userId);
                    if (user == null) return false;

                    var newTransaction = new transaction
                    {
                        address = actionAddress,
                        user = user,
                        txid = txid,
                        amount = actionLTCAmount,
                        state = (int)TransactionStatus.PendingOut
                    };

                    ctx.transactions.Add(newTransaction);
                    SaveChanges(ctx);
                }

                return true;
            });
        }

        public override void OffsetIncomingLTC(int userId)
        {
            using (var ctx = GetEntityModel())
            {
                var balToUpdate = ctx.balances.Where(b => b.userId == userId && b.type == 3);

                foreach (var bal in balToUpdate)
                    bal.type = 1;

                SaveChanges(ctx);
            }
        }

        public override IList<IUser> GetUsers()
        {
            IList<IUser> users;

            using (var ctx = GetEntityModel())
            {
                users = ctx.users.Where(u => u.status == 0).Select(usr => new MyUser
                {
                    Id = usr.uid,
                    AvaratarUrl = usr.avatarUrl,
                    Name = usr.username,
                    Address = usr.address.address1
                }).OrderBy(u => u.Name).ToList<IUser>();
            }

            return users;
        }

        public override IUser GetDefaultUser()
        {
            IUser user;
            using (var ctx = GetEntityModel())
            {
                user = ctx.users.Where(u => u.status == -3).Select(usr => new MyUser
                {
                    Id = usr.uid,
                    AvaratarUrl = usr.avatarUrl,
                    Name = usr.username,
                    Address = usr.address.address1
                }).OrderBy(u => u.Name).FirstOrDefault<IUser>();
            }

            return user;
        }

        public override IList<IBalance> GetBalances(IUser selectedUser)
        {
            List<balance> balances1 = null;

            using (var ctx = GetEntityModel())
            {
                if (ctx.balances != null && selectedUser != null)
                {
                    balances1 = ctx.balances.Where(b => b != null && b.userId == selectedUser.Id).OrderBy(o => o.type).ToList();
                }
            }

            if (balances1 == null) return null;

            var balances = balances1.GroupBy(x => x.type).Select(bal => new MyBalance
            {
                Amount = bal.Sum(b => b.balance1).Value,
                Type = BalanceUtils.GetBalanceType(bal.Key)
            }).ToList<IBalance>();

            return balances;
        }

        public override void Deposit(double amount, int userId, int type)
        {
            using (var ctx = GetEntityModel())
            {
                var balance = new balance
                {
                    balance1 = amount,
                    type = type,
                    userId = userId
                };

                ctx.balances.Add(balance);

                SaveChanges(ctx);
            }
        }

        public override async Task<bool> UpdateUserLTCCount(int id, double actionLTCAmount)
        {
            return await Task.Run(() =>
            {
                using (var ctx = GetEntityModel())
                {
                    var user = ctx.users.FirstOrDefault(u => u.uid == id);
                    if (user == null) return false;

                    var newBalance = new balance
                    {
                        balance1 = actionLTCAmount,
                        user = user,
                        type = (int)BalanceTypes.LTC
                    };

                    ctx.balances.Add(newBalance);
                    SaveChanges(ctx);
                    return true; ;
                }
            });
        }

        public override string GetLastBlock()
        {
            var lastblock = GetLastBlockTransaction();
            if (lastblock != null)
            { return lastblock.txid; }

            return string.Empty;
        }

        private transaction GetLastBlockTransaction()
        {
            string lastBlock = string.Empty;

            var bf = GetDefaultUser();

            if (bf != null)
            {
                using (var ctx = GetEntityModel())
                {
                    var lastblock = ctx.transactions.FirstOrDefault(t => t.state == (int)TransactionStatus.LatestBlock && t.userId == bf.Id);
                    return lastblock;

                }
            }

            return null;
        }

        public override void CheckIncomingCoin(IList<GetSinceBlock.Transaction> coinTransactions)
        {
            if (coinTransactions == null) return;

            using (var ctx = GetEntityModel())
            {
                var gotUsers = ctx.users.Select(u => new MyUser { Id = u.uid, Address = u.address.address1 }).ToList();

                var txInfos = (from u in gotUsers.Where(u => !string.IsNullOrEmpty(u.Address))
                               join t in coinTransactions on u.Address.TrimEnd('\r', '\n') equals t.address
                               where string.Compare(t.category, "receive", StringComparison.InvariantCultureIgnoreCase) == 0
                               select new transaction { amount = t.amount, txid = t.txid, address = t.address, userId = u.Id, state = (int)TransactionStatus.PendingIn, lastUpdate = DateTime.Now }).ToList();

                foreach (var txInfo in txInfos)
                {
                    if (!ctx.transactions.Any(t => string.Compare(t.txid, txInfo.txid, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        ctx.transactions.Add(txInfo);

                        var bal = new balance { balance1 = txInfo.amount, type = (int)BalanceTypes.INLTC, userId = txInfo.userId };
                        ctx.balances.Add(bal);
                    }
                }

                SaveChanges(ctx);
            }
        }

        public override void UpdateHeartBeatTransaction(GetSinceBlock.RootObject listSinceBlockResult)
        {
            var bf = GetDefaultUser();
            if (bf == null) return;

            var lastblock = GetLastBlockTransaction();

            using (var ctx = GetEntityModel())
            {
                if (lastblock == null)
                {
                    lastblock = new transaction
                    {
                        txid = listSinceBlockResult.lastblock,
                        amount = 0,
                        address = string.Empty,
                        userId = bf.Id,
                        state = (int)TransactionStatus.LatestBlock,
                    };

                    ctx.transactions.Add(lastblock);
                }
                else
                {
                    if (listSinceBlockResult != null)
                    {
                        lastblock.txid = listSinceBlockResult.lastblock;
                        lastblock.lastUpdate = DateTime.Now;
                    }
                }

                SaveChanges(ctx);
            }

        }

        public override void ReceivedSendCoinConfirmation(string oldTmpTxId, string txId)
        {
            using (var ctx = GetEntityModel())
            {
                var transaction = ctx.transactions.FirstOrDefault(t => string.Compare(t.txid, oldTmpTxId, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (transaction == null) return;

                transaction.txid = txId;
                transaction.state = (int)TransactionStatus.Out;
                transaction.lastUpdate = DateTime.Now;

                SaveChanges(ctx);
            }
        }
    }
}
