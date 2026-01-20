using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Global_Strategist_Platform_Server.Data;
using Global_Strategist_Platform_Server.Interface.Repositories;
using Global_Strategist_Platform_Server.Implementation.Repositories;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Implementation.Services;
using Global_Strategist_Platform_Server.Gateway.EmailSender;
using Global_Strategist_Platform_Server.Gateway.FileManager;

var builder = WebApplication.CreateBuilder(args);

// Add CORS configuration - Allow all origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TBP Platform API",
        Version = "v1",
        Description = "TBP Platform Web API with JWT Authentication"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter ONLY your token (without 'Bearer' prefix) in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReactionService, ReactionService>();
builder.Services.AddScoped<ICorporateService, CorporateService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IFileManager, FileManager>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured in appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            try
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                var userIdClaim = context.Principal?.FindFirst("sub")?.Value;
                
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    logger?.LogInformation("Token validated for user: {UserId}", userId);
                    
                    try
                    {
                        var userRepository = context.HttpContext.RequestServices.GetRequiredService<Global_Strategist_Platform_Server.Interface.Repositories.IBaseRepository<Global_Strategist_Platform_Server.Model.Entities.User>>();
                        var user = await userRepository.GetByIdAsync(userId);
                        
                        if (user == null)
                        {
                            logger?.LogWarning("Token validated but user not found in database: {UserId}", userId);
                        }
                        else if (!user.IsActive)
                        {
                            logger?.LogWarning("Token validated but user is inactive: {UserId}", userId);
                            context.Fail("User account is inactive.");
                            return;
                        }
                    }
                    catch (Exception dbEx)
                    {
                        logger?.LogError(dbEx, "Error checking user status during token validation - allowing token");
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                logger?.LogError(ex, "Error in OnTokenValidated - allowing token to proceed");
            }
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
            var exception = context.Exception;
            logger?.LogError(exception, "JWT Authentication failed. Exception: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                exception?.GetType().Name, 
                exception?.Message,
                exception?.StackTrace);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await DataSeeder.SeedDefaultDataAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
//}

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
