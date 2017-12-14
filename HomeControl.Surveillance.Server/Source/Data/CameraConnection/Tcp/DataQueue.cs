using System;
using System.IO;
using System.Text;

namespace HomeControl.Surveillance.Server.Data.Tcp
{
    internal class DataQueue: IDisposable
    {
        private MemoryStream Stream;

        public Int32 Length => (Int32)Stream.Length;



        public DataQueue() { Stream = new MemoryStream(); }

        public void Enqueue(Byte[] data) => Stream.Write(data, 0, data.Length);

        public void Enqueue(Byte[] data, Int32 offset, Int32 count) => Stream.Write(data, offset, count);

        public Byte[] Dequeue(Int32 amount)
        {
            if (amount > Stream.Length)
                amount = (Int32)Stream.Length;

            var data = new Byte[amount];
            Stream.Position = 0;
            Stream.Read(data, 0, amount);

            var restDataLength = (Int32)Stream.Length - amount;
            if (restDataLength > 0)
            {
                var shiftedData = new Byte[restDataLength];
                Stream.Read(shiftedData, 0, restDataLength);
                Stream.Position = 0;
                Stream.Write(shiftedData, 0, restDataLength);
            }
            Stream.SetLength(restDataLength);
            Stream.Position = restDataLength;

            return data;
        }

        public Byte Peek()
        {
            var data = new Byte[1];
            Stream.Position = 0;
            Stream.Read(data, 0, 1);
            Stream.Position = Stream.Length;
            return data[0];
        }

        public Byte[] Peek(Int32 amount)
        {
            if (amount > Stream.Length)
                amount = (Int32)Stream.Length;

            var data = new Byte[amount];
            if (amount > 0)
            {
                Stream.Position = 0;
                Stream.Read(data, 0, amount);
                Stream.Position = Stream.Length;
            }
            return data;
        }

        public void Clear() => Stream.SetLength(0);

        public void Dispose() => Stream.Dispose();

        public String PeekString(Int32 amount)
        {
            var data = Peek(amount);
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        public Int32? IndexOf(String sequence)
        {
            var data = new Byte[1];
            Stream.Position = 0;
            while (Stream.Position <= Stream.Length - sequence.Length)
            {
                var position = Stream.Position;
                var i = 0;
                var matches = 0;
                while ((i < sequence.Length) && (Stream.Position < Stream.Length))
                {
                    Stream.Read(data, 0, 1);
                    if (data[0] == (Byte)sequence[i++])
                        matches++;
                }

                if (matches == sequence.Length)
                    return (Int32)position;

                Stream.Position = position + 1;
            }

            Stream.Position = Stream.Length;
            return null;
        }
    }
}