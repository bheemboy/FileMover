using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

const string policyName = "CorsPolicy";

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: policyName,
        policy  =>
        {
            policy.WithOrigins("http://localhost", "http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }); 
});
builder.Services.AddSignalR(options => options.MaximumParallelInvocationsPerClient = 10);

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ??
    throw new Exception("JWT settings are not configured properly in appsettings.json");

builder.Services.AddSingleton(jwtSettings);

var applicationFolders = builder.Configuration.GetSection("ApplicationFolders").Get<List<ApplicationFolder>>() ??
    throw new Exception("Application Folders are not configured properly in appsettings.json");

builder.Services.AddSingleton(applicationFolders);

// Add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };

    // Add logging for JWT authentication events
    options.Events = new JwtBearerEvents
    {
        // We have to hook the OnMessageReceived event in order to
        // allow the JWT authentication handler to read the access
        // token from the query string when a WebSocket or 
        // Server-Sent Events request comes in.

        // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
        // due to a limitation in Browser APIs. We restrict it to only calls to the
        // SignalR hub in this code.
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hub")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(JwtBearerEvents));
            logger.LogError(context.Exception, "Authentication failed");
            return Task.CompletedTask;
        }
    };

});


// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseCors(policyName);

app.UseDefaultFiles();
app.UseStaticFiles();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", ([FromServices] JwtSettings jwtSettings, [FromBody] UserCredentials credentials) =>
{
    if (credentials.Username == "test" && credentials.Password == "movefiles")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, credentials.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
    return Results.Unauthorized();
}).AllowAnonymous();

app.MapHub<StorageHub>("/hub").RequireAuthorization("AuthenticatedUser");

app.Run();


public record JwtSettings(string Issuer, string Audience, string SecretKey);
public class ApplicationFolder
{
    public string ID {get; set;} = string.Empty;
    public string Title {get; set;} = string.Empty;
    public List<string> Paths {get; set;} = [];
}
public record UserCredentials(string Username, string Password);
