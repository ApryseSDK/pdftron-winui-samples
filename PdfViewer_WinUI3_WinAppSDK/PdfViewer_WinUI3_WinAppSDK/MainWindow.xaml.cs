using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Storage.Pickers;
using Windows.Storage;
// NOTE: the name space must be added in roder to use WinRT classes
using WinRT;

using pdftron;
using pdftron.PDF;


namespace PdfViewer_WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private bool mActivedHandled = false;
        PDFViewCtrl mPdfView;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (mActivedHandled)
                return;

            mActivedHandled = true;

            var file = await GetFileFromInstalledLocation(@"Resources\GettingStarted.pdf");
            if (file == null)
                return;

            var doc = PDFDoc.CreateFromStorageFile(file);

            OpenFileOnViewer(doc);
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await OpenFileAsync();
            if (file == null)
                return;

            var doc = PDFDoc.CreateFromStorageFile(file);

            OpenFileOnViewer(doc);
        }

        private void OpenFileOnViewer(PDFDoc doc)
        {
            if (mPdfView == null)
            {
                mPdfView = new PDFViewCtrl();
                pdfViewBorder.Child = mPdfView;

                var toolManager = new pdftron.PDF.Tools.ToolManager(mPdfView);
            }
            else
            {
                mPdfView.CloseDoc();
            }

            mPdfView.SetDoc(doc);
        }

        #region Utilities

        /// <summary>
        /// Open dialog box to search and load PDF file
        /// 
        /// </summary>
        public static async Task<StorageFile> OpenFileAsync()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.FileTypeFilter.Add(".pdf");

            // When running on win32, FileOpenPicker needs to know the top-level hwnd via IInitializeWithWindow::Initialize.
            if (Window.Current == null)
            {
                IInitializeWithWindow initializeWithWindowWrapper = filePicker.As<IInitializeWithWindow>();
                IntPtr hwnd = GetActiveWindow();
                initializeWithWindowWrapper.Initialize(hwnd);
            }

            StorageFile file = await filePicker.PickSingleFileAsync();
            return file;
        }

        /// <summary>
        /// Get a file from the application installed location
        /// </summary>
        /// <returns>The StorageFile, null if not found</returns>
        private async Task<StorageFile> GetFileFromInstalledLocation(string path)
        {
            StorageFile file = null;
            var installedPath = Package.Current.InstalledLocation.Path;

            try
            {
                var installedFolder = await StorageFolder.GetFolderFromPathAsync(installedPath);
                file = await installedFolder.GetFileAsync(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return file;
        }

        /// <summary>
        /// "In Desktop, or Win32, it's required to specify which Window Handle (HWND) owns the File/Folder Picker"
        /// Github issue: https://github.com/microsoft/microsoft-ui-xaml/issues/4100
        /// </summary>
        [ComImport, System.Runtime.InteropServices.Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IInitializeWithWindow
        {
            void Initialize([In] IntPtr hwnd);
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
        public static extern IntPtr GetActiveWindow();

        #endregion
    }
}
