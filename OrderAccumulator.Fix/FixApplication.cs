using OrderAccumulator.Application.Interface;
using QuickFix;
using QuickFix.Fields;

namespace OrderAccumulator.Fix
{
    public class FixApplication : MessageCracker, IApplication
    {
        private readonly IOrderProcessor _orderProcessor;

        public FixApplication(IOrderProcessor orderProcessor)
        {
            _orderProcessor = orderProcessor;
        }

        public void OnCreate(SessionID sessionID)
        {
            Console.WriteLine($"Sessão criada: {sessionID}");
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine($"Logon realizado: {sessionID}");
        }

        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine($"Logout realizado: {sessionID}");
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
        }

        public void ToApp(Message message, SessionID sessionID)
        {
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine($"Recebido da Web Order");

             Crack(message, sessionID);
        }

        public void OnMessage(QuickFix.FIX44.NewOrderSingle order, SessionID sessionID)
        {
            var response = _orderProcessor.ProcessNewOrder(order);
                
            Session.SendToTarget(response, sessionID);
        }
    }
}
