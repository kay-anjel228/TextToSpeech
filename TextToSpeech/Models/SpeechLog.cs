namespace TextToSpeech.Models
{
    public class SpeechLog
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public double Rate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
