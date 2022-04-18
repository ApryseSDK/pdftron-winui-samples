using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using PDFViewer_WinUI3Demo.Activation;
using PDFViewer_WinUI3Demo.Contracts.Services;
using PDFViewer_WinUI3Demo.Helpers;
using PDFViewer_WinUI3Demo.Services;
using PDFViewer_WinUI3Demo.ViewModels;
using PDFViewer_WinUI3Demo.Views;

// To learn more about WinUI3, see: https://docs.microsoft.com/windows/apps/winui/winui3/.
namespace PDFViewer_WinUI3Demo
{
    public partial class App : Application
    {
        public static Window MainWindow { get; set; } = new Window() { Title = "AppDispName".GetLocalized() };

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            Ioc.Default.ConfigureServices(ConfigureServices());

            pdftron.PDFNet.Initialize();

#if IS_64_BIT
            uint cacheSize = 800;
#else
            uint cacheSize = 400;
#endif
            pdftron.PDFNet.SetViewerCache(cacheSize * 1024 * 1024, true);
            pdftron.PDFNet.SetDefaultDiskCachingEnabled(true);

        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/windows/winui/api/microsoft.ui.xaml.unhandledexceptioneventargs
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            var activationService = Ioc.Default.GetService<IActivationService>();
            await activationService.ActivateAsync(args);
        }

        private System.IServiceProvider ConfigureServices()
        {
            // TODO WTS: Register your services, viewmodels and pages here
            var services = new ServiceCollection();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services

            // Views and ViewModels
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ModulesViewModel>();
            services.AddTransient<ModulesPage>();
            return services.BuildServiceProvider();
        }
    }
}
