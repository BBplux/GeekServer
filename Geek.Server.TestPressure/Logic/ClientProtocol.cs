﻿using System;
using System.Buffers;
using Bedrock.Framework.Protocols;
using Geek.Server.Core.Net.Messages;
using Geek.Server.Core.Utils;
using MessagePack;

namespace Geek.Server.TestPressure.Logic
{
    public class ClientProtocol : IProtocal<NetMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NetMessage message)
        {
            var reader = new SequenceReader<byte>(input);
            //客户端传过来的length包含了长度自身（data: [length:byte[1,2,3,4]] ==> 则length=int 4 个字节+byte数组长度4=8）
            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
            {
                message = default;
                return false;
            }

            var payload = input.Slice(reader.Position, length - 4);//length已经被TryReadBigEndian读取
            message = new NetMessage(payload);

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        private const int Magic = 0x1234;
        int count = 0;
        public void WriteMessage(NetMessage nmsg, IBufferWriter<byte> output)
        {
            //length + timestamp + magic + msgid
            var bytes = nmsg.Serialize();
            int len = 4 + 8 + 4 + 4 + bytes.Length;
            var span = output.GetSpan(len);

            int magic = Magic + ++count;
            magic ^= Magic << 8;
            magic ^= len;

            int offset = 0;
            XBuffer.WriteInt(len, span, ref offset);
            XBuffer.WriteLong(DateTime.Now.Ticks, span, ref offset);
            XBuffer.WriteInt(magic, span, ref offset);
            XBuffer.WriteInt(nmsg.Msg.MsgId, span, ref offset);
            XBuffer.WriteBytesWithoutLength(bytes, span, ref offset);
            output.Advance(len);
        }
    }

}
