using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    class SocketCommunicationClient : ICommunicationClient
    {
        public int Type { get; set; }
        public long Id { get; set; }
        public Socket Socket { get; set; }

        public event EventHandler<CommunicationMessageEventArgs> DataAvailable;
        public event EventHandler Authentificated;
        protected virtual void OnDataAvailable(CommunicationMessageEventArgs args)
        {
            DataAvailable?.Invoke(this, args);
        }
        protected virtual void OnAuthentificated()
        {
            Authentificated.Invoke(this, EventArgs.Empty);
        }
        public SocketCommunicationClient(Socket soket)
        {
            Socket = soket;
            SocketCommunicationMessage state = new SocketCommunicationMessage();
            state.MessageDecoded += State_MessageDecoded;
            state.workSocket = Socket;
            Socket.BeginReceive(state.Buffer, state.Offset, CommunicationMessage.BufferSize - state.Offset, 0,
                new AsyncCallback(ReadCallback), state);
        }

        private void State_MessageDecoded(object sender, CommunicationMessageEventArgs e)
        {
            string payload = Encoding.UTF8.GetString(e.Payload);
            switch(e.Type)
            {
                case 1:
                    Tuple<int,long> authData =CommunicationMessageInterpreter.GetAuthentificationData(e.Payload);
                    Type = authData.Item1;
                    Id = authData.Item2;
                    OnAuthentificated();
                    break;
                default:
                    OnDataAvailable(e);
                    break;

            }
            Console.WriteLine($"{e.Type} Payload: {payload}");
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            SocketCommunicationMessage state = (SocketCommunicationMessage)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            try
            {
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {

                    state.BytesRead += (ulong)bytesRead;

                    // There  might be more data, so store the data received so far.  

                    // Check for end-of-file tag. If it is not there, read
                    // more data.  
                    state.ReadMessage();
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.Buffer, state.Offset, CommunicationMessage.BufferSize - state.Offset, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public void SendData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
