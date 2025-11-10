using System.Net.Http.Headers;
using AIDIMS.Application;
using AIDIMS.Infrastructure;
using AIDIMS.API.Middleware;
using AIDIMS.API.Filters;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add health check
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
        name: "PostgreSQL",
        failureStatus: HealthStatus.Unhealthy,
        timeout: TimeSpan.FromSeconds(5)
    );

var orthancConfig = builder.Configuration.GetSection("Orthanc");
var orthancBaseUrl = orthancConfig["BaseUrl"];
var orthancUser = orthancConfig["Username"];
var orthancPass = orthancConfig["Password"];

var basicAuthToken = Convert.ToBase64String(
    Encoding.UTF8.GetBytes($"{orthancUser}:{orthancPass}")
);

builder.Services.AddHttpClient("OrthancClient", client =>
{
    client.BaseAddress = new Uri(orthancBaseUrl);

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", basicAuthToken);
});

// Add Application layer
builder.Services.AddApplication();

// Add Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is not configured");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    // Admin only policy
    options.AddPolicy(PolicyNames.AdminOnly, policy =>
        policy.Requirements.Add(new AdminRequirement()));

    // Doctor only policy
    options.AddPolicy(PolicyNames.DoctorOnly, policy =>
        policy.Requirements.Add(new DoctorRequirement()));

    // Technician only policy
    options.AddPolicy(PolicyNames.TechnicianOnly, policy =>
        policy.Requirements.Add(new TechnicianRequirement()));

    // Receptionist only policy
    options.AddPolicy(PolicyNames.ReceptionistOnly, policy =>
        policy.Requirements.Add(new ReceptionistRequirement()));

    // Medical staff only policy (Doctor or Technician)
    options.AddPolicy(PolicyNames.MedicalStaffOnly, policy =>
        policy.Requirements.Add(new MedicalStaffRequirement()));

    // Admin or Doctor policy
    options.AddPolicy(PolicyNames.AdminOrDoctor, policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role &&
                (c.Value == "Admin" || c.Value == "Doctor"))));

    // Admin or Technician policy
    options.AddPolicy(PolicyNames.AdminOrTechnician, policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role &&
                (c.Value == "Admin" || c.Value == "Technician"))));
});

// Register authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, AdminRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DoctorRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, TechnicianRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ReceptionistRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, MedicalStaffRequirementHandler>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AIDIMS API",
        Version = "v1",
        Description = "AIDIMS - AI Diagnostic Imaging Management System API"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIDIMS API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = "Healthy"
        }));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            details = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapGet("/", () => "Server is running on port 5104").AllowAnonymous();

app.Run();
