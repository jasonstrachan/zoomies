using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Zoomies.UI;

namespace Zoomies
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private ScreenshotOverlay? _screenshotOverlay;

        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();

            // Hide the window initially
            Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Initialize screenshot overlay
            _screenshotOverlay = _serviceProvider.GetRequiredService<ScreenshotOverlay>();

            // Set up system tray icon
            SetupTrayIcon();

        }

        private void SetupTrayIcon()
        {
            // Check if app starts with Windows
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            StartWithWindowsMenuItem.IsChecked = key?.GetValue("Zoomies") != null;
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(
                "💀 Zoomies - Screen Magnifier 💀\n\n" +
                "A lightweight magnifier that won't kill your performance!\n\n" +
                "Features:\n" +
                "• Zoom: CTRL + ALT + Scroll\n" +
                "• Pan: Move mouse to screen edges when zoomed\n" +
                "• Auto-start with Windows option\n\n" +
                $"Version {version}",
                "About Zoomies",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnStartWithWindowsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (StartWithWindowsMenuItem.IsChecked)
                {
                    key?.SetValue("Zoomies", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
                }
                else
                {
                    key?.DeleteValue("Zoomies", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update startup settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            _screenshotOverlay?.Close();
            TrayIcon?.Dispose();
            base.OnClosed(e);
        }
    }
}