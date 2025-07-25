using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeagueManager.Infrastructure.Services;
using LeagueManager.Application.Services;
using LeagueManager.Application.MappingProfiles;
using LeagueManager.API.Middleware;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LeagueDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<LeagueDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddScoped<ILeagueTableService, LeagueTableService>();
builder.Services.AddScoped<ITopScorersService, TopScorersService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
