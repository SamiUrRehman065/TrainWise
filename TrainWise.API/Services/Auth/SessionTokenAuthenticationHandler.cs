using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TrainWise.API.Services.Auth;

public sealed class SessionTokenAuthenticationHandler : AuthenticationHandler<SessionTokenAuthenticationOptions>
{
    private readonly ISessionStore _sessionStore;

    public SessionTokenAuthenticationHandler(
        IOptionsMonitor<SessionTokenAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISessionStore sessionStore)
        : base(options, logger, encoder)
    {
        _sessionStore = sessionStore;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Session-Token", out var tokenValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = tokenValues.ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!_sessionStore.TryGetUserId(token, out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired session token."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
