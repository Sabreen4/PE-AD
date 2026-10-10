using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.API.Grpc.Services;
using IdentityService.Application.Contracts;
using IdentityService.Application.Features.Auth.Commands;
using IdentityService.Application.Mappings;
using IdentityService.Application.Services;
using IdentityService.Application.Validators;
using IdentityService.Infrastructure.Common;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Authorization.Extensions;
using Shared.Authorization.Middleware;
using Shared.Common.Constants;
using Shared.InterServiceHelper.Extensions;
using Shared.Logging.Configuration;
using Shared.Middleware;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5125, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenLocalhost(5126, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.AddSharedLogging("IdentityService");

String connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in configuration.");

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql =>
            npgsql.MigrationsHistoryTable(
                "__ef_migrations_history_identity",
                SchemaConstants.IDENTITY_SCHEMA)));

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
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

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrganizationRequestDtoValidator).Assembly);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();

builder.Services.AddNotificationGrpcClient(builder.Configuration);
builder.Services.AddMetaDataGrpcClient(builder.Configuration);

builder.Services.AddSharedAuthorization(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddScoped<IRepoWrapper, RepoWrapper>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<IIdentityService, IdentityService.Application.Services.IdentityService>();
builder.Services.AddHostedService<ExpiredOtpCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.MapGrpcService<RbacGrpcService>();
app.MapGrpcService<EmployeeLookupGrpcService>();

using (IServiceScope scope = app.Services.CreateScope())
{
    DataSeeder seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();