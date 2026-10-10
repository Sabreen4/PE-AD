using System.Text;
using EvaluationService.Application;
using EvaluationService.Application.Contracts;
using EvaluationService.Application.Mappings;
using EvaluationService.Infrastructure.Services;
using EvaluationService.Application.Validators;
using EvaluationService.Infrastructure.Common;
using EvaluationService.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Shared.Authorization.Extensions;
using Shared.InterServiceHelper.Extensions;
using Shared.InterServiceHelper.Grpc.Identity;
using Shared.InterServiceHelper.Grpc.Program;
using Shared.Authorization.Middleware;
using Shared.Common.Constants;
using Shared.Logging.Configuration;
using Shared.Middleware;
using GeneratedProgramGrpcService = Shared.InterServiceHelper.Grpc.Program.ProgramGrpcService;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5064, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });

    options.ListenLocalhost(5065, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

builder.AddSharedLogging("EvaluationService");

String connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in configuration.");

builder.Services.AddDbContext<EvaluationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsql =>
            npgsql.MigrationsHistoryTable(
                "__ef_migrations_history_evaluation",
                SchemaConstants.EVALUATION_SCHEMA)));

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Evaluation Service API", Version = "v1" });

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
builder.Services.AddValidatorsFromAssembly(typeof(CreateAttendanceValidator).Assembly);
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(EvaluationService.Application.Features.Attendance.Commands.CreateAttendanceCommand).Assembly);
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddMetaDataGrpcClient(builder.Configuration);
builder.Services.AddNotificationGrpcClient(builder.Configuration);

builder.Services.AddGrpcClient<GeneratedProgramGrpcService.ProgramGrpcServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["ServiceUrls:ProgramServiceGrpc"]
        ?? throw new InvalidOperationException("Missing ServiceUrls:ProgramServiceGrpc in configuration."));
});
builder.Services.AddScoped<IProgramLookupServiceClient, ProgramLookupServiceClient>();

builder.Services.AddGrpcClient<IdentityLookupService.IdentityLookupServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["ServiceUrls:IdentityServiceGrpc"]
        ?? throw new InvalidOperationException("Missing ServiceUrls:IdentityServiceGrpc in configuration."));
});
builder.Services.AddScoped<IIdentityLookupServiceClient, IdentityLookupServiceClient>();

builder.Services.AddScoped<EvaluationService.Application.Contracts.IRepoWrapper, EvaluationService.Infrastructure.Common.RepoWrapper>();
builder.Services.AddScoped<EvaluationService.Application.Contracts.IEvaluationService, EvaluationService.Application.Services.EvaluationService>();

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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Evaluation Service API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
