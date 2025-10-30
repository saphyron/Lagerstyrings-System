using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ----------------------- Razor Pages + auth-konventioner -----------------------
builder.Services.AddRazorPages(opts => {
    // All pages require auth...
    opts.Conventions.AuthorizeFolder("/");

    // ...except login-page
    opts.Conventions.AllowAnonymousToPage("/Account/Login");

    // "/" -> /Account/Login
    opts.Conventions.AddPageRoute("/Account/Login", "");
});

// ----------------------- JWT-konfiguration ----------------
var issuer = builder.Configuration["Jwt:Issuer"] ?? "your-issuer";
var audience = builder.Configuration["Jwt:Audience"] ?? "your-audience";
var key = builder.Configuration["Jwt:Secret"] ?? "dev-secret-change-me";
var signing = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new() {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signing,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };

        o.Events = new JwtBearerEvents {
            OnMessageReceived = ctx => {
                if (string.IsNullOrEmpty(ctx.Token) &&
                    ctx.Request.Cookies.TryGetValue("AuthToken", out var jwt)) {
                    ctx.Token = jwt;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ----------------------- HttpClient to backend-API -------------------------
builder.Services.AddHttpContextAccessor();
var apiBase = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5107"; // BACKEND base-URL
builder.Services.AddHttpClient("Api", c => {
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