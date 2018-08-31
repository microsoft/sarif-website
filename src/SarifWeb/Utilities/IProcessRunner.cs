using System.Threading.Tasks;

namespace SarifWeb.Utilities
{
    public interface IProcessRunner
    {
        Task<ProcessResult> RunProcess(string exePath, string arguments);
    }
}