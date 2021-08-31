using System;

namespace Wrapr
{
    public class WraprException : Exception
    {
        public WraprException(string message) : base(message)
        {
        }
    }
}