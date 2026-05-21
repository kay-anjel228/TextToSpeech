using Microsoft.AspNetCore.Mvc;
using TextToSpeech.Dtos;
using TextToSpeech.Models;
using TextToSpeech.Services;

namespace TextToSpeech.Controllers.Api
{
    public class SpeechLogApiControllers
    {
        [ApiController]
        [Route("/api/speechlogs")]
        public class SpeechLogsApiController : ControllerBase
        {
            private readonly ISpeechLogService _speechLogService;
            private readonly ICurrentUserService _currentUserService;

            public SpeechLogsApiController(
                ISpeechLogService speechLogService, ICurrentUserService currentUserService)
            {
                _speechLogService = speechLogService;
                _currentUserService = currentUserService;
            }

            [HttpGet]
            public ActionResult<List<SpeechLogDto>> GetAll()
            {
                var logs = _speechLogService.GetAllLogs();
                var res = logs.Select(log => ToDo(log).ToList);
                return Ok(res);
            }

            [HttpPost]
            public IActionResult Create(CreateSpeechLogDto dto)
            {
                var userId = _currentUserService.GetCurrentUserId(HttpContext);

                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        message = "Нужно войти в аккаунт"
                    });
                }
                var log = new SpeechLog
                {
                    Text = dto.Text,
                    VoiceName = dto.VoiceName,
                    Rate = dto.Rate,
                    CreatedAt = DateTime.Now,
                    UserId = userId.Value
                };
                _speechLogService.AddLog(log);
                return Ok(new
                {
                    message = "Лог сохранён"
                });
            }

            private static SpeechLogDto ToDto(SpeechLog log)
            {
                return new SpeechLogDto()
                {
                    Id = log.Id,
                    Text = log.Text,
                    VoiceName= log.VoiceName,
                    Rate= log.Rate,
                    CreateAt = log.CreatedAt,
                    UserName = log.User?.Name
                };
            }
        }
    }
}
