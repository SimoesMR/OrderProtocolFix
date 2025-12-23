using OrderAccumulator.Application.Interface;
using OrderAccumulator.Domain.Entities;
using OrderAccumulator.Domain.Interface;
using OrderAccumulator.Domain.ValueObject;
using QuickFix;
using QuickFix.Fields;


namespace OrderAccumulator.Application.Service
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly IExposureCalculator _exposureCalculator;

        public OrderProcessor(IExposureCalculator exposureCalculator)
        {
            _exposureCalculator = exposureCalculator;
        }

        public Message ProcessNewOrder(Message message)
        {
            try
            {
                // extract data from message to order
                Order order = ExtractOrderFromMessage(message);

                //verify if the limit will exceed the limit
                if (_exposureCalculator.VerifyExceedLimit(order.Symbol.Value, order))
                {
                    Console.WriteLine($"Ordem rejeitada: excede limite de exposição limite R$0,00 a R$ 100.000.000");
                    return CreateRejectedExecutionReport(order,
                        "Ordem rejeitada: excede limite de exposição limite R$0, 00 a R$ 100.000.000");
                }

                //Add order to dictionary
                _exposureCalculator.AddOrder(order);

                //Update exposure
                decimal currentExposure = _exposureCalculator.GetCurrentExposure(order.Symbol.Value);
                Console.WriteLine($"Ordem aceita para {order.Symbol.Value}. " +
                    $"Exposição atual: R$ {currentExposure:N2}" +
                    $"Cl0rdID: {order.ClOrdID}");

                return CreateAcceptedExecutionReport(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar ordem: {ex.Message}");
                return CreateRejectedExecutionExceptionReport(
                    "UNKNOWN",
                    $"Erro no processamento: {ex.Message}.");
            }
        }

        private Order ExtractOrderFromMessage(Message message)
        {
            Domain.ValueObject.Symbol symbol = new Domain.ValueObject.Symbol(message.GetString(Tags.Symbol));
            OrderSideType side = (OrderSideType)message.GetChar(Tags.Side);
            Money money = new Money(message.GetDecimal(Tags.Price));
            int quantity = message.GetInt(Tags.OrderQty);
            string ClOrdID = message.GetString(Tags.ClOrdID);

            return new Order(symbol, side, money, quantity, ClOrdID);
        }

        private Message CreateAcceptedExecutionReport(Order order)
        {
            var execReport = new QuickFix.FIX44.ExecutionReport();

            // Identificadores
            execReport.SetField(new ClOrdID(order.ClOrdID));
            execReport.SetField(new OrderID(Guid.NewGuid().ToString()));
            execReport.SetField(new ExecID(Guid.NewGuid().ToString()));

            // Tipo de execução: New (0)
            execReport.SetField(new ExecType(ExecType.NEW));

            // Status da ordem: New (0)
            execReport.SetField(new OrdStatus(OrdStatus.NEW));

            // Dados da ordem
            execReport.SetField(new QuickFix.Fields.Symbol(order.Symbol.Value));
            execReport.SetField(new Side((char)order.Side));
            execReport.SetField(new Price(order.Price.Value));
            execReport.SetField(new OrderQty(order.Quantity));
            execReport.SetField(new LeavesQty(order.Quantity));
            execReport.SetField(new CumQty(0));

            execReport.SetField(new AvgPx(0)); // Preço médio = 0 (nada executado ainda)

            execReport.SetField(new TransactTime(DateTime.UtcNow));

            return execReport;
        }

        private Message CreateRejectedExecutionReport(Order order, string reason)
        {
            var execReport = new QuickFix.FIX44.ExecutionReport();

            // Identificadores
            execReport.SetField(new ClOrdID(order.ClOrdID));
            execReport.SetField(new OrderID(Guid.NewGuid().ToString()));
            execReport.SetField(new ExecID(Guid.NewGuid().ToString()));

            // Tipo de execução: Rejected (8)
            execReport.SetField(new ExecType(ExecType.REJECTED));

            // Status da ordem: Rejected (8)
            execReport.SetField(new OrdStatus(OrdStatus.REJECTED));

            // Dados da ordem
            if (!string.IsNullOrEmpty(order.Symbol.Value))
                execReport.SetField(new QuickFix.Fields.Symbol(order.Symbol.Value));
            if (order.Side != default)
                execReport.SetField(new Side((char)order.Side));
            if (order.Price.Value > 0)
                execReport.SetField(new Price(order.Price.Value));
            if (order.Quantity > 0)
                execReport.SetField(new OrderQty(order.Quantity));

            execReport.SetField(new LeavesQty(0));
            execReport.SetField(new CumQty(0));

            // ✓ ADICIONE ESTE CAMPO OBRIGATÓRIO:
            execReport.SetField(new AvgPx(0)); // Preço médio = 0

            // Motivo da rejeição
            execReport.SetField(new Text(reason));

            // Timestamp
            execReport.SetField(new TransactTime(DateTime.UtcNow));

            return execReport;
        }

        private Message CreateRejectedExecutionExceptionReport(string clOrdId, string reason)
        {
            var execReport = new QuickFix.FIX44.ExecutionReport();

            // Identificadores
            execReport.SetField(new ClOrdID(clOrdId));
            execReport.SetField(new OrderID(""));
            execReport.SetField(new ExecID(""));

            // Tipo de execução: Rejected (8)
            execReport.SetField(new ExecType(ExecType.REJECTED));
            execReport.SetField(new OrdStatus(OrdStatus.REJECTED));

            // Campos obrigatórios com valores default
            execReport.SetField(new LeavesQty(0));
            execReport.SetField(new CumQty(0));
            execReport.SetField(new AvgPx(0));

            // Motivo da rejeição
            execReport.SetField(new Text(reason));

            // Timestamp
            execReport.SetField(new TransactTime(DateTime.UtcNow));

            return execReport;
        }
    }
}
