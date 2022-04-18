using System.Threading.Tasks;

namespace PDFViewer_WinUI3Demo.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
