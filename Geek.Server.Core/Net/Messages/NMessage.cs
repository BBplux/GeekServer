﻿using System.Buffers;

namespace Geek.Server
{
    /// <summary>
    /// net message
    /// </summary>
    public struct NMessage
    {

        public ReadOnlySequence<byte> Payload { get; } = default;

        public NMessage(ReadOnlySequence<byte> payload)
        {
            Payload = payload;
        }


        public NMessage(int msgId, byte[] msgData)
        {
            this.MsgId = msgId;
            MsgRaw = msgData;
        }

        public int MsgId = 0;
        public byte[] MsgRaw = null;
        public long TargetId = 0;

        public Message Msg { get; } = null;

        public NMessage(Message msg)
        {
            Msg = msg;
            MsgId = msg.MsgId;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            MessagePack.MessagePackSerializer.Serialize(writer, Msg);
        }

        public byte[] Serialize()
        {
            try
            {
                return MessagePack.MessagePackSerializer.Serialize(Msg);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}
