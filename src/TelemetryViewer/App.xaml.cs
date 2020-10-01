using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using UGCS.SsdpDiscoveryService;
using UGCS.TelemetryViewer.Services;
using UGCS.TelemetryViewer.ViewModels;
using UGCS.TelemetryViewer.ViewModels.Auxiliary;
using UGCS.TelemetryViewer.ViewModels.Intrerfaces;
using UGCS.TelemetryViewer.Views;
using UGCS.UcsServices;

namespace UGCS.TelemetryViewer
{
    public class TelemetryViewerApp : Application
    {

        private const double DEFAULT_WIDTH = 252;
        private const double DEFAULT_HEIGHT = 700;

        public static Version Version
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version;
            }
        }

        private static readonly ILog _log = LogManager.GetLogger(typeof(TelemetryViewerApp));

        public IClassicDesktopStyleApplicationLifetime Desktop { get { return this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime; } }

        public static new TelemetryViewerApp Current
        {
            get
            {
                Debug.Assert(Application.Current is TelemetryViewerApp, "Current application type is unexpected");
                return (TelemetryViewerApp)Application.Current;
            }
        }

        public static ServiceProvider ServiceProvider { get; private set; }

        private IVisualComponentsFactory _viewFactory;
        public IVisualComponentsFactory ViewFactory => _viewFactory;

        public void SetMainWindow(Window window)
        {
            if (Desktop != null)
                Desktop.MainWindow = window;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (Desktop != null)
                Desktop.Exit += onExit;

            string dataDirectory;
            string logsDirectory;
            dataDirectory = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                   "UgCS",
                   "telemetry-viewer");

            ServiceProvider = initServices(dataDirectory);

            ServiceProvider.GetRequiredService<IDiscoveryService>().StartListen();

            _viewFactory = ServiceProvider.GetService<IVisualComponentsFactory>();
            try
            {
                if (!Directory.Exists(dataDirectory))
                    Directory.CreateDirectory(dataDirectory);

                logsDirectory = Path.Combine(dataDirectory, "logs");
                if (!Directory.Exists(logsDirectory))
                    Directory.CreateDirectory(logsDirectory);

                initLog(logsDirectory);
            }
            catch (Exception err)
            {
                // Log still is not initialized, app can display error message only.
                ErrorWindow errWindow = _viewFactory.CreateWindow<ErrorWindow>(true);
                errWindow.DataContext = err;
                errWindow.Show();
                TelemetryViewerApp.Current.SetMainWindow(errWindow);
                return;
            }

            _log.Info($"Telemetry viewer {Version} started.");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _viewFactory.CreateWindow<InitializingSplashWindow>();
                ((IInitializingSplashWindowViewModel)desktop.MainWindow.DataContext).StartInitializing();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void initLog(string logsDirectory)
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            // Update relative file pathes to absolute (relative to the data directory).
            foreach (XmlElement fileSection in log4netConfig.SelectNodes("LogConfig/defaultLog/log4net/appender/file"))
            {
                string path = fileSection.GetAttribute("value");
                if (Path.IsPathRooted(path))
                    continue;
                fileSection.SetAttribute(
                    "value",
                    Path.Combine(
                        logsDirectory,
                        path));
            }

            log4net.GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            var repoDefault = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                       typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(repoDefault, log4netConfig["LogConfig"]["defaultLog"]["log4net"]);
        }

        private static ServiceProvider initServices(string dataPath)
        {
            var services = new ServiceCollection();

            // Factories
            services.AddSingleton<IVisualComponentsFactory, VisualComponentsFactory>();
            services.AddSingleton<ITelemetryPlateFactory, TelemetryPlateFactory>();

            // View Models
            services.AddTransient<IInitializingSplashWindowViewModel, InitializingSplashWindowViewModel>();
            services.AddTransient<ILoginWindowViewModel, LoginWindowViewModel>();
            services.AddTransient<IHostWindowViewModel, HostWindowViewModel>();
            services.AddTransient<IMainWindowViewModel, MainWindowViewModel>();
            services.AddTransient<ICreatePlateWindowViewModel, CreatePlateWindowViewModel>();

            // Services
            services.AddSingleton<ConnectionService>();
            services.AddSingleton<UcsAutoReconnectService>();
            services.AddSingleton<VehicleListener>();
            services.AddSingleton<VehicleService>();
            services.AddSingleton<TelemetryListener>();
            services.AddSingleton<IDiscoveryService, DiscoveryService>();
            services.AddSingleton<ISelectedVehicleContainer, SelectedVehicleContainer>();
            services.AddSingleton<IStorageService, StorageService>()
                .AddOptions<StorageService.Options>()
                .Configure(o => o.Path = dataPath);
            services.AddSingleton<IAppContextContainer, AppContextContainer>()
                .AddOptions<AppContextContainer.Options>()
                .Configure(o => { o.DefaultHeight = DEFAULT_HEIGHT; o.DefaultWidth = DEFAULT_WIDTH; });

            return services.BuildServiceProvider();
        }

        private void onExit(object sender, ControlledApplicationLifetimeExitEventArgs args)
        {
            ServiceProvider.Dispose();
        }
    }
}
