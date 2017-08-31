using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;

public class CommandManager
{
    public static AutoResetEvent AutoEvent = new AutoResetEvent(false);
    ConnectionFactory factory;
    MemoryCache myCache = new MemoryCache(new MemoryCacheOptions());

    readonly string walletpassphrase;
    readonly string walletopentimeout;
    readonly string walletaccount;
    readonly string rmqhostname;
    readonly string rmqusername;
    readonly string rmqpassword;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(CommandManager));

    public CommandManager()
    {
        log.Info("Started command manager");

        var builder = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json")
           .AddInMemoryCollection(new[]
           {
                new KeyValuePair<string, string>("the-key", "the-value"),
           });

        var configuration = builder.Build();

        var tmpwalletpassphrase = configuration["walletpassphrase"];
        this.walletopentimeout = configuration["walletopentimeout"];
        this.walletaccount = configuration["walletaccount"];
        this.rmqhostname = configuration["rmqhostname"];
        this.rmqusername = configuration["rmqusername"];
        this.rmqpassword = configuration["rmqpassword"];

        this.walletpassphrase = Encryption.AESThenHMAC.SimpleDecryptWithPassword(tmpwalletpassphrase, "passwordMustBe12");

        this.factory = new ConnectionFactory() { HostName = rmqhostname, UserName = rmqusername, Password = rmqpassword };
    }

    public void ReceiveCommands()
    {
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "command", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = JsonConversionExtensions.Deserialize(ea.Body);

                    foreach (var key in body.Keys)
                    {
                        switch (key)
                        {
                            case "keypool":
                                {
                                    var result = (IList)body[key];
                                    SetKeyPool(int.Parse(result[0].ToString()));

                                    break;
                                }
                            case "getinfo":
                                {
                                    GetInfo();

                                    break;
                                }
                            case "generateAddress":
                                {
                                    var result = (IList)body[key];
                                    GenerateAddress(result[0] as string, int.Parse(result[1].ToString()));

                                    break;
                                }

                            case "sendcoin":
                                {
                                    var result = (IList)body[key];
                                    SendCoin(result[0] as string, result[1] as string, double.Parse(result[2].ToString()));

                                    break;
                                }
                            case "heartbeat":
                                {
                                    GetInfo();

                                    var result = (IList)body[key];
                                    GetListSinceBlock(result[0] as string);

                                    break;
                                }
                            case "listsinceblock":
                                {
                                    var result = (IList)body[key];
                                    GetListSinceBlock(result[0] as string);

                                    break;
                                }

                            default:
                                break;
                        }

                    }
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Error performing command:\n{0}\n{1}", exc.Message, exc.StackTrace);
                }
            };

            channel.BasicConsume(queue: "command", noAck: true, consumer: consumer);
            AutoEvent.WaitOne();
        }
    }

    private void SetKeyPool(int newSize)
    {
        var unlockResult = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("walletpassphrase {0} {1}", walletpassphrase, walletopentimeout));

        var txid = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("keypoolrefill {0}", newSize));

        var lockResult = LocalProcessCaller.GetResultFromCall("litecoin-cli", "walletlock");
    }

    private void GetInfo()
    {
        log.InfoFormat("getting info");

        var result = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("getinfo"));

        var pusher = new Pusher();

        pusher.Push(factory, "info", result);
    }

    private void GetListSinceBlock(string blockhash)
    {
        log.InfoFormat("getting latest since block {0}", blockhash);

        var result = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("listsinceblock {0}", blockhash));

        var pusher = new Pusher();

        pusher.Push(factory, "listsinceblock", result);
    }

    private void SendCoin(string address, string guid, double amount)
    {
        log.InfoFormat("sending coin");

        var recentRequests = myCache.Get<List<string>>("recent");

        if (recentRequests != null)
        {
            if (recentRequests.Contains(guid))
            {
                log.WarnFormat("Duplicate Send coin request, ignoring it");

                return;
            }

            recentRequests.Add(guid);

            log.InfoFormat("Adding Guid {0} to pre-existing memcache list", guid);

            this.myCache.Set<List<string>>("recent", recentRequests, TimeSpan.FromSeconds(10));
        }
        else
        {
            log.InfoFormat("memcache list of guids is empty");

            log.InfoFormat("creating new list and adding Guid {0} to memcache", guid);

            this.myCache.Set<List<string>>("recent", new List<string>(new[] { guid }), TimeSpan.FromSeconds(10));
        }

        log.InfoFormat("sending {0} coin to address {1}", amount, address);

        var unlockResult = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("walletpassphrase {0} {1}", walletpassphrase, walletopentimeout));

        var txid = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("sendfrom {0} {1} {2}", walletaccount, address, amount));

        var lockResult = LocalProcessCaller.GetResultFromCall("litecoin-cli", "walletlock");

        var pusher = new Pusher();

        pusher.Push(factory, "sendconfirmation", guid + "," + txid);

        log.Info("Sent coin successfully");
    }

    private void GenerateAddress(string accountName, int noAddresses)
    {
        Console.WriteLine(string.Format("generating {0} addresses for account {1}", noAddresses, accountName));

        List<string> addresses = new List<string>();

        for (int i = 0; i < noAddresses; i++)
        {
            var output = LocalProcessCaller.GetResultFromCall("litecoin-cli", string.Format("getnewaddress {0}", accountName));
            log.InfoFormat("new address '{0}'", output);
            addresses.Add(output);
        }

        var pusher = new Pusher();
        pusher.Push(factory, "address", string.Join(",", addresses));
    }
}
