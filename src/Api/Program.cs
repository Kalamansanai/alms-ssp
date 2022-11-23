using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using Api;
using Api.Endpoints.Detectors;
using Api.RequestBinders;
using Application.Interfaces;
using Application.Services;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.Database;
using Infrastructure.Logging;
using Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();
builder.Services.Configure<HostOptions>(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(1));
builder.Services.AddDatabase(builder.Configuration);

// TODO(rg): into extension method
builder.Services.AddScoped<ICHNameUniquenessChecker<Site>, SiteNameUniquenessChecker>();
builder.Services.AddScoped(typeof(ICHNameUniquenessChecker<,>), typeof(CHNameUniquenessChecker<,>));

// TODO(rg): into extension method
builder.Services.AddScoped<IDetectorConnection, DetectorHttpConnection>();
builder.Services.AddSingleton<IDetectorStreamCollection, DetectorStreamCollection>();

builder.Services.AddHttpClient();
builder.Services.AddAuthorization();

// TODO(rg): into extension method
builder.Services.AddSingleton(typeof(IRequestBinder<Command.Req>), typeof(CommandReqBinder));
builder.Services.AddFastEndpoints();

builder.Services.AddSwaggerDoc(s =>
{
    s.TypeNameGenerator = new ShorterTypeNameGenerator();
    s.SerializerSettings.Converters.Add(new StringEnumConverter());
    s.GenerateEnumMappingDescription = true;
    s.DocumentName = "v1";
});
builder.Services.AddSignalR();
builder.Services.AddCors();

var jwtOptions = new JwtOptions();
builder.Configuration.GetSection(JwtOptions.ConfigurationEntryName).Bind(jwtOptions);

builder.Services.AddIdentityCore<User>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<ApplicationRole>()
    //nemtom erre mi az új solution, edit: ig ez
    .AddEntityFrameworkStores<Context>();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var tokenValidationParameters = new TokenValidationParameters
{
    ValidIssuer = jwtOptions.ValidIssuer,
    ValidAudience = jwtOptions.ValidAudience,
    NameClaimType = JwtRegisteredClaimNames.Sub,
    RoleClaimType = jwtOptions.RoleClaimName,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
};

builder.Services.AddAuthentication(opt =>
    {
        // TODO(rg): not all of these are needed
        //ez a todo örökké itt lesz
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, "DefaultScheme", opt =>
    {
        opt.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.Configure<DefaultUserOptions>(builder.Configuration.GetSection(DefaultUserOptions.ConfigurationEntryName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.ConfigurationEntryName));
builder.Services.AddSingleton<IOptions<TokenValidationParameters>>(new OptionsWrapper<TokenValidationParameters>(tokenValidationParameters));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabase();
    app.UseOpenApi();
    app.UseSwaggerUi3(c => { c.ConfigureDefaults(); });
}
app.UseCors(options =>
{
    options.AllowAnyMethod();
    options.AllowAnyHeader();
    // NOTE(rg): workaround; the JS SignalR requires credentials to be allowed,
    // but AllowAnyOrigin and AllowCredentials can't be used together
    options.SetIsOriginAllowed(_ => true);
    // options.AllowAnyOrigin();
    options.AllowCredentials();
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseFastEndpoints(options =>
{
    options.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    options.Endpoints.Configurator = o =>
    {
        o.DontAutoTag();
        o.DontCatchExceptions();
    };
    options.Endpoints.RoutePrefix = "api/v1";
});

app.Run();
