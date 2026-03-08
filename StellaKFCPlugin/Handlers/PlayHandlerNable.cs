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
    public class PlayHandlerNable : StellaHandler
    {
        [StellaHandler("game", "sv7_play_s", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayS()
        {
            
            return new PlaySEResponse();
        }

        [StellaHandler("game", "sv7_play_e", typeof(PlaySERequest))]
        public async Task<PlaySEResponse> PlayE()
        {
            return new PlaySEResponse();
        }
    }
}
