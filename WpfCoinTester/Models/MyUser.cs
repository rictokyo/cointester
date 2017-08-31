namespace WpfLiteCoinTester
{
    public class MyUser : IUser
    {
        private string imgUrl;

        public string Name { get; set; }
        public string AvaratarUrl
        {
            get { return "Resources/" + this.imgUrl; }
            set { this.imgUrl = value; }
        }
        public string Address { get; set; }
        public int State { get; set; }
        public int Id { get; set; }
    }
}