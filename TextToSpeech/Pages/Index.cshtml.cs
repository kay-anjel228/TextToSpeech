using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TextToSpeech.Data;
using TextToSpeech.Models;
using TextToSpeech.Services;

namespace TextToSpeech.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ICurrentUserService _currentUserService;
        public IndexModel(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _currentUserService = currentUserService;
        }
        [BindProperty]
        public string RegisterName { get; set; } = string.Empty;
        [BindProperty]
        public string RegisterLogin { get; set; } = string.Empty;
        [BindProperty]
        public string RegisterPassword { get; set; } = string.Empty;
        [BindProperty]
        public string RegisterConfirmPassword { get; set; } = string.Empty;
        [BindProperty]
        public string LoginLogin { get; set; } = string.Empty;
        [BindProperty]
        public string LoginPassword { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; }
        public string CurrentUserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
            LoadCurrentUser();
        }

        public IActionResult OnPostRegister()
        {
            LoadCurrentUser();

            if (string.IsNullOrEmpty(RegisterName) ||
                string.IsNullOrEmpty(RegisterLogin) ||
                string.IsNullOrEmpty(RegisterPassword) ||
                string.IsNullOrEmpty(RegisterConfirmPassword))
            {
                Message = "Заполните все поля регистрации";
                return Page();
            }

            if (RegisterConfirmPassword != RegisterPassword)
            {
                Message = "Пароли не совпадают";
                return Page();
            }

            bool loginExist = _context.Users.Any(u => u.Login == RegisterLogin);

            if (loginExist)
            {
                Message = "Такой логин уже существует";
                return Page();
            }

            var user = new User
            {
                Name = RegisterName,
                Login = RegisterLogin
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, RegisterPassword);
            _context.Users.Add(user);
            _context.SaveChanges();
            _currentUserService.SignIn(HttpContext, user.Id);

            return RedirectToPage("/Reader");
        }

        public IActionResult OnPostLogin()
        {
            LoadCurrentUser();

            if (string.IsNullOrEmpty(LoginLogin) ||
                string.IsNullOrEmpty(LoginPassword))
            {
                Message = "Введите логин и пароль";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.Login == LoginLogin);

            if (user == null)
            {
                Message = "Неверный логин или пароль";
                return Page();
            }

            var result = _passwordHasher.VerifyHashedPassword
            (
                user,
                user.PasswordHash,
                LoginPassword
            );

            if (result == PasswordVerificationResult.Failed)
            {
                Message = "Неверный логин или пароль";
                return Page();
            }

            _currentUserService.SignIn(HttpContext, user.Id);

            return RedirectToPage("/Reader");
        }

        public IActionResult OnPostLogout()
        {
            _currentUserService.SignOut(HttpContext);
            return RedirectToPage();
        }

        private void LoadCurrentUser()
        {
            var user = _currentUserService.GetCurrentUser(HttpContext);

            if (user == null)
            {
                IsAuthorized = false;
                return;
            }

            IsAuthorized = true;
            CurrentUserName = user.Name;
        }
    }
}

