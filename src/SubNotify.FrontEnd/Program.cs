using SubNotify.FrontEnd.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using LSSD.MongoDB;
using SubNotify.Core;
using SubNotify.FrontEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// My notes on how to set up OIDC in .Net 8: https://github.com/MarkStrendin/BlazorDotNet8OIDC

// Add blazor authentication
const string MS_OIDC_SCHEME = "MicrosoftOidc";
builder.Services.AddAuthentication(MS_OIDC_SCHEME)
    .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
    {
        // Pull in configuration, so we can get the OIDC info
        IConfiguration Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();

        oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;        
        oidcOptions.Scope.Add("openid");
        oidcOptions.Scope.Add("profile");
        oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        oidcOptions.GetClaimsFromUserInfoEndpoint = true;
        oidcOptions.Authority = Configuration["OIDC:Authority"];
        oidcOptions.ClientId = Configuration["OIDC:ClientId"];
        oidcOptions.ClientSecret = Configuration["OIDC:ClientSecret"];
        oidcOptions.SaveTokens = true;
        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        oidcOptions.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "groups",
            ValidateIssuer = true
        };
        oidcOptions.Events = new OpenIdConnectEvents
        {
            OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/");
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add services for dependency injection
// example:
//  services.AddScoped<IRegistrationRepository<School>, MongoRepository<School>>();
IConfiguration Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Program>()
                    .Build();

// Main DB connection
builder.Services.AddSingleton<MongoDbConnection>(x => new MongoDbConnection(Configuration.GetConnectionString("Internal")));

// Repositories (For services to consume)
builder.Services.AddSingleton<IRepository<GroupPermission>, MongoRepository<GroupPermission>>();
builder.Services.AddSingleton<IRepository<School>, MongoRepository<School>>();

// Services (For pages to consume)
builder.Services.AddSingleton<GroupPermissionService>();
builder.Services.AddSingleton<SchoolService>();

// Other services
builder.Services.AddSingleton<PermissionsManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);    
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
