// GrainBroker.Api/Program.cs
using GrainBroker.Core.Services;
using GrainBroker.Data;
using GrainBroker.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext (LocalDB for dev)
builder.Services.AddDbContext<GrainBrokerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("GrainBroker"))
       .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

// Repos + Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ? Register AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// (Dev) auto-apply migrations so your local DB is created
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GrainBrokerDbContext>();
    db.Database.Migrate();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
