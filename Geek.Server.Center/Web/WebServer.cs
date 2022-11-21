﻿using Geek.Server.Center.Common;
using Geek.Server.Center.Logic;
using Geek.Server.Center.Web.Data;
using Geek.Server.Center.Web.Service;
using MagicOnion;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;

namespace Geek.Server.Center.Web
{
    public static class WebServer
    {
        static WebApplication app;
        public static Task Start(string webUrl)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                WebRootPath = "Web/wwwroot",
            });

            StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

            // Add services to the container.
            builder.Services.AddRazorPages().WithRazorPagesRoot("/Web/Pages");
            builder.Services.AddServerSideBlazor();
            builder.Services.AddHttpContextAccessor(); //用于获得客户端信息
            builder.Services.AddSingleton<DBService>();
            builder.Services.AddSingleton<LoginService>();
            builder.Services.AddSingleton<ConfigService>();
            builder.Services.AddSingleton<NamingService>();
            //builder.Services.AddSingleton<ServerNodeService>();
            builder.Services.AddScoped<ProtectedSessionStorage>();
            builder.Services.AddScoped<ProtectedLocalStorage>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddMudServices();

            var app = builder.Build();

            var provider = app.Services;
            //如果没有默认用户,初始化默认用户
            var dbService = provider.GetRequiredService<DBService>();
            var adminName = Settings.InsAs<CenterSetting>().InitUserName;

            if (dbService.GetData<UserInfo>(adminName) == null)
            {
                dbService.UpdateData<UserInfo>(adminName, new UserInfo { Name = adminName, Password = Settings.InsAs<CenterSetting>().InitPassword });
            }

            ServiceManager.ConfigService = provider.GetRequiredService<ConfigService>();
            ServiceManager.NamingService = provider.GetRequiredService<NamingService>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            //app.Urls.Clear();
            //app.Urls.Add(webUrl);

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();

            app.MapFallbackToPage("/_Host");

            return app.StartAsync();
        }

        public static Task Stop()
        {
            return app.StopAsync();
        }
    }
}
