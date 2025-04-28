using HealthMed.Appointments.Application.Clients;
using HealthMed.Appointments.Application.Interfaces;
using HealthMed.Appointments.Application.Services;
using HealthMed.Appointments.Domain.Enums;               // para AppointmentStatus
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Infrastructure.Data;
using HealthMed.Appointments.Infrastructure.Handlers;
using HealthMed.Appointments.Infrastructure.Messaging;
using HealthMed.Appointments.Infrastructure.Repositories;
using HealthMed.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;                            // para OpenApiString
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;                   // JsonStringEnumConverter

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// 1) Porta
builder.WebHost.UseUrls("http://0.0.0.0:80");

// 2) EF Core
builder.Services.AddDbContext<AppointmentsDbContext>(opt =>
    opt.UseSqlServer(cfg.GetConnectionString("DefaultConnection"))
);

// 3) Conexão RabbitMQ
builder.Services.AddSingleton<IConnection>(sp => {
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

// 4) Repositórios e serviços
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAvailableSlotProjectionRepository, AvailableSlotProjectionRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// 5) Publisher — agora recebe IConnection e cria seu próprio canal
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

// 6) HostedServices (consumidores de eventos para popular projeções)
builder.Services.AddHostedService<SlotCreatedConsumer>();
builder.Services.AddHostedService<SlotDeletedConsumer>();

// 7) HTTP + Auth
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new Uri(cfg["AuthApi:BaseUrl"]!);
})
.AddHttpMessageHandler<AuthTokenHandler>();

// 8) Controllers + JSON enum as string
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

// 9) Swagger + JWT + enum doc
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Appointments.API", Version = "v1" });

    // security
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Digite: Bearer {seu_token}"
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

    // documenta AppointmentStatus como string e lista de valores
    c.MapType<AppointmentStatus>(() =>
        new OpenApiSchema
        {
            Type = "string",
            Enum = Enum
                .GetNames(typeof(AppointmentStatus))
                .Select(n => new OpenApiString(n) as IOpenApiAny)
                .ToList()
        }
    );
});

var jwtKey = Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
