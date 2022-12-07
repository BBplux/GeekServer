﻿using Geek.Server.Core.Center;
using Geek.Server.Core.Net.Tcp;
using Geek.Server.Gateway.Common;
using Geek.Server.Gateway.Net.Rpc;
using Geek.Server.Gateway.Net.Tcp.Inner;
using Geek.Server.Gateway.Net.Tcp.Outer;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Net
{
    internal class GateNetMgr
    {
        public static Connections ClientConns { get; private set; } = new Connections();

        public static Connections ServerConns { get; private set; } = new Connections();

        public static GateCenterRpcClient CenterRpcClient { get; set; }

        public static Task<bool> ConnectCenter()
        {
            CenterRpcClient = new GateCenterRpcClient(Settings.CenterUrl);
            return CenterRpcClient.Connect();
        }


        private static TcpServer outerTcpServer;
        private static TcpServer innerTcpServer;
        public static async Task StartTcpServer()
        {
            outerTcpServer = new TcpServer();
            await outerTcpServer.Start(Settings.TcpPort, builder => builder.UseConnectionHandler<OuterTcpConnectionHandler>());
            innerTcpServer = new TcpServer();
            await innerTcpServer.Start(Settings.InsAs<GateSettings>().InnerTcpPort, builder => builder.UseConnectionHandler<InnerTcpConnectionHandler>());
        }

        public static async Task Stop()
        {
            if (outerTcpServer != null)
                await outerTcpServer.Stop();
            if (innerTcpServer != null)
                await innerTcpServer.Stop();
            if (CenterRpcClient != null)
                await CenterRpcClient.Stop();
        }

        public static long SelectAHealthNode(int serverId)
        {
            //TODO:分布式大服结构中网络节点会存在多个
            //TODO:选择一个负载最小的节点
            var conn = ServerConns.GetByNodeId(serverId);
            if (conn != null)
                return conn.NodeId;
            return -1;
        }

        public static int GetConnectionCount()
        {
            return ClientConns.GetConnectionCount();
        }

    }
}
