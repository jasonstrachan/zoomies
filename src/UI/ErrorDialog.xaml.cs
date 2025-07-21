using System;
using System.Windows;

namespace Zoomies.UI
{
    public partial class ErrorDialog : Window
    {
        public ErrorDialog(string title, string errorMessage)
        {
            InitializeComponent();

            Title = $"Zoomies Error - {title}";
            ErrorTextBox.Text = errorMessage;

            // Select all text for easy copying
            ErrorTextBox.SelectAll();
            ErrorTextBox.Focus();
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ErrorTextBox.Text);
                MessageBox.Show("Error details copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Copy Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public static void Show(string title, Exception exception)
        {
            var errorMessage = FormatException(exception);
            var dialog = new ErrorDialog(title, errorMessage);
            dialog.ShowDialog();
        }

        public static void Show(string title, string message, Exception exception)
        {
            var errorMessage = $"{message}\n\n{FormatException(exception)}";
            var dialog = new ErrorDialog(title, errorMessage);
            dialog.ShowDialog();
        }

        private static string FormatException(Exception exception)
        {
            var message = $"Error Type: {exception.GetType().FullName}\n";
            message += $"Message: {exception.Message}\n\n";

            if (exception.InnerException != null)
            {
                message += "Inner Exception:\n";
                message += FormatException(exception.InnerException);
                message += "\n";
            }

            message += $"Stack Trace:\n{exception.StackTrace}";

            return message;
        }
    }
}