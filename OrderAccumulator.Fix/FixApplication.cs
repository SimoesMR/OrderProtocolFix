using OrderAccumulator.Application.Interface;
using QuickFix;
using QuickFix.Fields;
using Serilog;

namespace OrderAccumulator.Fix
{
    public class FixApplication : MessageCracker, IApplication
    {
        private readonly IOrderProcessor _orderProcessor;
        private readonly ILogger _logger;

        public FixApplication(IOrderProcessor orderProcessor)
        {
            _orderProcessor = orderProcessor;
            _logger = Log.ForContext<FixApplication>();
        }

        public void OnCreate(SessionID sessionID)
        {
            _logger.Information("Sessão FIX criada: {SessionId}", sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            _logger.Information($"Logon realizado: {sessionID}");
        }

        public void OnLogout(SessionID sessionID)
        {
            _logger.Warning($"Logout realizado: {sessionID}");
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            _logger.Debug("Mensagem enviada para Initiator");
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            _logger.Information("Mensagem recebida do Initiator");

            Crack(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle order, SessionID sessionID)
        {
            var clOrdId = order.ClOrdID.Value;
            var symbol = order.Symbol.Value;
            var side = order.Side.Value;
            var qty = order.OrderQty.Value;
            var price = order.Price.Value;

            _logger.Information(
                "NewOrderSingle recebida: ClOrdID={ClOrdId} Symbol={Symbol} Side={Side} Qty={Qty} Price={Price:N2}",
                clOrdId, symbol, side == '1' ? "BUY" : "SELL", qty, price);

            var response = _orderProcessor.ProcessNewOrder(order);

            _logger.Debug("Enviando ExecutionReport para ClOrdID={ClOrdId}", clOrdId);

            Session.SendToTarget(response, sessionID);
        }
    }
}
