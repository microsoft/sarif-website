namespace SarifWeb.Models
{
    public class ValidationResponse
    {
        public string Message { get; set; }
        public int ExitCode { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public string Arguments { get; internal set; }
        public object LogContents { get; internal set; }
    }
}