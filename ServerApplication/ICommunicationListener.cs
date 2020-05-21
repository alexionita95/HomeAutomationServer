using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class CommunicationListenerEventArgs : EventArgs 
    {
        public ICommunicationClient Client { get; set; }
    }
    public interface ICommunicationListener
    {
        event EventHandler<CommunicationListenerEventArgs> ClientAvailable;
        void Start();
    }
}
