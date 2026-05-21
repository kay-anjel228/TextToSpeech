using Microsoft.EntityFrameworkCore;
using TextToSpeech.Data;
using TextToSpeech.Models;

namespace TextToSpeech.Services
{
    public class SpeechLogService : ISpeechLogService
    {
        private readonly AppDbContext _context;

        public SpeechLogService(AppDbContext context)
        {
            _context = context;
        }

        public List<SpeechLog> GetAllLogs()
        {
            return _context.SpeechLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }

        public List<SpeechLog> GetLogsByUserId(int userId)
        {
            return _context.SpeechLogs
                .Include(l => l.User)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }

        public void AddLog(SpeechLog log)
        {
            _context.SpeechLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
