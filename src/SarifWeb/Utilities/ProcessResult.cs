namespace SarifWeb.Utilities
{
    public class ProcessResult
    {
        public string StandardOutput { get; internal set; }
        public string StandardError { get; internal set; }
        public int ExitCode { get; internal set; }
    }
}