using System.ComponentModel;
using TextToSpeech.Models;

namespace TextToSpeech.Services
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated(HttpContext httpContext);
        int? GetCurrentUserId(HttpContext httpContext);
        User? GetCurrentUser(HttpContext httpContext);
        void SignIn(HttpContext httpContext, int userId);
        void SignOut(HttpContext httpContext);
    }
}
