// Schedule.API/Program.cs
using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Application.Services;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Schedule.Infrastructure.Data;
using HealthMed.Schedule.Infrastructure.Messaging;   // <- aqui
using HealthMed.Schedule.Infrastructure.Repositories;
using HealthMed.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Escuta em todas as interfaces na porta 80
builder.WebHost.UseUrls("http://0.0.0.0:80");

// 2) EF Core
builder.Services.AddDbContext<ScheduleDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// 3) RabbitMQ: registrar a conexão e o canal
builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = cfg["RabbitMQ:Host"]!,
        Port = int.Parse(cfg["RabbitMQ:Port"]!),
        UserName = cfg["RabbitMQ:Username"]!,
        Password = cfg["RabbitMQ:Password"]!
    };
    return factory.CreateConnection();
});

// este registro garante que quem pedir um IModel receba um canal novo
builder.Services.AddSingleton<IModel>(sp =>
    sp.GetRequiredService<IConnection>().CreateModel()
);

// 4) Repositório e serviço de domínio
builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<IAvailableSlotService, AvailableSlotService>();

// 5) Publisher de eventos
builder.Services.AddScoped<IEventPublisher, RabbitMqPublisher>();

// 6) HostedServices (consumidores)
//builder.Services.AddHostedService<SlotCreatedConsumer>();
//builder.Services.AddHostedService<SlotDeletedConsumer>();
builder.Services.AddHostedService<ConsultationCreatedConsumer>();
builder.Services.AddHostedService<ConsultationAcceptedConsumer>();
builder.Services.AddHostedService<ConsultationRejectedConsumer>();
builder.Services.AddHostedService<ConsultationCancelledConsumer>();
builder.Services.AddHostedService<GetAvailableSlotsConsumer>();

// 7) JWT Auth
var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

// 8) Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HealthMed.Schedule.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Use: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 9) Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Aplica migrações pendentes
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
    var pending = db.Database.GetPendingMigrations();
    if (pending.Any())
    {
        try { db.Database.Migrate(); }
        catch (SqlException ex) when (ex.Number == 1801) { /* ignora “Database already exists” */ }
    }
}

// pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
