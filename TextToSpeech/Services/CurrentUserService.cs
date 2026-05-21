using TextToSpeech.Data;
using TextToSpeech.Models;

namespace TextToSpeech.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly AppDbContext _context;

        public CurrentUserService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsAuthenticated(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("UserId") != null;
        }

        public int? GetCurrentUserId(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("UserId");
        }

        public User? GetCurrentUser(HttpContext httpContext)
        {
            var userId = GetCurrentUserId(httpContext);
            if (userId == null)
            {
                return null;
            }
            return _context.Users.FirstOrDefault(u => u.Id == userId.Value);
        }

        public void SignIn(HttpContext httpContext, int userId)
        {
            httpContext.Session.SetInt32("UserId", userId);
        }

        public void SignOut(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }
    }
}
