﻿using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

using Windows.Storage;
using Windows.UI;
using Windows.ApplicationModel;

// NOTE: the name space must be added in roder to use WinRT classes
using WinRT;
using static pdftron.PDF.Tools.UtilityWinRT;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using pdftron.PDF.Tools;
using pdftron.PDF;


namespace PDFViewer_WinUI3Demo.ViewModels
{
    public class PdfTabInfo : ObservableRecipient, IDisposable
    {
        PDFViewCtrl mPDFViewCtrl;
        ToolManager mToolManager;
        bool mIsVerticalScrolling = true;

        public PdfTabInfo(StorageFile file, PDFDoc doc)
        {
            mPDFViewCtrl = new PDFViewCtrl();
            mToolManager = new ToolManager(PDFView);

            if (file != null)
            {
                OriginalFile = file;
                Name = file.DisplayName;
            }

            PDFView.SetBackgroundColor(Windows.UI.Color.FromArgb(255, 241, 243, 245));
            PDFView.SetDoc(doc);
            PDFView.SetPagePresentationMode(PDFViewCtrlPagePresentationMode.e_single_continuous);

            // Set Undo and Redo Manager
            UndoRedoManager undoRedoManager = new UndoRedoManager();
            mToolManager.SetUndoRedoManager(undoRedoManager);
            mToolManager.IsSignatureDialogFullScreen = false;
        }

        #region Public Properties

        public PDFViewCtrl PDFView
        {
            get { return mPDFViewCtrl; }
            set { SetProperty(ref mPDFViewCtrl, value); }
        }

        public ToolManager ToolMagr
        {
            get { return mToolManager; }
            set { mToolManager = value; }
        }

        public StorageFile OriginalFile { get; set; }

        public string Name { get; set;} = "untitled";

        public bool IsVerticalScrolling
        {
            get { return mIsVerticalScrolling; }
            set
            { 
                mIsVerticalScrolling = value;
                // NOTE: just update back to single
                SetPresentationMode("single");
            }
        }

        #endregion

        #region Public Methods

        public void SetPresentationMode(string strVal)
        {
            var presentationMode = PDFView.GetPagePresentationMode();

            switch (strVal)
            {
                case "single":
                    if (IsVerticalScrolling)
                        presentationMode = PDFViewCtrlPagePresentationMode.e_single_continuous;
                    else
                        presentationMode = PDFViewCtrlPagePresentationMode.e_single_page;
                    break;

                case "facing":
                    if (IsVerticalScrolling)
                        presentationMode = PDFViewCtrlPagePresentationMode.e_facing_continuous;
                    else
                        presentationMode = PDFViewCtrlPagePresentationMode.e_facing;
                    break;

                case "cover":
                    if (IsVerticalScrolling)
                        presentationMode = PDFViewCtrlPagePresentationMode.e_facing_continuous_cover;
                    else
                        presentationMode = PDFViewCtrlPagePresentationMode.e_facing_cover;
                    break;

                default:
                    return;
            }

            if (PDFView.GetPagePresentationMode() != presentationMode)
            {
                PDFView.SetPagePresentationMode(presentationMode);
                PDFView.UpdatePageLayout();
            }
        }

        public async Task SaveDocAsync()
        {
            if (!PDFView.HasDocument)
                return;

            var doc = PDFView.GetDoc();

            bool isLocked = false;
            try
            {
                doc.LockRead();
                isLocked = true;

                if (doc.IsModified())
                {
                    doc.UnlockRead();
                    isLocked = false;

                    await doc.SaveAsync(pdftron.SDF.SDFDocSaveOptions.e_incremental);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (isLocked)
                    doc.UnlockRead();
            }

        }

        public async Task SaveDocAsAsync()
        {
            if (!PDFView.HasDocument)
                return;

            var doc = PDFView.GetDoc();
            await UtilityWinRT.SaveDocAsAsync(doc);
        }

        public bool CloseContextMenu()
        {
            if (ToolMagr != null)
                return ToolMagr.ClosePopup();

            return false;
        }

        public void Dispose()
        {
            if (ToolMagr != null)
            {
                ToolMagr.Dispose();
                ToolMagr = null;
            }

            if (PDFView != null)
            {
                PDFView.Deactivate();
                PDFView = null;
            }
        }
        #endregion
    }

    public class MainViewModel : ObservableRecipient
    {
        private ObservableCollection<PdfTabInfo> mPdfTabs;
        private PdfTabInfo mPdfTab;

        public MainViewModel()
        {
            // Initialize commands
            CMDAddTabAsync = new AsyncRelayCommand(AddTabAsync);
            CMDSaveAsync = new AsyncRelayCommand(SaveAsync);
            CMDSaveAsAsync = new AsyncRelayCommand(SaveAsAsync);
            CMDPrint = new RelayCommand(Print);
            CMDRotateClockwise = new RelayCommand(RotateClockwise);
            CMDViewModeChange = new RelayCommand<string>(ViewModeChange);
            CMDToggleVerticalScrolling = new RelayCommand(TogleVerticalScrolling);

            mPdfTabs = new ObservableCollection<PdfTabInfo>();

            this.PropertyChanged += MainViewModel_PropertyChanged;
        }

        #region Public Properties
        public ObservableCollection<PdfTabInfo> PdfTabs
        {
            get { return mPdfTabs; }
            set { SetProperty(ref mPdfTabs, value); }
        }

        public PdfTabInfo SelectedPdfTab
        {
            get { return mPdfTab; }
            set { SetProperty(ref mPdfTab, value); }
        }

