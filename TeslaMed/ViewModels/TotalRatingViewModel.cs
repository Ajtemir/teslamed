namespace TeslaMed.ViewModels
{
    public class TotalRatingViewModel
    {
        public List<ResearchMethodRatingViewModel> ResearchMethodRatings { get; set; }
        public double TotalRating { get; set; }
        public string AssistentName { get; set; }
        public TotalRatingViewModel()
        {
            TotalRating = 0;
        }
    }
}
