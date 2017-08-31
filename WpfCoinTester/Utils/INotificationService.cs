namespace WpfLiteCoinTester
{
    public interface INotificationService
    {
        bool AskForConfirmation(string messageBox, string caption);
    }
}
