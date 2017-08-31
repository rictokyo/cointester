using Newtonsoft.Json;
using System.Collections.Generic;
namespace GetSinceBlock
{
    public class RootObject
    {
        public List<Transaction> transactions { get; set; }
        public string lastblock { get; set; }
    }

    public class Transaction
    {
        public string account { get; set; }
        public string address { get; set; }
        public string category { get; set; }
        public double amount { get; set; }
        public string label { get; set; }
        public int vout { get; set; }
        public double fee { get; set; }
        public int confirmations { get; set; }
        public string blockhash { get; set; }
        public int blockindex { get; set; }
        public int blocktime { get; set; }
        public string txid { get; set; }
        public List<object> walletconflicts { get; set; }
        public int time { get; set; }
        public int timereceived { get; set; }
        [JsonIgnore]
        [JsonProperty(PropertyName = "bip125-replaceable")]
        public bool abandoned { get; set; }
    }
}
