using System;

namespace Stella.Abstractions.Plugins
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StellaHandlerAttribute : Attribute
    {
        public string Service { get; }
        public string Module { get; }

        public Type RequestType { get; set; }

        public StellaHandlerAttribute(string service, string module, Type? requestType)
        {
            Service = service;
            Module = module;
            RequestType = requestType ?? typeof(object);
        }
    }
}
