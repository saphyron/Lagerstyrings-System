using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(opts =>
{

    opts.Conventions.AuthorizeFolder("/");


    opts.Conventions.AllowAnonymousToPage("/Account/Login");


    opts.Conventions.AddPageRoute("/Account/Login", "");
});


var issuer   = builder.Configuration["Jwt:Issuer"]   ?? "your-issuer";
var audience = builder.Configuration["Jwt:Audience"] ?? "your-audience";
var key      = builder.Configuration["Jwt:Secret"]   ?? "dev-secret-change-me";
var signing  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));


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
            ValidateLifetime = true
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
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("Self", (sp, client) =>
{
    var http = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var scheme = http?.Request?.Scheme ?? "https";
    var host   = http?.Request?.Host.Value ?? "localhost:7267"; // din HTTPS-port
    client.BaseAddress = new Uri($"{scheme}://{host}");
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
