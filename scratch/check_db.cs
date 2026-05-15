using Microsoft.EntityFrameworkCore;
using TrainWise.API.Data;
using TrainWise.API.Data.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Server=localhost,1433;Database=TrainWiseDb;User Id=sa;Password=CHANGE_ME_IN_ENV;TrustServerCertificate=True";

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new AppDbContext(optionsBuilder.Options);

var users = await context.Users.ToListAsync();
Console.WriteLine($"Total Users: {users.Count}");
foreach (var u in users) Console.WriteLine($"- {u.Username} ({u.UserId})");

var datasets = await context.Datasets.ToListAsync();
Console.WriteLine($"Total Datasets: {datasets.Count}");

var experiments = await context.Experiments.ToListAsync();
Console.WriteLine($"Total Experiments: {experiments.Count}");
