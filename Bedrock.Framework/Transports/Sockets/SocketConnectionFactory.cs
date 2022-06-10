﻿using System.Net;
using Microsoft.AspNetCore.Connections;

namespace Bedrock.Framework
{
    public class SocketConnectionFactory : IConnectionFactory
    {
        public static SocketConnectionFactory Factory { get; private set; } = new SocketConnectionFactory();

        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            return new SocketConnection(endpoint).StartAsync();
        }
    }
}
