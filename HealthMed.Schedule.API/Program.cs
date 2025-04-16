using HealthMed.Schedule.Application.Services;
using HealthMed.Schedule.Domain.Interfaces;
using HealthMed.Schedule.Infrastructure.Data;
using HealthMed.Schedule.Infrastructure.Messaging;
using HealthMed.Schedule.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

var configuration = builder.Configuration;
var rabbitHost = configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("RabbitMQ Host is missing.");
var rabbitPortStr = configuration["RabbitMQ:Port"] ?? "5672";
var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

if (!int.TryParse(rabbitPortStr, out var rabbitPort))
{
    throw new InvalidOperationException("RabbitMQ Port is invalid.");
}

var factory = new ConnectionFactory
{
    HostName = rabbitHost,
    Port = rabbitPort,
    UserName = rabbitUser,
    Password = rabbitPass
};

var connection = factory.CreateConnection();
var channel = connection.CreateModel();

// Configuração do banco de dados (SQL Server)
builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de repositórios e serviços
builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<AvailableSlotService>();
builder.Services.AddSingleton(channel);
builder.Services.AddHostedService<ConsultationCreatedConsumer>();
builder.Services.AddHostedService<ConsultationCancelledConsumer>();
builder.Services.AddHostedService<ConsultationRescheduledConsumer>();
builder.Services.AddHostedService<ConsultationRejectedConsumer>();
builder.Services.AddHostedService<ConsultationAcceptedConsumer>();
builder.Services.AddHostedService<GetAvailableSlotsConsumer>();

// Autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Swagger com suporte a JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        Description = "Digite: Bearer {seu token JWT}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
