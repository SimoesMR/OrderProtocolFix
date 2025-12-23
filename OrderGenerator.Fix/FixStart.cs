using Microsoft.Extensions.Hosting;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using Serilog;

namespace OrderGenerator.Fix
{
    public class FixStart : IHostedService
    {
        private SocketInitiator _initiator;
        private readonly FixApplication _fixApplication;
        private readonly ILogger _logger;
        public FixStart(FixApplication fixApplication)
        {
            _fixApplication = fixApplication;
            _logger = Log.ForContext<FixStart>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Iniciando conexão FIX...");

            var settings = new SessionSettings("initiator.cfg");
            var storeFactory = new FileStoreFactory(settings);
            var logFactory = new FileLogFactory(settings);
           
            _initiator = new SocketInitiator(
                _fixApplication,
                storeFactory,
                settings,
                logFactory
            );

            _initiator.Start();

            _logger.Information("SocketInitiator iniciado, aguardando conexão...");

            // Aguardar um tempo para a sessão ser estabelecida
            System.Threading.Thread.Sleep(5000);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Encerrando conexão FIX...");
            _initiator?.Stop();
            return Task.CompletedTask;
        }
    }
}
