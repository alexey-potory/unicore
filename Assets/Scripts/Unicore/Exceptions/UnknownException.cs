using System;
using UnityEngine;

namespace Unicore.Exceptions
{
    internal sealed class UnknownException : Exception
    {
        private const string DefaultMessage = "An unknown error occurred.";

        public UnknownException() : base(DefaultMessage)
        {
            LogWarning();
        }

        public UnknownException(string message) : base(message ?? DefaultMessage)
        {
            LogWarning();
        }

        public UnknownException(string message, Exception innerException) : base(message ?? DefaultMessage,
            innerException)
        {
            LogWarning();
        }

        private static void LogWarning()
        {
#if UNITY_EDITOR
            Debug.LogWarning("Result was in failure state but contained no error.");
#endif
        }
    }
}