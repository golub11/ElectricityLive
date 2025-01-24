using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X_API_KEY";
   
    private string apiKey;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        apiKey = configuration.GetSection(ApiKeyHeaderName).Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Method == "OPTIONS")
        {
            return AuthenticateResult.NoResult();
        }

        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
        {
            return AuthenticateResult.Fail("API Key was not provided.");
        }

            
        if (!potentialApiKey.Equals(apiKey))
        {
            return AuthenticateResult.Fail("Invalid API Key provided.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, apiKey)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principals = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principals, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
