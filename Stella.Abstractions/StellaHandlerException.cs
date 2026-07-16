using System;
using System.Collections.Generic;
using System.Text;

namespace Stella.Abstractions
{
    public class StellaHandlerException(int errorCode) : Exception
    {
        /// <summary>Generic bad/malformed request error code.</summary>
        public const int BadRequestCode = 400;

        /// <summary>Generic internal server error code.</summary>
        public const int InternalErrorCode = 500;

        public int ErrorCode { get; } = errorCode;
    }
}
