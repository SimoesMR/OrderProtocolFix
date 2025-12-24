using QuickFix;
using QuickFix.FIX44;
using Serilog;

namespace OrderGenerator.Fix
{
    public class FixApplication : MessageCracker, IApplication
    {
        private SessionID _sessionId;
        private readonly ILogger _logger;

        // Evento para notificar quando um ExecutionReport é recebido
        public event EventHandler<OrderGenerator.Fix.ExecutionReportEventArgs>? OnExecutionReportReceived;

        public FixApplication() 
        {
            _logger = Log.ForContext<FixApplication>();
        }

        public void OnCreate(SessionID sessionID)
        {
            _sessionId = sessionID;
            _logger.Information("Sessão FIX criada: {SessionId}", sessionID);
        }

        //Quando loga(faz a conexao) com o acceptor
        public void OnLogon(SessionID sessionID)
        {
            _logger.Information("FIX Logon: {SessionId}", sessionID);
        }

        //Quando logout(desconecta) com o acceptor
        public void OnLogout(SessionID sessionID)
            => _logger.Warning("FIX Logout: {SessionId}", sessionID);

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
        }
        public void FromAdmin(QuickFix.Message message, SessionID sessionID) {
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID) {
            _logger.Debug($"Mensagem enviada para Acceptor: {message}");
        }
        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            _logger.Information($"Recebido ordem do console order");
            Crack(message, sessionID);
        }   

        public bool SendOrder(NewOrderSingle order)
        {
            if (_sessionId == null)
            {
                _logger.Error("Sessão FIX não iniciada!");
                throw new InvalidOperationException("Sessão FIX não iniciada");
            }

            _logger.Information("Enviando ordem: ClOrdID={ClOrdId}", order.ClOrdID.Value);

            var responde = Session.SendToTarget(order, _sessionId);
            return responde;
        }

        public void OnMessage(QuickFix.FIX44.ExecutionReport execReport, SessionID sessionID)
        {
            _logger.Information($"Recebido ordem do console order");
    
            // Identificadores
            var clOrdId = execReport.IsSetClOrdID() ? execReport.ClOrdID.Value : "N/A";
 
            // Status
            var execType = execReport.IsSetExecType() ? execReport.ExecType.Value : ' ';
            var ordStatus = execReport.IsSetOrdStatus() ? execReport.OrdStatus.Value : ' ';

            // Dados da ordem
            var symbol = execReport.IsSetSymbol() ? execReport.Symbol.Value : "N/A";
            var side = execReport.IsSetSide() ? GetSideDescription(execReport.Side.Value) : "N/A";
            var price = execReport.IsSetPrice() ? execReport.Price.Value : 0;
            var qty = execReport.IsSetOrderQty() ? execReport.OrderQty.Value : 0;
            var leavesQty = execReport.IsSetLeavesQty() ? execReport.LeavesQty.Value : 0;
            var cumQty = execReport.IsSetCumQty() ? execReport.CumQty.Value : 0;

            _logger.Information(
               "ExecutionReport: ClOrdID={ClOrdId} Symbol={Symbol} Side={Side} Price={Price} OrderQty={OrderQty} " +
               "CumQty={CumQty} LeavesQty={LeavesQty} ExecType={ExecType} OrdStatus={OrdStatus}",
               clOrdId, symbol, side, price, qty, cumQty, leavesQty, 
               GetExecTypeDescription(execType), GetOrdStatusDescription(ordStatus));

            OnExecutionReportReceived?.Invoke(this, new OrderGenerator.Fix.ExecutionReportEventArgs(execReport, sessionID));
        }

        private string GetExecTypeDescription(char execType)
        {
            return execType switch
            {
                '0' => "NEW (Nova)",
                '1' => "PARTIAL_FILL (Parcial)",
                '2' => "FILL (Executada)",
                '3' => "DONE_FOR_DAY",
                '4' => "CANCELED (Cancelada)",
                '5' => "REPLACED (Substituída)",
                '6' => "PENDING_CANCEL",
                '7' => "STOPPED",
                '8' => "REJECTED (Rejeitada)",
                '9' => "SUSPENDED",
                'A' => "PENDING_NEW",
                'B' => "CALCULATED",
                'C' => "EXPIRED (Expirada)",
                'D' => "RESTATED",
                'E' => "PENDING_REPLACE",
                _ => $"UNKNOWN ({execType})"
            };
        }

        private string GetOrdStatusDescription(char ordStatus)
        {
            return ordStatus switch
            {
                '0' => "NEW (Nova)",
                '1' => "PARTIALLY_FILLED (Parcial)",
                '2' => "FILLED (Executada)",
                '3' => "DONE_FOR_DAY",
                '4' => "CANCELED (Cancelada)",
                '5' => "REPLACED",
                '6' => "PENDING_CANCEL",
                '7' => "STOPPED",
                '8' => "REJECTED (Rejeitada)",
                '9' => "SUSPENDED",
                'A' => "PENDING_NEW",
                'B' => "CALCULATED",
                'C' => "EXPIRED (Expirada)",
                'D' => "ACCEPTED_FOR_BIDDING",
                'E' => "PENDING_REPLACE",
                _ => $"UNKNOWN ({ordStatus})"
            };
        }

        private string GetSideDescription(char side)
        {
            return side switch
            {
                '1' => "BUY (Compra)",
                '2' => "SELL (Venda)",
                _ => $"UNKNOWN ({side})"
            };
        }
    }
}
