using RabbitMQ.Client;
namespace FileCreateWorkerService.Services
{
    public class RabbitMqClientService :IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMqClientService> _logger;

        public static string QueueName = "queue-excel-file";
        public RabbitMqClientService(ConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }



        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }
            _channel = _connection.CreateModel();
            _logger.LogInformation("RabbitMq ile baglantı kuruldu");
            return _channel;
        }

        public void Dispose()
        {
            _channel.Close();
            _channel.Dispose();
            _connection.Close();
            _connection.Dispose();
            _logger.LogInformation("RabbitMQ ile baglantı kesildi...");

        }
    }
}
