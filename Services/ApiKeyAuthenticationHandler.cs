using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X_API_KEY";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
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

        //string apiKey = Environment.GetEnvironmentVariable("X_API_KEY");
        string apiKey = "F3tdRvZbApkGV4TAqgwL6g30iFZlJf"; //TODO

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