//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfLiteCoinTester.BfEntities
{
    using System;
    using System.Collections.Generic;
    
    public partial class BankOutgoing
    {
        public int uid { get; set; }
        public string bank_outgoing_id { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string account_id { get; set; }
        public string bank_code { get; set; }
        public string bank_name_kana { get; set; }
        public string brunch_code { get; set; }
        public string brunch_name_kana { get; set; }
        public string clearing_house_no { get; set; }
        public string account_type { get; set; }
        public string bank_account_number { get; set; }
        public string receipt_name_kana { get; set; }
        public decimal amount { get; set; }
        public string update_code { get; set; }
        public string client_code1 { get; set; }
        public string client_code2 { get; set; }
        public string shikibatsu_code { get; set; }
        public System.DateTime created_date { get; set; }
        public System.DateTime updated_date { get; set; }
        public string dummy { get; set; }
        public int is_pending { get; set; }
    }
}
