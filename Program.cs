using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using WorkerServiceDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new OpenApiInfo { Title = "ExDemo", Version = "v1" });
});

var Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Logger);
Serilog.Debugging.SelfLog.Enable(msg =>
{
    Debug.Print(msg);
    Debugger.Break();
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("../swagger/v1/swagger.json", "ExDemo v1"));
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

//app.UseExceptionHandler(
//options =>
//{
//    options.Run(
//    async httpcontext =>
//    {
//        httpcontext.Response.ContentType = "text/html";
//        var ex = httpcontext.Features.Get<IExceptionHandlerFeature>();
//        if (ex != null)
//        {
//            await httpcontext.Response.WriteAsJsonAsync(new { error = ex.Error.Message });

//            Logger.Warning("{@logProperty} {@exProperty} {@srcProperty}",
//                "fromprogramcs", ex.Error.Message, ex.Error.Source);
//        }
//    });
//});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();


//using System;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Serilog;

//namespace WorkerServiceDemo
//{
//    public static class Program
//    {
//        public static void Main(string[] args)
//        {
//            //Serilog.Debugging.SelfLog.Enable(Console.Error);
//            CreateHostBuilder(args).Build().Run();
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureServices((hostContext, services) =>
//                {
//                    services.AddHostedService<Worker>();
//                })
//                .UseSerilog((hostingContext, loggerConfiguration) =>
//                {
//                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
//                });
//    }
//}
