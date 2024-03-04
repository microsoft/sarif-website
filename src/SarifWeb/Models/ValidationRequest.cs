using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;

namespace SarifWeb.Models
{
    public class ValidationRequest
    {
        public string PostedFileName { get; set; }
        public string SavedFileName { get; set; }
        public List<RuleKind> RuleKinds { get; set; } = new List<RuleKind>();
    }
}