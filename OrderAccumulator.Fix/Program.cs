using OrderAccumulator.Application.Service;
using OrderAccumulator.Domain.Interface;
using OrderAccumulator.Fix;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/orderaccumulator-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Iniciando OrderAccumulator...");

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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal no OrderAccumulator!");
    throw;
}
finally 
{
    Log.Information("Finalizando logs...");
    Log.CloseAndFlush(); 
}