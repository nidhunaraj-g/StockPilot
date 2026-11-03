
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockPilot.Infrastructure;
using StockPilot.Infrastructure.Persistence;
using StockPilot.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is missing.");

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
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
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var customerIdValue = context.Principal?
                    .FindFirstValue("CustomerId");

                // Admin tokens do not contain CustomerId.
                // Their authentication is handled by role authorization.
                if (string.IsNullOrWhiteSpace(customerIdValue))
                    return;

                var userIdValue = context.Principal?
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(customerIdValue, out var customerId) ||
                    !int.TryParse(userIdValue, out var userId))
                {
                    context.Fail("Invalid customer identity.");
                    return;
                }

                await using var scope = context.HttpContext
                    .RequestServices
                    .CreateAsyncScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<StockPilotDbContext>();

                var isActive = await dbContext.Customers
                    .AsNoTracking()
                    .AnyAsync(
                        customer =>
                            customer.Id == customerId &&
                            customer.ApplicationUserId == userId &&
                            customer.IsActive,
                        context.HttpContext.RequestAborted);

                if (!isActive)
                {
                    context.Fail(
                        "This customer account is inactive or unavailable.");
                }
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
