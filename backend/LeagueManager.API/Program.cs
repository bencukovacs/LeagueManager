using LeagueManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LeagueManager.Infrastructure.Services;
using LeagueManager.Application.Services;
using LeagueManager.Application.MappingProfiles;
using LeagueManager.API.Middleware;
using LeagueManager.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LeagueManager.Application.Settings;
using Microsoft.OpenApi.Models;
using LeagueManager.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using FluentValidation.AspNetCore;
using FluentValidation;
using LeagueManager.Application.Validators;
using Serilog;
using Polly;
using Npgsql;

const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // Your React app's address
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<LeagueDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<LeagueDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanManageTeam", policy =>
        policy.AddRequirements(new CanManageTeamRequirement()))
    .AddPolicy("CanUpdatePlayer", policy =>
        policy.AddRequirements(new CanUpdatePlayerRequirement()))
    .AddPolicy("CanSubmitResult", policy =>
        policy.AddRequirements(new CanSubmitResultRequirement()));



builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTeamDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PlayerDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LocationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFixtureDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateFixtureDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitResultDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<MomVoteDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GoalscorerDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateResultStatusDtoValidator>();

builder.Services.AddScoped<ILeagueTableService, LeagueTableService>();
builder.Services.AddScoped<ITopScorersService, TopScorersService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IFixtureService, FixtureService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILeagueConfigurationService, LeagueConfigurationService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IAuthorizationHandler, CanManageTeamHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanUpdatePlayerHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CanSubmitResultHandler>();

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Run the Seeder and Migrations with a Retry Policy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        // Define a retry policy: try 5 times, waiting 5 seconds between each attempt.
        var retryPolicy = Policy
            .Handle<NpgsqlException>() // Only retry on a Postgres connection error
            .WaitAndRetryAsync(5, RetryAttempt => 
            {
                var TimeToWait = TimeSpan.FromSeconds(Math.Pow(2, RetryAttempt));
                logger.LogWarning("Database connection failed. Waiting {TimeToWait} before next retry. Attempt {RetryAttempt}", TimeToWait, RetryAttempt);
                return TimeToWait;
            });

        // Execute the migration and seeding within the retry policy
        await retryPolicy.ExecuteAsync(async () =>
        {
            logger.LogInformation("Starting database setup (migrations and seeding)...");
            
            var dbContext = services.GetRequiredService<LeagueDbContext>();
            await dbContext.Database.MigrateAsync();
            
            await DbSeeder.SeedRolesAndAdminAsync(services);
            
            logger.LogInformation("Database setup completed successfully.");
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding after multiple retries.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
