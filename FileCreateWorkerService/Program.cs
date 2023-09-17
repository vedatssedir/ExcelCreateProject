using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddDbContext<AdventureWorks2019Context>(options => options.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer")));
        services.AddSingleton<RabbitMqClientService>();
        services.AddSingleton(_ => new ConnectionFactory()
        {
            Uri = new Uri(hostContext.Configuration.GetConnectionString("RabbitMQ") ?? string.Empty),
            DispatchConsumersAsync = true
        });



    })
    .Build();

host.Run();
