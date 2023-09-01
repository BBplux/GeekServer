﻿using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Buffers.Binary;
using Base;
using UnityEngine;

public class KcpUdpClientSocket : AKcpSocket
{
    UdpClient socket;
    Action onGateClose;
    Action onServerClose;
    public KcpUdpClientSocket(int serverId)
    {
        this.ServerId = serverId;
    }

    public override async Task<ConnectResult> Connect(string ip, int port, long netId = 0)
    {
        isConnecting = true;
        try
        {
            socket = new UdpClient(ip, port);
            //socket.ExclusiveAddressUse = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return new(false, true, false);
        }
        this.NetId = netId;
        //serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        var data = new byte[TempNetPackage.headLen];
        data.Write(NetPackageFlag.SYN, 0);
        data.Write(NetId, 1);
        data.Write(ServerId, 9);
        //Debug.Log($"开始udp连接....{NetId}");  
        socket.Send(data, data.Length);
        try
        {
            var task = socket.ReceiveAsync();
            if (task == await Task.WhenAny(task, Task.Delay(400)))
            {
                var buffer = task.Result.Buffer;
                if (buffer.Length >= TempNetPackage.headLen)
                {
                    var flag = buffer[0];
                    NetId = buffer.ReadLong(1);
                    var serId = buffer.ReadInt(9);
                    Debug.Log($"收到连接包:{flag}");
                    if (flag == NetPackageFlag.ACK)
                    {
                        Debug.Log($"连接成功..");
                        return new(true, true, false);
                    }
                    if (flag == NetPackageFlag.NO_GATE_CONNECT)
                    {
                        Close();
                        return new(false, true, false);
                    }
                    if (flag == NetPackageFlag.NO_INNER_SERVER) //不能发现服务器
                    {
                        Close();
                        return new(false, false, true);
                    }
                    if (flag == NetPackageFlag.CLOSE) //服务器已关闭连接
                    {
                        return new(false, true, true);
                    }
                }
            }
            else
            {
                Close();
                Debug.Log("接收udp消息失败....");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return new(false, true, false);
        }
        finally
        {
            isConnecting = false;
        }
        Close();
        return new(false, true, false);
    }

    public override void Close()
    {
        lock (this)
        {
            base.Close();
            cancelSrc.Cancel();
            socket?.Close();
            socket = null;
        }
    }

    public override bool IsClose()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return true;
        }
#endif
        return !isConnecting && socket == null;
    }

    readonly byte[] sendBuffer = new byte[2000];
    public override void Send(TempNetPackage package)
    {
        if (socket == null)
            return;
        var target = new Span<byte>(sendBuffer);
        target[0] = package.flag;
        int offset = 1;
        target.Write(package.netId, ref offset);
        target.Write(package.innerServerId, ref offset);
        if (!package.body.IsEmpty)
        {
            package.body.CopyTo(target.Slice(TempNetPackage.headLen));
        }
        socket.Send(sendBuffer, package.Length);
    }


    public override async Task StartRecv(OnReceiveNetPackFunc onRecv, Action onGateCloseAct, Action onServerCloseAct)
    {
        this.onGateClose = onGateCloseAct;
        this.onServerClose = onServerCloseAct;

        _ = StartGateHeartAsync();

        void onRecvUdpData(byte[] data)
        {
            var package = new TempNetPackage(data);
            if (package.netId != NetId)
                return;
            switch (package.flag)
            {
                case NetPackageFlag.NO_GATE_CONNECT:
                    Debug.Log("gate 断开连接...");
                    onGateClose?.Invoke();
                    Close();
                    break;
                case NetPackageFlag.CLOSE:
                case NetPackageFlag.NO_INNER_SERVER:
                    Debug.Log("server 断开连接...");
                    onServerClose?.Invoke();
                    Close();
                    break;
                case NetPackageFlag.HEART:
                    if (package.body.Length > 0)
                    {
                        var id = BinaryPrimitives.ReadInt32BigEndian(package.body);
                        Debug.Log($"收到心跳回复包...{id}");
                        EndWaitHeartId(id);
                    }
                    break;
                case NetPackageFlag.MSG:
                    onRecv?.Invoke(package.body);
                    break;
            }
        }


        await Task.Delay(1);

        while (!cancelSrc.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveAsync();
                // Debuger.Log($"收到udp数据：{result.Buffer.Length}");
                var buffer = result.Buffer;
                if (buffer.Length >= TempNetPackage.headLen)
                {
                    onRecvUdpData(buffer);
                }
            }
            catch (Exception e)
            {
                Close();
                onGateClose?.Invoke();
                break;
            }
        }
    }
}