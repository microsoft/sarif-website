namespace SarifWeb.Models
{
    public class ValidationResponse
    {
        public string Message { get; set; }
        public int ExitCode { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
    }
}