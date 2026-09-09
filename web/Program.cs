using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Authorization;

using Radzen;
using WebApp;
using WebApp.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("ASMPT_API", client => 
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"])
    ).AddHttpMessageHandler(sp => 
        sp.GetRequiredService<AuthorizationMessageHandler>()
          .ConfigureHandler(
              authorizedUrls: new[] { builder.Configuration["ApiSettings:BaseUrl"] },
              scopes: builder.Configuration.GetSection("ApiSettings:Scopes").Get<string[]>()
            )
        );

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, CustomUserAccount>(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes.Add("api://43353f78-34d4-4a75-86aa-07b8b11eaf8a/user_access");
        options.ProviderOptions.LoginMode = "redirect";
    }).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount, CustomAccountFactory>();

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IExceptionHandler, ExceptionHandler>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ASMPT_API"));

builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
