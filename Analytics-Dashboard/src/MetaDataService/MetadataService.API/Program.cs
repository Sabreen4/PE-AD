using System.Text;
using MetaDataService.Application;
using MetaDataService.Application.Contracts;
using MetaDataService.Application.Mappings;
using MetaDataService.Application.Services;
using MetaDataService.Infrastructure.Common;
using MetaDataService.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Authorization.Extensions;
using Shared.Authorization.Middleware;
using Shared.Common.Constants;
using Shared.Logging.Configuration;
using Shared.Middleware;
using Shared.Redis.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddSharedLogging("MetaDataService");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5290, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenLocalhost(5291, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

String connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in configuration.");

builder.Services.AddDbContext<MetaDataDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql =>
            npgsql.MigrationsHistoryTable(
                "__ef_migrations_history_metadata",
                SchemaConstants.METADATA_SCHEMA)));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
});

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MetaData Service API", Version = "v1" });

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
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddScoped<IRepoWrapper, RepoWrapper>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<IMetadataService, MetaDataService.Application.Services.MetadataService>();

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaData Service API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<MetaDataService.API.Grpc.Services.MetadataGrpcService>();

using (IServiceScope scope = app.Services.CreateScope())
{
    string seedDataPath = Path.Combine(AppContext.BaseDirectory, "SeedData");
    DataSeeder seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync(seedDataPath);
}

app.Run();
