using System;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zoomies
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private Mutex? _singleInstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Check for single instance
            const string mutexName = "Global\\Zoomies_SingleInstance_Mutex";
            _singleInstanceMutex = new Mutex(true, mutexName, out bool isNewInstance);
            
            if (!isNewInstance)
            {
                MessageBox.Show("Zoomies is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown(0);
                return;
            }
            
            base.OnStartup(e);

            // Set up global exception handling
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                // Configure services
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider();

                // Create and show main window manually
                var logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
                var mainWindow = new MainWindow(logger, _serviceProvider);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                UI.ErrorDialog.Show("Startup Error", "Failed to start application", ex);
                Shutdown(1);
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            UI.ErrorDialog.Show("Application Error", e.Exception);
            e.Handled = true;
            Shutdown(1);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                UI.ErrorDialog.Show("Fatal Error", ex);
            }
            else
            {
                MessageBox.Show($"Fatal error: {e.ExceptionObject}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Shutdown(1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure logging
            services.AddLogging(configure =>
            {
                configure.AddDebug();
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Debug);
            });

            // Register application services
            services.AddSingleton<Core.ScreenCapture.IScreenCapture, Core.ScreenCapture.GdiScreenCapture>();
            services.AddSingleton<Core.Input.GlobalHookManager>();
            services.AddSingleton(provider => Dispatcher);

            // Register windows
            services.AddSingleton<UI.ScreenshotOverlay>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}