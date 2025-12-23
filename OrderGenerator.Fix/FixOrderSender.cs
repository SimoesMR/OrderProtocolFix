using OrderGenerator.Application.Dto;
using OrderGenerator.Application.Interfaces;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using System.Collections.Concurrent;

namespace OrderGenerator.Fix
{
    public class FixOrderSender : IFixOrderSender
    {
        private readonly FixApplication _fixApp;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<OrderResult>> _pendingOrders = new();

        // Evento para notificar quando um ExecutionReport é recebido
        public event EventHandler<ExecutionReportEventArgs>? ExecutionReportReceived;

        public FixOrderSender(FixApplication fixApp)
        {
            _fixApp = fixApp;
            // Registra o handler para o evento do FixApplication
            _fixApp.OnExecutionReportReceived += HandleExecutionReport;
        }

        public Task<OrderResult> FixOrderSenderAsync(OrderDto dto)
        {
            try
            {
                var clOrdId = Guid.NewGuid().ToString();
                var order = new NewOrderSingle(
                    new ClOrdID(clOrdId),
                    new Symbol(dto.Symbol),
                    new Side(dto.Side == "BUY" ? Side.BUY : Side.SELL),
                    new TransactTime(DateTime.UtcNow),
                    new OrdType(OrdType.LIMIT)
                );

                order.Set(new OrderQty(dto.Quantity));
                order.Set(new Price(dto.Price));
            
                var response = _fixApp.SendOrder(order);
                if (response)
                {
                    // Cria uma TaskCompletionSource para aguardar a resposta do ExecutionReport
                    var tcs = new TaskCompletionSource<OrderResult>();
                    _pendingOrders.TryAdd(clOrdId, tcs);

                    // Define um timeout de 30 segundos
                    _ = Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(_ =>
                    {
                        if (_pendingOrders.TryRemove(clOrdId, out var pendingTcs))
                        {
                            if (!pendingTcs.Task.IsCompleted)
                            {
                                pendingTcs.SetResult(new OrderResult(false, "Timeout aguardando resposta do FIX"));
                            }
                        }
                    });

                    return tcs.Task;
                }
                else
                { 
                    return Task.FromResult(new OrderResult(false, "Falha ao enviar ordem ao FIX"));
                }
            }
            catch (SessionNotFound ex)
            {
                return Task.FromResult(new OrderResult(false, $"Sessão FIX não encontrada: {ex.Message}"));
            }
        }

        private void HandleExecutionReport(object? sender, ExecutionReportEventArgs e)
        {
            var clOrdId = e.ExecutionReport.IsSetClOrdID() ? e.ExecutionReport.ClOrdID.Value : null;

            if (!string.IsNullOrEmpty(clOrdId) && _pendingOrders.TryRemove(clOrdId, out var tcs))
            {
                // Determina o resultado baseado no status da execução
                var execType = e.ExecutionReport.IsSetExecType() ? e.ExecutionReport.ExecType.Value : ' ';
                var isRejected = execType == '8'; // REJECTED
                var message = isRejected && e.ExecutionReport.IsSetText() 
                    ? e.ExecutionReport.Text.Value 
                    : $"Ordem {(isRejected ? "rejeitada" : "processada")}";

                var result = new OrderResult(!isRejected, message);
                tcs.SetResult(result);
            }

            // Dispara o evento para que o front-end seja notificado
            ExecutionReportReceived?.Invoke(this, e);
        }
    }

    // Classe para transportar dados do ExecutionReport via evento
    public class ExecutionReportEventArgs : EventArgs
    {
        public ExecutionReport ExecutionReport { get; set; }
        public SessionID SessionID { get; set; }

        public ExecutionReportEventArgs(ExecutionReport executionReport, SessionID sessionID)
        {
            ExecutionReport = executionReport;
            SessionID = sessionID;
        }
    }
}
