using System;
using System.Collections.Generic;
using System.Text;

namespace Stella.Abstractions
{
    public class StellaHandlerException(int errorCode) : Exception
    {
        public int ErrorCode { get; } = errorCode;
    }
}
