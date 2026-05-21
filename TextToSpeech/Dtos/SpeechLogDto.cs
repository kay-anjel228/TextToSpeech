namespace TextToSpeech.Dtos
{
    public class SpeechLogDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public double Rate { get; set; }
        public DateTime CreateAt { get; set; }
        public string? UserName { get; set; }
    }
}
