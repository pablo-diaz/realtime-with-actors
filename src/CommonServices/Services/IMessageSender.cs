using System.Threading.Tasks;

namespace Services
{
    public interface IMessageSender
    {
        Task SendMessage(object message);
    }
}
