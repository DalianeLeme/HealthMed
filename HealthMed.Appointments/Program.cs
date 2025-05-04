using FluentValidation;
using FluentValidation.AspNetCore;
using HealthMed.Appointments.Application.Clients;
using HealthMed.Appointments.Application.Interfaces;
using HealthMed.Appointments.Application.Services;
using HealthMed.Appointments.Domain.Enums;
using HealthMed.Appointments.Domain.Interfaces;
using HealthMed.Appointments.Infrastructure.Data;
using HealthMed.Appointments.Infrastructure.Handlers;
using HealthMed.Appointments.Infrastructure.Messaging;
using HealthMed.Appointments.Infrastructure.Repositories;
using HealthMed.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.WebHost.UseUrls("http://0.0.0.0:80");

builder.Services.AddDbContext<AppointmentsDbContext>(opt =>
    opt.UseSqlServer(cfg.GetConnectionString("DefaultConnection"))
);

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

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAvailableSlotProjectionRepository, AvailableSlotProjectionRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

builder.Services.AddHostedService<SlotCreatedConsumer>();
builder.Services.AddHostedService<SlotDeletedConsumer>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new Uri(cfg["AuthApi:BaseUrl"]!);
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

builder.Services.AddValidatorsFromAssemblyContaining<ScheduleRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CancelAppointmentRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RescheduleRequestValidator>();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Appointments.API", Version = "v1" });

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();

    try
    {
        if (db.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Aplicando migrations pendentes (AppointmentsDbContext)...");
            db.Database.Migrate();
        }
    }
    catch (SqlException ex) when (ex.Number == 1801)
    {
        Console.WriteLine("Banco de dados já existe (AppointmentsDbContext).");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations no AppointmentsDbContext: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();