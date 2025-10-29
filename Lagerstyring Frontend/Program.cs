using System.Text;
using System.IdentityModel.Tokens.Jwt;                 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Logging; //temp: for showing PII in dev

var builder = WebApplication.CreateBuilder(args);

// ----------------------- Razor Pages + auth-konventioner -----------------------
builder.Services.AddRazorPages(opts =>
{
    // All pages require auth...
    opts.Conventions.AuthorizeFolder("/");

    // ...except login-page
    opts.Conventions.AllowAnonymousToPage("/Account/Login");

    // "/" -> /Account/Login
    opts.Conventions.AddPageRoute("/Account/Login", "");
});

// ----------------------- JWT-konfiguration ----------------
var issuer   = builder.Configuration["Jwt:Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer missing");
var audience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
//Console.WriteLine($"JWT Key SHA256 (frontend): {KeyFingerprint(key)}"); // debug

var signing = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

// DEV: se mere detaljerede fejl (slå FRA igen når det virker)
/*IdentityModelEventSource.ShowPII = true;

static string KeyFingerprint(string key)
{
    using var sha = System.Security.Cryptography.SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(key);
    var hash = sha.ComputeHash(bytes);
    return Convert.ToHexString(hash); // f.eks. "A1B2C3..."
}*/


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signing,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token) &&
                    ctx.Request.Cookies.TryGetValue("AuthToken", out var jwt))
                {
                    ctx.Token = jwt;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                // Token kan ikke valideres (forkert nøgle/udløbet/korrupt) => drop cookien
                ctx.Response.Cookies.Delete("AuthToken", new CookieOptions { Path = "/" });
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse(); // undertryk standard 401-svar
                ctx.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ----------------------- HttpClient to backend-API -------------------------
builder.Services.AddHttpContextAccessor(); 
var apiBase = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5107"; // BACKEND base-URL
builder.Services.AddHttpClient("Api", c =>
{
    c.BaseAddress = new Uri(apiBase); 
});

// ----------------------- Build + middleware pipeline ----------------------------
var app = builder.Build();
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
