using Microsoft.CodeAnalysis;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class DiagnosticsAndOperatingCosts
    {
        public List<Diagnostics> Diagnostics { get; set; }
        public List<OperatingCost> OperatingCosts { get; set; }
    }
}