        public bool IsVerticalScrolling
        {
            get
            {
                if (SelectedPdfTab != null)
                    return SelectedPdfTab.IsVerticalScrolling;

                return false;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Close document tab
        /// </summary>
        public void CloseTab(PdfTabInfo tab)
        {
            if (!PdfTabs.Contains(tab))
                return;

            tab.Dispose();
            PdfTabs.Remove(tab);
        }

        /// <summary>
        /// Open getting started PDF sample file from installed app's folder
        /// </summary>
        public async Task OpenGettingStarted()
        {
            var file = await GetFileFromInstalledLocation(@"Resources\GettingStarted.pdf");
            if (file == null)
                return;

            AddTabAsync(file);
        }
        #endregion

        #region Public Commands

        public IAsyncRelayCommand CMDAddTabAsync { get; set; }

        public IAsyncRelayCommand CMDOpenFileAsync { get; set; }

        public IAsyncRelayCommand CMDSaveAsync { get; set; }

        public IAsyncRelayCommand CMDSaveAsAsync { get; set; }

        public RelayCommand CMDPrint { get; set; }

        public RelayCommand CMDRotateClockwise { get; set; }

        public RelayCommand<string> CMDViewModeChange { get; set; }

        public RelayCommand CMDToggleVerticalScrolling { get; set; }

        #endregion

        #region Operations
 
        private void Print()
        {
            if (SelectedPdfTab == null || SelectedPdfTab.PDFView == null) 
                return;

            PrinterSettings settings = new PrinterSettings();
            if (!settings.IsDefaultPrinter)
                return;

            var doc = SelectedPdfTab.PDFView.GetDoc();

            // Setup printing options:
            PrinterMode printerMode = new PrinterMode();
            printerMode.SetAutoCenter(true);
            printerMode.SetAutoRotate(true);
            printerMode.SetCollation(true);
            printerMode.SetCopyCount(1);
            printerMode.SetDPI(300); // regardless of ordering, an explicit DPI setting overrides the OutputQuality setting
            printerMode.SetDuplexing(PrinterModeDuplexMode.e_Duplex_Auto);
            printerMode.SetNUp(PrinterModeNUp.e_NUp_1_1, PrinterModeNUpPageOrder.e_PageOrder_LeftToRightThenTopToBottom);
            printerMode.SetOrientation(PrinterModeOrientation.e_Orientation_Portrait);
            printerMode.SetOutputAnnot(PrinterModePrintContentTypes.e_PrintContent_DocumentAndAnnotations);

            // If the XPS print path is being used, then the printer spooler file will
            // ignore the grayscale option and be in full color
            printerMode.SetOutputColor(PrinterModeOutputColor.e_OutputColor_Grayscale);
            printerMode.SetOutputPageBorder(false);
            printerMode.SetOutputQuality(PrinterModeOutputQuality.e_OutputQuality_Medium);
            printerMode.SetPaperSize(new Rect(0, 0, 612, 792));
            PageSet pagesToPrint = new PageSet(1, doc.GetPageCount(), PageSetFilter.e_all);

            try
            {
                // Print the document on the default printer, name the print job the name of the 
                // file, print to the printer not a file, and use printer options:
                pdftron.PDF.Print.StartPrintJob(doc, "", doc.GetFileName(), "", pagesToPrint, printerMode, null);
            }
            catch (Exception)
            {
                // User cancelation
            }
        }

        private void RotateClockwise()
        {
            if (SelectedPdfTab == null || SelectedPdfTab.PDFView == null)
                return;

            SelectedPdfTab.PDFView.RotateClockwise();
        }

        private void ViewModeChange(string val)
        {
            if (SelectedPdfTab == null)
                return;

            if (string.IsNullOrEmpty(val))
                return;

            SelectedPdfTab.SetPresentationMode(val);
        }

        private void TogleVerticalScrolling()
        {
            if (SelectedPdfTab == null)
                return;

            SelectedPdfTab.IsVerticalScrolling = !SelectedPdfTab.IsVerticalScrolling;
            Broadcast<bool>(IsVerticalScrolling, IsVerticalScrolling, nameof(IsVerticalScrolling));
        }

        private async Task AddTabAsync()
        {
            var file = await UtilityWinRT.OpenFileAsync();
            if (file == null)
                return;

            AddTabAsync(file);
        }

        private void AddTabAsync(StorageFile file)
        {
            var pdfDoc = CreatePDFDocFromStorageFile(file);
            if (pdfDoc == null)
                return;

            var pdfTabInfo = new PdfTabInfo(file, pdfDoc);
            PdfTabs.Add(pdfTabInfo);
        }

        private async Task SaveAsAsync()
        {
            if (SelectedPdfTab == null)
                return;

            await SelectedPdfTab.SaveDocAsAsync();
        }

        private async Task SaveAsync()
        {
            if (SelectedPdfTab == null)
                return;

            await SelectedPdfTab.SaveDocAsync();
        }

        #endregion

        #region Events

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPdfTab))
            {
                foreach(var tab in PdfTabs)
                    tab.CloseContextMenu();
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create a new PDFDoc instance from a StorageFile object
        /// </summary>
        /// <param name="file">The StorageFile to be opened</param>
        /// <returns>An instance of PDFDoc</returns>
        private PDFDoc CreatePDFDocFromStorageFile(IStorageFile file)
        {
            if (file == null)
                return null;

            PDFDoc doc = null;

            try
            {
                doc = PDFDoc.CreateFromStorageFile(file);
                if (doc.InitSecurityHandler() == false)
                {
                    // password protected just ingnore for now
                    doc.Dispose();
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return doc;
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

        #endregion
    }
}
