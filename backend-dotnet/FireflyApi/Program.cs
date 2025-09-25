using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using FireflyApi.Data;
using FireflyApi.Models;
using FireflyApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
var connectionString = $"Server={builder.Configuration["DB_SERVER"]};Database={builder.Configuration["DB_NAME"]};User={builder.Configuration["DB_USER"]};Password={builder.Configuration["DB_PASSWORD"]};";

// For development, use in-memory database if MySQL is not available
var useInMemoryDb = builder.Configuration["DB_SERVER"] == "localhost" || string.IsNullOrEmpty(builder.Configuration["DB_SERVER"]);

if (useInMemoryDb)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("FireflyInMemoryDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
}

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var secretKey = builder.Configuration["SECRET_KEY"] ?? throw new ArgumentNullException("SECRET_KEY not found");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Redis (optional for development)
var redisConnectionString = builder.Configuration["REDIS_CACHE"] ?? "localhost:6379";
try
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not connect to Redis: {ex.Message}");
    // Add a dummy Redis service for development
    builder.Services.AddSingleton<IConnectionMultiplexer>(provider => null!);
}

// Register custom services
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Configuration["PROJECT_NAME"] ?? "Firefly API",
        Version = builder.Configuration["PROJECT_VERSION"] ?? "1.0.0"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firefly API v1");
        c.RoutePrefix = "docs";
    });
}

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var firstUserEmail = builder.Configuration["FIRST_USER_EMAIL"];
    var firstUserFullName = builder.Configuration["FIRST_USER_FULLNAME"];
    var firstUserPassword = builder.Configuration["FIRST_USER_PASSWORD"];

    if (!string.IsNullOrEmpty(firstUserEmail) && !string.IsNullOrEmpty(firstUserFullName) && !string.IsNullOrEmpty(firstUserPassword))
    {
        var existingUser = await userManager.FindByEmailAsync(firstUserEmail);
        if (existingUser == null)
        {
            var firstUser = new User
            {
                UserName = firstUserEmail,
                Email = firstUserEmail,
                FullName = firstUserFullName
            };

            await userManager.CreateAsync(firstUser, firstUserPassword);
        }
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
