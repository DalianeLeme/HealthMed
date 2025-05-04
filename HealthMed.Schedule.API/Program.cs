using FluentValidation;
using FluentValidation.AspNetCore;
using HealthMed.Schedule.Application.Interfaces;
using HealthMed.Schedule.Application.Services;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Schedule.Infrastructure.Data;
using HealthMed.Schedule.Infrastructure.Messaging;
using HealthMed.Schedule.Infrastructure.Repositories;
using HealthMed.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

builder.Services.AddDbContext<ScheduleDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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

builder.Services.AddSingleton<IModel>(sp =>
    sp.GetRequiredService<IConnection>().CreateModel()
);

builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<IAvailableSlotService, AvailableSlotService>();

builder.Services.AddScoped<IEventPublisher, RabbitMqPublisher>();

builder.Services.AddHostedService<ConsultationCreatedConsumer>();
builder.Services.AddHostedService<ConsultationAcceptedConsumer>();
builder.Services.AddHostedService<ConsultationRejectedConsumer>();
builder.Services.AddHostedService<ConsultationCancelledConsumer>();
builder.Services.AddHostedService<GetAvailableSlotsConsumer>();

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

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

builder.Services.AddValidatorsFromAssemblyContaining<CreateAvailableSlotRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateAvailableSlotRequestValidator>();

builder.Services.AddFluentValidationAutoValidation();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

    try
    {
        if (db.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Aplicando migrations pendentes (ScheduleDbContext)...");
            db.Database.Migrate();
        }
    }
    catch (SqlException ex) when (ex.Number == 1801)
    {
        Console.WriteLine("Banco de dados já existe (ScheduleDbContext).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations no ScheduleDbContext: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
