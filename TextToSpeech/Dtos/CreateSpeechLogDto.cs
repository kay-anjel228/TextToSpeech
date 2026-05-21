using System.ComponentModel.DataAnnotations;

namespace TextToSpeech.Dtos
{
    public class CreateSpeechLogDto
    {
        [Required]
        public string Text { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public double Rate { get; set; } = 1;
    }
}
