using System.Text;
using System.Reflection;

using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

using DemoEF.Infrastructure.Data;
using DemoEF.Application.Interfaces;
using DemoEF.Application.Services;
using DemoEF.Infrastructure.Data.Seeders;
using DemoEF.WebApi.Middleware;
using DemoEF.Infrastructure.Security;
using DemoEF.Application.Validation.User;
using DemoEF.Common.Authorization;
using SmtpEmailService = DemoEF.Application.Services.SmtpEmailService;

using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using FluentValidation.AspNetCore;
using DemoEF.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

var smtpSection = builder.Configuration.GetSection("SMTP");
var smtpUser = smtpSection["User"];
var smtpPass = smtpSection["Pass"];

//JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),

            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

//Authorization Policy
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

//DI DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Controllers & Validation
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

//OAuth Options
builder.Services.Configure<GoogleOAuthOptions>(
    builder.Configuration.GetSection("Google"));

builder.Services.Configure<FacebookOAuthOptions>(
    builder.Configuration.GetSection("Facebook"));

//DI Seeders
builder.Services.AddScoped<ISeeder, UserDataSeeder>();
builder.Services.AddScoped<ISeeder, PermissionSeeder>();
builder.Services.AddScoped<DatabaseSeeder>();

//DI Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();
builder.Services.AddSingleton<IEmailService>(
    new SmtpEmailService(
        smtpUser: smtpUser!,
        smtpPass: smtpPass!
    )
);

//DI OAuth Clients
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGoogleOAuthClient, GoogleOAuthClient>();
builder.Services.AddScoped<IFacebookOAuthClient, FacebookOAuthClient>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Authentication vào Swagger
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header sử dụng Bearer scheme.'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoEF API V1");
        options.DocumentTitle = "DemoEF API Documentation";
    });
}

//Seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databaseSeeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await databaseSeeder.SeedAsync(db);
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
