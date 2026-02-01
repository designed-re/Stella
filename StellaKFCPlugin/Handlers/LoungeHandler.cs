using CorePlugin.EF;
using CorePlugin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Stella.Abstractions;

namespace StellaKFCPlugin.Handlers
{
    public class LoungeHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_lounge", typeof(LoungeRequest))]
        public async Task<LoungeResponse> Lounge()
        {
            
            return new LoungeResponse(){Interval = 30};
        }
    }
}
