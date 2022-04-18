using System.Threading.Tasks;
using System;
using System.IO;

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

using CommunityToolkit.Mvvm.DependencyInjection;

using PDFViewer_WinUI3Demo.ViewModels;


namespace PDFViewer_WinUI3Demo.Views
{
    public sealed partial class MainPage : Microsoft.UI.Xaml.Controls.Page
    {
        public static String InputPath { get { return System.IO.Path.Combine(Package.Current.InstalledLocation.Path, "Resources"); } }

        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            ViewModel = Ioc.Default.GetService<MainViewModel>();
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            String input_file_path = Path.Combine(InputPath, "GettingStarted.pdf");
            ViewModel.AddTab(input_file_path);
        }

        private async void pdfTabView_TabItemsChanged(TabView sender, Windows.Foundation.Collections.IVectorChangedEventArgs args)
        {
            // A minor delay to give it time for the TabView to properly finish it's "adding tab logic"
            await Task.Delay(50);

            var changeType = args.CollectionChange;
            if (changeType == Windows.Foundation.Collections.CollectionChange.ItemInserted)
            {
                sender.SelectedIndex = (int)args.Index;
            }
        }

        private void CommandBar_Opening(object sender, object e)
        {
            btnSave.IsCompact = false;
            btnSaveAs.IsCompact = false;
        }

        private void CommandBar_Closed(object sender, object e)
        {
            btnSave.IsCompact = true;
            btnSaveAs.IsCompact = true;
        }

        private void pdfTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (ViewModel == null)
                return;

            var pdfTabInfo = args.Tab.DataContext as PdfTabInfo;
            if (pdfTabInfo == null)
                return;

            ViewModel.CloseTab(pdfTabInfo);            
        }
    }
}
