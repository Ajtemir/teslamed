using TeslaMed.Models;

namespace TeslaMed.ViewModels
{
    public class ResearchMethodRatingViewModel
    {
        public int ResearchMethodId { get; set; }
        public ResearchMethod? ResearchMethod { get; set; }
        public double AverageRating { get; set; } 
        public ResearchMethodRatingViewModel()
        {
            AverageRating = 0;
        }
    }
}
