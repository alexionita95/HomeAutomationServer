using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public interface ICommunicationClient
    {
        event EventHandler<CommunicationMessageEventArgs> DataAvailable;
        event EventHandler Authentificated;
        int Type { get; set; }
        long Id { get; set; }

        void SendData(byte[] data);

    }
}
