using System.Data;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMq.ExcelCreate.Services;

namespace FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqClientService _rabbitMqClientService;

        private readonly IServiceProvider _serviceProvider;

        private IModel _channel;

        public Worker(ILogger<Worker> logger, RabbitMqClientService rabbitMqClientService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMqClientService = rabbitMqClientService;
            _serviceProvider = serviceProvider;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientService.Connect();
            _channel.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMqClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            await Task.Delay(5000);
            var createExcelMessage = JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            await using var memoryStream = new MemoryStream();
            var wb = new XLWorkbook();
            var ds = new DataSet();
            ds.Tables.Add(GetTable("products"));

            wb.Worksheets.Add(ds);
            wb.SaveAs(memoryStream);
            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(memoryStream.ToArray()), "file", Guid.NewGuid().ToString() + ".xlsx");
            var baseUrl = "https://localhost:44335/api/files";
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage!.FileId}", multipartFormDataContent);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"File (Id :{createExcelMessage.FileId} was created by successful");
                _channel.BasicAck(@event.DeliveryTag, false);
            }
        }





        private DataTable GetTable(string tableName)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = _serviceProvider.GetRequiredService<AdventureWorks2019Context>();
            var products = context.Products.ToList();
            var dataTable = new DataTable() { TableName = tableName };

            dataTable.Columns.Add("ProductId", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("ProductNumber", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            products.ForEach(item =>
            {
                dataTable.Rows.Add(item.ProductId, item.Name, item.ProductNumber, item.Color);
            });
            return dataTable;
        }









    }
}