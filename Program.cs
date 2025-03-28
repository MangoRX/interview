using System.Reflection;
using System.Security.Claims;
using System.Text;

using Aspnet.React.Data;
using Aspnet.React.Mapping;
using Aspnet.React.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Serilog;

using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
AuthOptions.KEY = builder.Configuration["JWT:Key"];

// Add services to the container.

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning);
    lc.WriteTo.Console();
});

builder.Services.AddAutoMapper(typeof(AppMappingProfile));

builder.Services.AddDbContext<ApplicationDbContext>(c => c.UseSqlite("Filename=DataBase.db"));

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});


builder.Services.AddSwaggerGen(c =>
{
    // c.DocumentFilter<CustomModelDocumentFilter<Model>>();

    c.MapType<Object>(() => new OpenApiSchema { Type = "object" });

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
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
            new string[] { }
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequiredLength = 8;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = true;
    opts.Password.RequireDigit = true;
});


// Adding Authentication
var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

// Adding Jwt Bearer
authenticationBuilder.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Headers.ContainsKey("Sec-WebSocket-Protocol") &&
                context.HttpContext.WebSockets.IsWebSocketRequest)
            {
                var header = context.Request.Headers["Sec-WebSocket-Protocol"].ToString();
                var newHeader = header;
                foreach (var s in header.Split(','))
                {
                    if (s.Trim().StartsWith("Bearer-"))
                    {
                        context.Token = s.Trim().Substring(7);
                        newHeader = String.Join(", ", newHeader.Split(",").Where(x => x != s).ToArray());
                        break;
                    }
                }

                context.Request.Headers["Sec-WebSocket-Protocol"] = newHeader;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var name = context.Principal.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value;
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(name);

            if (user == null)
            {
                context.Fail("Invalid token");
            }
            else
            {
                context.HttpContext.Items["User"] = user;
            }
        }
    };
});

// Google OAuth example
// authenticationBuilder.AddOAuth<GoogleOptions, ReplacedGoogleHandler>(GoogleDefaults.AuthenticationScheme, GoogleDefaults.DisplayName, googleOptions =>
// {
//     googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//     googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//     googleOptions.CallbackPath = "/oauth/google";
//
//     googleOptions.CorrelationCookie.Path = "/";
//             
//     googleOptions.Events.OnTicketReceived += ctx => AuthUtils.OnTicketReceived(ctx);
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.UseAuthentication();
app.UseAuthorization();

app.Run();

public class CustomModelDocumentFilter<T> : IDocumentFilter where T : class
{
    public void Apply(OpenApiDocument openapiDoc, DocumentFilterContext context)
    {
        context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
    }
}

public class AuthOptions
{
    public const string ISSUER = "$rootnamespace$";
    public const string AUDIENCE = "$rootnamespace$.Client";
    public static string KEY = "";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}