namespace TextToSpeech.Middleware
{
    public class EdgeOnlyMiddleware
    {
        private readonly RequestDelegate _next;

        public EdgeOnlyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            if (path.StartsWith("/swagger"))
            {
                await _next(context);
                return;
            }

            var userAgent = context.Request.Headers.UserAgent.ToString();
            bool isEdge = userAgent.Contains("Edg/");

            if (isEdge)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync("""
                <!DOCTYPE html>
                <html lang="ru">
                <head>
                    <meta charset="utf-8" />
                    <title>Требуется Microsoft Edge<title>
                    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
                </head>
                <body>
                    <div class="container mt-5>
                        <div class="alert alert-warning">
                        <h1>Откройте сайт в Microsoft Edge</h1>
                        <p>Этот проект специально ограничен браузером Microsoft Edge</p>
                        </div>
                    </div>
                </body>
                </html>

                """);
                return;
            }

            await _next(context);
        }
    }
}
