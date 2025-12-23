using OrderAccumulator.Application.Service;
using OrderAccumulator.Domain.Interface;
using OrderAccumulator.Fix;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

SessionSettings settings = new SessionSettings("acceptor.cfg");
var exposureCalculator = new ExposureCalculator();
var orderProcessor = new OrderProcessor(exposureCalculator);
IApplication app = new FixApplication(orderProcessor);
IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
ILogFactory logFactory = new FileLogFactory(settings);
ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);

acceptor.Start();

Console.WriteLine("Acceptor rodando. Pressione <ENTER> para parar.");
Console.ReadLine();

acceptor.Stop();
Console.WriteLine("Acceptor parado.");