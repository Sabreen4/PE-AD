using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using NotificationService.API.Hubs;
using NotificationService.Application;
using NotificationService.Application.Contracts;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Mappings;
using NotificationService.Application.Services;
using NotificationService.Application.Validators;
using NotificationService.Infrastructure.Common;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Settings;
using Shared.InterServiceHelper.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.Application.Features.Notifications.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Shared.Authorization.Interfaces;
using Shared.Authorization.Middleware;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared.Authorization.Services;
using Shared.Common.Constants;
using Shared.Logging.Configuration;
using Shared.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5203, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenLocalhost(5204, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.AddSharedLogging("NotificationService");

String connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in configuration.");

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql =>
            npgsql.MigrationsHistoryTable(
                "__ef_migrations_history_notification",
                SchemaConstants.NOTIFICATION_SCHEMA
            )
    )
);

String jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
    });
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(SendRegistrationEmailRequestDtoValidator).Assembly);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();
builder.Services.AddSignalR();

builder.Services.AddScoped<IUserContextService, UserContextService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(SendRegistrationEmailCommand).Assembly);
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddMetaDataGrpcClient(builder.Configuration);

builder.Services.AddScoped<IRepoWrapper, RepoWrapper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();
builder.Services.AddScoped<INotificationTypeResolver, MetadataServiceNotificationTypeResolver>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

WebApplication app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSharedRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<NotificationService.API.Grpc.Services.NotificationGrpcService>();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
