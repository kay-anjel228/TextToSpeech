using TextToSpeech.Models;

namespace TextToSpeech.Services
{
    public interface ISpeechLogService
    {
        List<SpeechLog> GetAllLogs();
        List<SpeechLog> GetLogsByUserId(int userId);
        void AddLog(SpeechLog log);
    }
}
