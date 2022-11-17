﻿using System.Buffers;
using Bedrock.Framework.Protocols;

namespace Geek.Server.Gateway.Logic.Net.Tcp.Outer
{
    public class OuterProtocol : IProtocal<NMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out NMessage message)
        {
            var reader = new SequenceReader<byte>(input);
            //客户端传过来的length包含了长度自身（data: [length:byte[1,2,3,4]] ==> 则length=int 4 个字节+byte数组长度4=8）
            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
            {
                message = default;
                return false;
            }

            var payload = input.Slice(reader.Position, length - 4);
            message = new NMessage(payload);

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        public void WriteMessage(NMessage nmsg, IBufferWriter<byte> output)
        {
            byte[] bytes = nmsg.GetBytes();
            int len = 8 + bytes.Length;
            var span = output.GetSpan(len);
            int offset = 0;
            XBuffer.WriteInt(len, span, ref offset);
            XBuffer.WriteInt(nmsg.MsgId, span, ref offset);
            XBuffer.WriteBytesWithoutLength(bytes, span, ref offset);
            output.Advance(len);
        }

    }
}
