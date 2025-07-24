using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeagueManager.Infrastructure.Services;
using LeagueManager.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LeagueDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

builder.Services.AddScoped<ILeagueTableService, LeagueTableService>();
builder.Services.AddScoped<ITopScorersService, TopScorersService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
