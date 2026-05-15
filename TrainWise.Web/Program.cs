using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TrainWise.Web;
using TrainWise.Web.Services.Api;
using TrainWise.Web.Services.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5002/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<SessionState>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthApi>();
builder.Services.AddScoped<DatasetApi>();
builder.Services.AddScoped<TrainingApi>();
builder.Services.AddScoped<ExperimentApi>();
builder.Services.AddScoped<UserApi>();

await builder.Build().RunAsync();
