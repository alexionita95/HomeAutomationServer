using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{

    public class CommunicationMessageEventArgs : EventArgs
    {
        public uint Type { get; set; }
        public byte[] Payload { get; set; }
    }
    public class CommunicationMessageHeader
    {
        public uint Type { get; set; }
        public uint PayloadSize { get; set; }
    }
    public class CommunicationMessage
    {
        public event EventHandler<CommunicationMessageEventArgs> MessageDecoded;
        CommunicationMessageHeader Header { get; set; }
        public const int BufferSize = 1024;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
        public byte[] Payload { get; set; } = null;
        public int Offset { get; set; } = 0;
        public ulong BytesRead { get; set; } = 0;
        public ulong CurrentPayloadSize { get; set; } = 0;

        public CommunicationMessage()
        {
            Header = new CommunicationMessageHeader();
        }
        protected virtual void OnMessageDecoded(CommunicationMessageEventArgs args)
        {
            MessageDecoded?.Invoke(this, args);
        }

        private void Reset()
        {
            Payload = null;
            Header.Type = 0;
            Header.PayloadSize = 0;
            CurrentPayloadSize = 0;
        }

        public void ReadMessage()
        {
            ulong bytesRead = BytesRead;
            int index = 0;
            if (Header.Type == 0 && bytesRead >= sizeof(uint))
            {
                byte[] typeArray = new byte[sizeof(uint)];
                Array.Copy(Buffer,index, typeArray,0, sizeof(uint));
                uint type = BitConverter.ToUInt32(typeArray,0);
                Header.Type = type;
                bytesRead -= sizeof(uint);
                index += sizeof(uint);
            }

            if (Header.PayloadSize == 0 && bytesRead >=sizeof(uint))
            {
                byte[] payloadArray = new byte[sizeof(uint)];
                Array.Copy(Buffer, index,payloadArray, 0, sizeof(uint));
                uint payloadSize = BitConverter.ToUInt32(payloadArray, 0);
                bytesRead -= sizeof(uint);
                index += sizeof(uint);
                Header.PayloadSize = payloadSize;
            }

            if(Payload == null && Header.PayloadSize != 0)
            {
                Payload = new byte[Header.PayloadSize];
            }

            ulong remaningBytes = Header.PayloadSize - CurrentPayloadSize;

            if(bytesRead > remaningBytes)
            {
                Array.Copy(Buffer, index, Payload, (int)CurrentPayloadSize, (int)remaningBytes);
                bytesRead -= remaningBytes;
                index += (int)remaningBytes;
                CurrentPayloadSize += remaningBytes;
            }
            else
            {
                Array.Copy(Buffer, index, Payload, (int)CurrentPayloadSize, (int)bytesRead);
                CurrentPayloadSize += bytesRead;
                bytesRead = 0;
            }

            if(bytesRead != 0)
            {
                Array.Copy(Buffer, index, Buffer, 0, (int)bytesRead);
            }
            Offset = (int)bytesRead;
            BytesRead = bytesRead;
            if (CurrentPayloadSize == Header.PayloadSize)
            {
                CommunicationMessageEventArgs args = new CommunicationMessageEventArgs();
                args.Type = Header.Type;
                args.Payload = new byte[Header.PayloadSize];
                Array.Copy(Payload, args.Payload, (int)Header.PayloadSize);
                OnMessageDecoded(args);
                Reset();
            }
            if(BytesRead >= sizeof(uint))
            {
                ReadMessage();
            }





        }

    }


}
