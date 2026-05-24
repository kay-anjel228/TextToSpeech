using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Services;

namespace TextToSpeech.Controlllers
{
    public class AdminLogsControllers : Controller
    {
        private readonly ISpeechLogService _speechLogService;
        private readonly ICurrentUserService _currentUserService;

        public AdminLogsControllers(ISpeechLogService speechLogService, ICurrentUserService currentUserService)
        {
            _speechLogService = speechLogService;
            _currentUserService = currentUserService;
        }

        public IActionResult Index()
        {
            if (!_currentUserService.IsAuthenticated(HttpContext))
            {
                return RedirectToPage("/Index");
            }
            var log = _speechLogService.GetAllLogs();
            return View(log);
        }
    }
}
