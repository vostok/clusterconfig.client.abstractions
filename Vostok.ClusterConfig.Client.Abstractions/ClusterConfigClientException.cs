using System;
using JetBrains.Annotations;

namespace Vostok.ClusterConfig.Client.Abstractions
{
    /// <summary>
    /// Represents an exception produced by <see cref="IClusterConfigClient"/> implementations.
    /// </summary>
    [PublicAPI]
    public class ClusterConfigClientException : Exception
    {
        public ClusterConfigClientException(string message)
            : base(message)
        {
        }

        public ClusterConfigClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}