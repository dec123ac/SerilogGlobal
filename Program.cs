using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using WorkerServiceDemo;
using WorkerServiceDemo.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<SourceEnricher>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new OpenApiInfo { Title = "ExDemo", Version = "v1" });
});

#region UseAppSettingsSerilog
/*
var Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .CreateLogger();
*/
//builder.Logging.ClearProviders();
//builder.Logging.AddSerilog(Logger);
#endregion UseAppSettings

var columnOptions = new ColumnOptions
{
    AdditionalColumns = new Collection<SqlColumn>
    {
        new SqlColumn
            {ColumnName = "LOG_DATE", PropertyName = "dateProperty", DataType = SqlDbType.DateTime, AllowNull = false},

        new SqlColumn
            {ColumnName = "LOG", PropertyName = "logProperty", DataType = SqlDbType.VarChar, DataLength = -1},

        new SqlColumn
            {ColumnName = "EXCEPTION", PropertyName = "exProperty", DataType = SqlDbType.VarChar, DataLength = 255},

        new SqlColumn
            {ColumnName = "SOURCE", PropertyName = "srcProperty", DataType = SqlDbType.VarChar, DataLength = 100, AllowNull = false},
    }
};

columnOptions.Store.Remove(StandardColumn.Id);
columnOptions.Store.Remove(StandardColumn.Message);
columnOptions.Store.Remove(StandardColumn.MessageTemplate);
columnOptions.Store.Remove(StandardColumn.Level);
columnOptions.Store.Remove(StandardColumn.TimeStamp);
columnOptions.Store.Remove(StandardColumn.Exception);
columnOptions.Store.Remove(StandardColumn.Properties);
columnOptions.TimeStamp.ColumnName = "LOG_DATE";

var sinkOptions = new MSSqlServerSinkOptions
{
    AutoCreateSqlTable = true,
    TableName = "NewLogs"
};

builder.Services.AddSerilog((serviceProvider, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .Enrich.With<SourceEnricher>()
        .WriteTo.MSSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
            columnOptions: columnOptions,
            sinkOptions: sinkOptions);
});

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
