namespace TeslaMed.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int CreatorId { get; set; }
        public User? Creator { get; set; }
        public DateTime Created { get; set; }
        public int DiagnosticsId { get; set; }
        public Diagnostics? Diagnostics { get; set; }
    }
}
