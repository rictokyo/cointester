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
    
    public partial class BankIncoming
    {
        public int uid { get; set; }
        public string bank_incoming_id { get; set; }
        public string status { get; set; }
        public int in_out_count { get; set; }
        public string filename { get; set; }
        public string sakuseibi { get; set; }
        public string kanjobi_from { get; set; }
        public string kanjobi_to { get; set; }
        public string bank_code { get; set; }
        public string bank_name_kana { get; set; }
        public string brunch_code { get; set; }
        public string brunch_name_kana { get; set; }
        public string account_type { get; set; }
        public string bank_account_number { get; set; }
        public string account_name { get; set; }
        public string kashikoshi_kubun { get; set; }
        public string tucho_kubun { get; set; }
        public decimal previous_amount { get; set; }
        public string shokai_bango { get; set; }
        public string kanjobi { get; set; }
        public string haraidashibi { get; set; }
        public string iriharai_kubun { get; set; }
        public string torihiki_kubun { get; set; }
        public decimal amount { get; set; }
        public decimal amount_by_other_bank { get; set; }
        public string kokan_teijibi { get; set; }
        public string fuwatari_henkanbi { get; set; }
        public string tegata_kubun { get; set; }
        public string tegata_number { get; set; }
        public string ryoten_number { get; set; }
        public string furikomi_irainin_code { get; set; }
        public string furimomi_irainin_name { get; set; }
        public string shimuke_bank_name { get; set; }
        public string shimuke_brunch_name { get; set; }
        public string tekiyo { get; set; }
        public string edi_info { get; set; }
        public System.DateTime created_date { get; set; }
        public System.DateTime updated_date { get; set; }
        public decimal following_amount { get; set; }
        public string account_id { get; set; }
        public string memo { get; set; }
        public int is_pending { get; set; }
        public string type { get; set; }
        public string pending_reasons { get; set; }
    }
}
