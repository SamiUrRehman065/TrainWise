using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TrainWise.API.Configuration;
using TrainWise.API.Data;
using TrainWise.API.Middleware;
using TrainWise.API.Services.Auth;
using TrainWise.API.Services.Datasets;
using TrainWise.API.Services.Experiments;
using TrainWise.API.Services.ML;
using TrainWise.API.Services.Training;
using TrainWise.API.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<UploadOptions>(builder.Configuration.GetSection("Upload"));
builder.Services.Configure<MLServiceOptions>(builder.Configuration.GetSection("MLService"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<DatasetStorageOptions>(builder.Configuration.GetSection("DatasetStorage"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IMLServiceClient, MLServiceClient>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDatasetService, DatasetService>();
builder.Services.AddScoped<IDatasetStorageService, DatasetStorageService>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<IExperimentService, ExperimentService>();
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddHostedService<DatasetArchiveBackgroundService>();

builder.Services.AddAuthentication("Session")
    .AddScheme<SessionTokenAuthenticationOptions, SessionTokenAuthenticationHandler>("Session", null);

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var isDevelopment = builder.Environment.IsDevelopment();
        if (!isDevelopment && allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin in non-development environments.");
        }

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }

        if (isDevelopment)
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Scheme is "http" or "https"
                       && (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                           || uri.Host.Equals("127.0.0.1"));
            });
        }

        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FileUploadOperationFilter>();

    options.AddSecurityDefinition("SessionToken", new OpenApiSecurityScheme
    {
        Name = "X-Session-Token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Scheme = "Session",
        Description = "Enter your session token obtained from POST /api/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "SessionToken"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
