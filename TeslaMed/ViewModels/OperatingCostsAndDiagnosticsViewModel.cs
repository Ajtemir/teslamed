using Microsoft.CodeAnalysis;
using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class OperatingCostsAndDiagnosticsViewModel
    {
        public IQueryable<Diagnostics> Diagnostics { get; set; }
        public IQueryable<OperatingCost> OperatingCosts { get; set; } 
    }
}
