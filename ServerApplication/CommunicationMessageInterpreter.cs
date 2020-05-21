using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public class CommunicationMessageInterpreter
    {
        public static Tuple<int,long> GetAuthentificationData(byte[] payload)
        {
            int type = ReadInt32(payload, 0);
            long id = ReadInt64(payload, sizeof(int));
            return new Tuple<int, long>(type,id);
        }

        public static int ReadInt32(byte[] buffer, int index)
        {
            int result = 0;
            byte[] array = new byte[sizeof(int)];
            Array.Copy(buffer, index, array, 0, sizeof(int));
            result = BitConverter.ToInt32(array, 0);
            return result;
        }

        public static long ReadInt64(byte[] buffer, int index)
        {
            long result = 0;
            byte[] array = new byte[sizeof(long)];
            Array.Copy(buffer, index, array, 0, sizeof(long));
            result = BitConverter.ToInt64(array, 0);
            return result;
        }
    }
}
