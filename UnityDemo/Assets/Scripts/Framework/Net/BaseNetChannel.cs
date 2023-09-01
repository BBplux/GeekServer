﻿using System;
using System.Collections.Concurrent;

public abstract class BaseNetChannel
{
    public virtual string RemoteAddress { get; }
    public long NetId { get; set; }
    public int TargetServerId { get; set; }
    private ConcurrentDictionary<string, object> Datas { get; set; } = new ConcurrentDictionary<string, object>();
    public T GetData<T>(string key)
    {
        if (Datas.TryGetValue(key, out var v))
        {
            return (T)v;
        }
        return default;
    }
    public void SetData(string key, object v)
    {
        Datas[key] = v;
    }
    public virtual void Write(Message msg) => throw new NotImplementedException();
    public virtual void Write(TempNetPackage package) => throw new NotImplementedException();
    public virtual void Close() => throw new NotImplementedException();
    public virtual bool IsClose() => throw new NotImplementedException();

    protected long lastRecvMessageTime;
    public void UpdateRecvMessageTime(long offsetTicks = 0)
    {
        lastRecvMessageTime = DateTime.UtcNow.Ticks + offsetTicks;
    }

    public long GetLastMessageTimeSecond(in DateTime utcTime)
    {
        return (utcTime.Ticks - lastRecvMessageTime) / 10000_000;
    }
}