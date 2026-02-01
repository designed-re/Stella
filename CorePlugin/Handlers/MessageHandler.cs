using CorePlugin.Models;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class MessageHandler : StellaHandler
    {
        [StellaHandler("message", "get", typeof(GetMessageRequest))]
        public async Task<GetMessageResponse> GetMessage()
        {
            return new GetMessageResponse
            {
                Expire = 600,
                Items = (PluginConfig as CorePluginConfig).MaintenanceMode ? new List<MessageItem> //if true, return maintenance messages
                {
                    new MessageItem
                    {
                        Start = 0,
                        End = 86400,
                        Name = "sys.mainte"
                    },
                    new MessageItem
                    {
                        Start = 0,
                        End = 86400,
                        Name = "sys.eacoin.mainte"
                    }
                } : []
            };
        }
    }
}
