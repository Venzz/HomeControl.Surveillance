using System;
using System.Collections.Generic;
using System.IO;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class Message
    {
        private static UInt32 LastGeneratedId;
        private static Object Sync = new Object();

        public UInt32 Id { get; }
        public MessageId Type { get; }
        public Byte[] Data { get; }

        public Message(Byte[] data)
        {
            Id = BitConverter.ToUInt32(data, 8);
            Type = (MessageId)data[12];
            Data = data;
        }

        public Message(UInt32 id, IMessage message)
        {
            Id = id;
            Type = message.Type;
            using (var messageDataStream = new MemoryStream())
            using (var messageDataWriter = new BinaryWriter(messageDataStream))
            {
                messageDataWriter.Write(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFE });
                messageDataWriter.Write(new Byte[] { 0x00, 0x00, 0x00, 0x00 });
                messageDataWriter.Write(id);
                messageDataWriter.Write((Byte)message.Type);
                switch (message)
                {
                    case StoredRecordsMetadataRequest storedRecordsMetadataRequest:
                        break;
                    case StoredRecordsMetadataResponse storedRecordsMetadataResponse:
                        messageDataWriter.Write((UInt16)storedRecordsMetadataResponse.RecordsMetadata.Count);
                        foreach (var item in storedRecordsMetadataResponse.RecordsMetadata)
                            messageDataWriter.Write(item.Ticks);
                        break;
                    case LiveMediaDataResponse liveMediaDataResponse:
                        messageDataWriter.Write((Byte)liveMediaDataResponse.MediaType);
                        messageDataWriter.Write(liveMediaDataResponse.Data.Length);
                        messageDataWriter.Write(liveMediaDataResponse.Data);
                        messageDataWriter.Write(liveMediaDataResponse.Timestamp.Ticks);
                        messageDataWriter.Write((UInt32)liveMediaDataResponse.Duration.TotalMilliseconds);
                        break;
                    default:
                        throw new NotSupportedException($"Message of type {message.Type} is not supported.");
                }
                messageDataWriter.Seek(4, SeekOrigin.Begin);
                messageDataWriter.Write((UInt32)messageDataStream.Length - 8);

                Data = new Byte[messageDataStream.Length];
                messageDataStream.Position = 0;
                messageDataStream.Read(Data, 0, (Int32)messageDataStream.Length);
            }
        }

        public static IMessage Create(Message message)
        {
            using (var messageDataStream = new MemoryStream(message.Data, 13, message.Data.Length - 13))
            using (var messageDataReader = new BinaryReader(messageDataStream))
            {
                switch (message.Type)
                {
                    case MessageId.StoredRecordsMetadataRequest:
                        return new StoredRecordsMetadataRequest();
                    case MessageId.StoredRecordsMetadataResponse:
                        var recordsCount = messageDataReader.ReadUInt16();
                        var recordsMetadata = new List<DateTime>();
                        for (var i = 0; i < recordsCount; i++)
                            recordsMetadata.Add(new DateTime(messageDataReader.ReadInt64()));
                        return new StoredRecordsMetadataResponse(recordsMetadata);
                    case MessageId.LiveMediaData:
                        var mediaDataType = (MediaDataType)messageDataReader.ReadByte();
                        var dataLength = messageDataReader.ReadInt32();
                        var data = messageDataReader.ReadBytes(dataLength);
                        var timestamp = new DateTime(messageDataReader.ReadInt64(), DateTimeKind.Utc);
                        var duration = TimeSpan.FromMilliseconds(messageDataReader.ReadUInt32());
                        return new LiveMediaDataResponse(mediaDataType, data, timestamp, duration);
                    default:
                        throw new NotSupportedException($"Message of type {message.Type} is not supported.");
                }
            }
        }

        public static UInt32 GetId()
        {
            lock (Sync)
            {
                LastGeneratedId++;
                return LastGeneratedId;
            }
        }
    }
}
