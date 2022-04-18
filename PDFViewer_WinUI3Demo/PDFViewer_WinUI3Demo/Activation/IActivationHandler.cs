using System.Threading.Tasks;

namespace PDFViewer_WinUI3Demo.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
