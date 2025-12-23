using Microsoft.Extensions.Hosting;
using QuickFix;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace OrderGenerator.Fix
{
    public class FixStart : IHostedService
    {
        private SocketInitiator _initiator;
        private readonly FixApplication _fixApplication;
        public FixStart(FixApplication fixApplication)
        {
            _fixApplication = fixApplication;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
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
            
            // Aguardar um tempo para a sessão ser estabelecida
            System.Threading.Thread.Sleep(5000);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _initiator?.Stop();
            return Task.CompletedTask;
        }
    }
}
