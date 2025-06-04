using System;
using System.Text;
using System.Linq;
using System.Threading;
using AspNetCoreRateLimit;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using HealthChecks.UI.Client;
using CustomerService.API.Middlewares;
using CustomerService.API.Services.Implementations;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Repositories.Implementations;
using CustomerService.API.Repositories.Interfaces;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.SqlServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CustomerService.API.Delegations;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CustomerService.API.Pipelines.Implementations;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Data.Context;
using MapsterMapper;
using System.Reflection;
using CustomerService.API.Hubs;
using System.Text.Json.Serialization;
using CustomerService.API.WhContext;
using CustomerService.API.Hosted;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Serilog ---------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// --------------------- DbContext ---------------------
builder.Services.AddDbContext<CustomerSupportContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //opts.EnableSensitiveDataLogging();
    //opts.LogTo(Console.WriteLine, LogLevel.Information);

});

// --------------------- CORS ---------------------
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", p =>
    p.WithOrigins(origins!)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()
));


builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile(Path.Combine("WhContext", "messages.json"), optional: false, reloadOnChange: true);

builder.Services.Configure<MessagePrompts>(
    builder.Configuration.GetSection("Prompts")
);

builder.Services.Configure<MessageKeywords>(
    builder.Configuration.GetSection("Keywords")
);

// --------------------- HttpClients ---------------------
// 1) Vincula tu sección "Gemini" a GeminiOptions
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini"));

// 2) Registra el DelegatingHandler que inyecta el header
builder.Services.AddTransient<GeminiDelegatingHandler>();

// 3) Configura el HttpClient para IGeminiClient
//    - BaseAddress = host de Google
//    - El handler añadirá el header x-goog-api-key
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
})
.AddHttpMessageHandler<GeminiDelegatingHandler>();


// --------------------- Jwt Settings ---------------------
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

// --------------------- Repositories & UoW ---------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IRoleMenuRepository, RoleMenuRepository>();
builder.Services.AddScoped<IContactLogRepository, ContactLogRespository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>();
builder.Services.AddScoped<ISystemParamRepository, SystemParamRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --------------------- Application Services ---------------------
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IMessagePipeline, MessagePipeline>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IContactLogService, ContactLogService>();
builder.Services.AddScoped<INicDatetime, NicDatetime>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRecipientService, NotificationRecipientService>();
builder.Services.AddScoped<ISystemParamService, SystemParamService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<InactivityHostedService>();
//builder.Services.AddScoped<IHostedService, ConversationCleanupService>();

// --------------------- Mapster ---------------------
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

builder.Services.AddScoped<IPresenceService, PresenceService>();
//builder.Services.AddHostedService<PresenceBackgroundService>();
builder.Services.AddSingleton<ISignalRNotifyService, SignalRNotifyService>();

// --------------------- Response Caching & Compression ---------------------
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// --------------------- Rate Limiting ---------------------
builder.Services.AddOptions();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --------------------- Health Checks & UI ---------------------
builder.Services.AddHealthChecks()
    .AddApplicationStatus("api_status", tags: new[] { "api" })
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql", "sqlserver" }
    );
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// --------------------- SignalR ---------------------
builder.Services.AddSignalR();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = true;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };

    // Esto permite que el JWT que SignalR manda como ?access_token=…
    // sea reconocido y validado antes de entrar al hub.
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat") ||
                 path.StartsWithSegments("/hubs/notifications") ||
                 path.StartsWithSegments("/hubs/presence")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminPolicy", p => p.RequireRole("Admin"));
    opts.AddPolicy("AgentPolicy", p => p.RequireRole("Support"));
});

// --------------------- Swagger ---------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerSupport.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Usa: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
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

// --------------------- Controllers ---------------------
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
          new JsonStringEnumConverter()
        )
    );



var app = builder.Build();

// --------------------- Middleware Pipeline ---------------------
app.UseSerilogRequestLogging();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

app.UseHttpsRedirection();


app.UseStaticFiles();

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseResponseCompression();
app.UseResponseCaching();
app.UseIpRateLimiting();
app.UseErrorHandling();




app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

app.UseAuthentication();
app.UseAuthorization();


// Liveness
app.MapGet("/liveness", () => Results.Ok(new { status = "alive" }))
   .AllowAnonymous();

// Health endpoints
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI();

// SignalR hub
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHub<PresenceHub>("/hubs/presence");

app.MapControllers();

app.Run();