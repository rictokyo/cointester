namespace WpfLiteCoinTester
{
    public interface IUser
    {
        string Name { get; set; }
        string AvaratarUrl { get; set; }
        int State { get; set; }
        int Id { get; set; }
        string Address { get; set; }
    }
}