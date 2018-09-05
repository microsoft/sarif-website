using System.Diagnostics;
using System.Threading.Tasks;

namespace SarifWeb.Utilities
{
    public class ProcessRunner : IProcessRunner
    {
        public async Task<ProcessResult> RunProcess(string exePath, string arguments)
        {
            var processResult = new ProcessResult();

            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);

                process.Dispose();
            };

            process.Start();
            processResult.StandardOutput = process.StandardOutput.ReadToEnd();
            processResult.StandardError = process.StandardError.ReadToEnd();

            processResult.ExitCode = await tcs.Task;

            return processResult;
        }
    }
}