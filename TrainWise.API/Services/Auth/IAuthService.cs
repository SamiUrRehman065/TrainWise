using TrainWise.API.Contracts.Auth;

namespace TrainWise.API.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<LoginResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken);
}
