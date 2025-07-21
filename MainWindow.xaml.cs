using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Zoomies.UI;

namespace Zoomies
{
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ScreenshotOverlay? _screenshotOverlay;

        public MainWindow(ILogger<MainWindow> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            InitializeComponent();

            // Hide the window initially
            Hide();

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            _logger.LogInformation($"=== Zoomies v{version} ===");
            _logger.LogInformation("Zoomies main window initialized");
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
            MessageBox.Show(
                "💀 Zoomies - Screen Magnifier 💀\n\n" +
                "A lightweight magnifier that won't kill your performance!\n\n" +
                "Keyboard shortcuts:\n" +
                "CTRL + ALT + Scroll - Zoom in/out\n\n" +
                "Version 1.0.0",
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
                _logger.LogError(ex, "Failed to update startup registry");
                MessageBox.Show("Failed to update startup settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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