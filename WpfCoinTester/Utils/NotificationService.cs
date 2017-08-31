using System.Threading;
using System.Windows;

namespace WpfLiteCoinTester
{
    public class NotificationService : INotificationService
    {
        public bool AskForConfirmation(string messageBox, string caption)
        {
            var resEvent = new AutoResetEvent(false);
            bool result = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                string sMessageBoxText = "Do you want to continue?";
                string sCaption = "Duplicate send detected";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
                resEvent.Set();

                result = rsltMessageBox == MessageBoxResult.Yes;
            });

            resEvent.WaitOne(5000);

            return result;
        }

    }
}
