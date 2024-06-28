using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class ConclusionAndImageViewModel
    {
        public Diagnostics Diagnostics { get; set; }
        public Patient Patient { get; set; }
        public DicomPathAndImagesPath Main { get; set; }
        public string Base64Image { get; set; }
        public int Age { get; set; }
    }
}
