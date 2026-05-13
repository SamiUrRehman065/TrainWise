using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TrainWise.API.Configuration;
using TrainWise.API.Contracts.Auth;
using TrainWise.API.Data;
using TrainWise.API.Data.Models;

namespace TrainWise.API.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly ISessionStore _sessionStore;
    private readonly AuthOptions _authOptions;

    public AuthService(AppDbContext dbContext, ISessionStore sessionStore, IOptions<AuthOptions> authOptions)
    {
        _dbContext = dbContext;
        _sessionStore = sessionStore;
        _authOptions = authOptions.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var ttl = TimeSpan.FromHours(_authOptions.SessionTimeoutHours);
        var token = _sessionStore.CreateSession(user.UserId, ttl);
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse
        {
            SessionToken = token,
            UserId = user.UserId
        };
    }

    public async Task<LoginResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        if (request.Username.Length < 3 || request.Password.Length < 6)
        {
            return null;
        }

        // Check if username already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (existingUser is not null)
        {
            return null;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var ttl = TimeSpan.FromHours(_authOptions.SessionTimeoutHours);
        var token = _sessionStore.CreateSession(user.UserId, ttl);

        return new LoginResponse
        {
            SessionToken = token,
            UserId = user.UserId
        };
    }
}
