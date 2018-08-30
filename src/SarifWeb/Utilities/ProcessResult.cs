namespace SarifWeb.Utilities
{
    public class ProcessResult
    {
        public string StdOut { get; internal set; }
        public string StdErr { get; internal set; }
        public int ExitCode { get; internal set; }
    }
}