// GrainBroker.Api/Program.cs
using GrainBroker.Core.Mappings;
using GrainBroker.Core.Services;
using GrainBroker.Data;
using GrainBroker.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

var builder = WebApplication.CreateBuilder(args);

// ---------- Data ----------
builder.Services.AddDbContext<GrainBrokerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("GrainBroker"))
       .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.AddAutoMapper(typeof(OrderMappingProfile));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ---------- AuthN/AuthZ ----------
var tenantId = builder.Configuration["AzureAd:TenantId"]!;
var apiClientId = builder.Configuration["AzureAd:Api:ClientId"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudiences = new[]
            {
                apiClientId,
                $"api://{apiClientId}"
            },
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{tenantId}/v2.0",
                $"https://sts.windows.net/{tenantId}/"
            },
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BrokerRead", p => p.RequireRole("Broker.Read"));
    options.AddPolicy("BrokerWrite", p => p.RequireRole("Broker.Write"));
});

// ---------- CORS (Angular dev) ----------
builder.Services.AddCors(o =>
{
    o.AddPolicy("AllowLocalAngular", p => p
        .WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200" // if you run `ng serve --ssl`
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

// ---------- MVC / Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Grain Broker API", Version = "v1" });

    // Paste a Bearer token in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste JWT here (format: Bearer {token})",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GrainBrokerDbContext>();
    db.Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS MUST be before auth/authorization and endpoints
app.UseCors("AllowLocalAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
