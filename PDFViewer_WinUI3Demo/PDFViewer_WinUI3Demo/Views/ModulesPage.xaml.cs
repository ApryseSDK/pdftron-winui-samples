using Microsoft.UI.Xaml.Controls;

using CommunityToolkit.Mvvm.DependencyInjection;

using PDFViewer_WinUI3Demo.ViewModels;

namespace PDFViewer_WinUI3Demo.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModulesPage : Page
    {
        public ModulesViewModel ViewModel { get; }

        public ModulesPage()
        {
            ViewModel = Ioc.Default.GetService<ModulesViewModel>();
            this.InitializeComponent();
        }
    }
}
