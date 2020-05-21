using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace ServerApplication
{
    
    public partial class MainForm : Form
    {
        ICommunicationListener listener;
        Thread listenerThread;
        List<ICommunicationClient> clients;
        List<DeviceClient> devices;
        public MainForm()
        {
            InitializeComponent();
            Logger.Log = (message) =>
            {
                textBox1.Invoke((MethodInvoker)delegate
                {
                    textBox1.Text += message + Environment.NewLine;
                });
            };
            clients = new List<ICommunicationClient>();
            devices = new List<DeviceClient>();
            listener = new SocketCommunicationListener();
            listener.ClientAvailable += Listener_ClientAvailable;
            listenerThread = new Thread(new ThreadStart(() =>
            {
                listener.Start();
            }));
        }

        private void Listener_ClientAvailable(object sender, CommunicationListenerEventArgs e)
        {
            Logger.Log($"Client connected");
            e.Client.Authentificated += (object source, EventArgs args) =>
            {
                ICommunicationClient client = (ICommunicationClient)source;
                Logger.Log($"Client authentificated Type:{client.Type} ID:{client.Id}");
                DeviceClient device = new DeviceClient(client);
                device.ModelAvailable += (object clientS, EventArgs a) =>
                 {
                     DeviceClient curentClient = (DeviceClient)clientS;
                     Logger.Log($"Model data available from client {curentClient.Client.Id}");
                     //Console.WriteLine(curentClient.DecodeModel());
                     RenderModel(curentClient.DecodeModel());
                     
                 };
                devices.Add(device);
                clients.Remove(client);
            };
            clients.Add(e.Client);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listenerThread.Start();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            listenerThread.Abort();
        }
        public void RenderModel(BasicModel model)
        {
            Logger.Log("Rendering new model");
            renderPanel.Invoke((MethodInvoker)delegate
            {
                Graphics g = renderPanel.CreateGraphics();
                g.Clear(Color.White);
                g.DrawRectangle(Pens.Black, new Rectangle(model.x, model.y, model.w, model.h));
            });
        }
    }

    public struct BasicModel
    {
        public long id;
        public int x, y, w, h;
        public override string ToString()
        {
            return $"ID:{id} x:{x} y:{y} w:{w} h:{h}";
        }
    }
    public class DeviceClient
    {
        public event EventHandler ModelAvailable;
        protected virtual void OnModelAvailable()
        {
            ModelAvailable?.Invoke(this, EventArgs.Empty);
        }
        public DeviceClient()
        {

        }
        public DeviceClient(ICommunicationClient client)
        {
            Client = client;
            Client.DataAvailable += (object sender, CommunicationMessageEventArgs e) =>
            {
                switch (e.Type)
                {
                    case 2:
                        byte[] model = DecompressModel(e.Payload);
                        ModelData = new byte[model.Length];
                        Buffer.BlockCopy(model, 0, ModelData, 0, ModelData.Length);
                        OnModelAvailable();
                        break;
                }
            };
        }
        public BasicModel DecodeModel()
        {
            long id = CommunicationMessageInterpreter.ReadInt64(ModelData, 0);
            int x = CommunicationMessageInterpreter.ReadInt32(ModelData, sizeof(ulong));
            int y = CommunicationMessageInterpreter.ReadInt32(ModelData, sizeof(uint) + sizeof(ulong));
            int w = CommunicationMessageInterpreter.ReadInt32(ModelData, 2 * sizeof(uint) + sizeof(ulong));
            int h = CommunicationMessageInterpreter.ReadInt32(ModelData, 3 * sizeof(uint) + sizeof(ulong));
            return new BasicModel { id = id, x = x, y = y, w = w, h = h };
        }
        public byte[] ModelData { get; set; }
        public ICommunicationClient Client { get; private set; }

        public byte[] DecompressModel(byte[] inputData)
        {
            Logger.Log($"Decompressing model for device {Client.Id}");
            Logger.Log($"Compressed model size: {inputData.Length}");
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            MemoryStream input = new MemoryStream(inputData);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            Logger.Log($"Decompressed model size: {output.Length}");
            return output.ToArray();

        }
    }
}
