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

namespace StellaKFCPlugin.Handlers
{
    public class FrozenHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_frozen", typeof(FrozenRequest))]
        public async Task<FrozenResponse> Frozen()
        {
            var request = Request as FrozenRequest;
            var context = new StellaKFCContext();

            return new FrozenResponse();
        }
    }
}
