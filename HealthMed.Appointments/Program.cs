using Appointments.Infra.Messaging;
using HealthMed.Appointments.Application.Clients;
using HealthMed.Appointments.Application.Events;
using HealthMed.Appointments.Application.Services;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Infrastructure.Data;
using HealthMed.Appointments.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.WebHost.UseUrls("http://0.0.0.0:80");

// Configuração do banco de dados
builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Configuração do RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var rabbitHost = configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("RabbitMQ Host is missing.");
    var rabbitPortStr = configuration["RabbitMQ:Port"] ?? "5672";
    var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
    var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

    if (!int.TryParse(rabbitPortStr, out var rabbitPort))
        throw new InvalidOperationException("RabbitMQ Port is invalid.");

    var factory = new ConnectionFactory
    {
        HostName = rabbitHost,
        Port = rabbitPort,
        UserName = rabbitUser,
        Password = rabbitPass
    };

    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateModel();
});

builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddSingleton<ScheduleClient>();
builder.Services.AddScoped<MessagePublisher>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// HTTP Client para Schedule
builder.Services.AddHttpClient<ScheduleClient>(client =>
{
    client.BaseAddress = new Uri("http://schedule.api");
});
builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new Uri("http://auth.api"); // nome do container no Docker
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HealthMed.Appointments.API", Version = "v1" });

    // JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite: Bearer {seu_token_jwt}"
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

// JWT
var jwtKey = configuration["Jwt:Key"]!;
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
