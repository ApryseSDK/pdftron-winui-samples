using System;

namespace PDFViewer_WinUI3Demo.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
