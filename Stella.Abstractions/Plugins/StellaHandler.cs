using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Stella.Abstractions.Plugins
{
    public class StellaHandler
    {
        public IStellaEAmuseRequest? Request { get; set; } = null;

        public string? Model { get; set; }

        public string? PCBId { get; set; }

        public IStellaPluginConfig PluginConfig { get; set; }

        public ILogger Logger { get; set; }

        public HttpContext HttpContext { get; set; }
    }
}

