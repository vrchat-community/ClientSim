using System;

namespace VRC.SDK3.ClientSim
{
    /// <summary>
    /// A wrapper for exceptions that can easily be checked in Tests.
    /// </summary>
    public class ClientSimException : Exception
    {
        public ClientSimException(string message) : base(message) { }
    }
}