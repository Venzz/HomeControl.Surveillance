using HomeControl.Surveillance.Data.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class Message
    {
        private static UInt32 LastGeneratedId;
        private static Object Sync = new Object();

        public UInt32 ConsumerId { get; }
        public UInt32 Id { get; }
        public MessageId Type { get; }
        public Byte[] Data { get; }

        public Message(Byte[] data)
        {
            ConsumerId = BitConverter.ToUInt32(data, 4);
            Id = BitConverter.ToUInt32(data, 12);
            Type = (MessageId)data[16];
            Data = data;
        }

        public Message(UInt32 consumerId, UInt32 id, IMessage message)
        {
            ConsumerId = consumerId;
            Id = id;
            Type = message.Type;
            using (var messageDataStream = new MemoryStream())
            using (var messageDataWriter = new BinaryWriter(messageDataStream))
            {
                messageDataWriter.Write(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFE });
                messageDataWriter.Write(ConsumerId);
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
                        {
                            messageDataWriter.Write(item.Id);
                            messageDataWriter.Write(item.Date.Ticks);
                        }
                        break;
                    case LiveMediaDataResponse liveMediaDataResponse:
                        messageDataWriter.Write((Byte)liveMediaDataResponse.MediaType);
                        messageDataWriter.Write(liveMediaDataResponse.Data.Length);
                        messageDataWriter.Write(liveMediaDataResponse.Data);
                        messageDataWriter.Write(liveMediaDataResponse.Timestamp.Ticks);
                        messageDataWriter.Write((UInt32)liveMediaDataResponse.Duration.TotalMilliseconds);
                        break;
                    case StoredRecordMediaDescriptorsRequest storedRecordMediaDescriptorsRequest:
                        messageDataWriter.Write(storedRecordMediaDescriptorsRequest.StoredRecordId);
                        break;
                    case StoredRecordMediaDescriptorsResponse storedRecordMediaDescriptorsResponse:
                        messageDataWriter.Write((UInt32)storedRecordMediaDescriptorsResponse.MediaDescriptors.Count);
                        foreach (var item in storedRecordMediaDescriptorsResponse.MediaDescriptors)
                        {
                            messageDataWriter.Write((Byte)item.Type);
                            messageDataWriter.Write(item.Offset);
                            messageDataWriter.Write(item.Timestamp.Ticks);
                            messageDataWriter.Write(item.Duration.TotalMilliseconds);
                        }
                        break;
                    case StoredRecordMediaDataRequest storedRecordMediaDataRequest:
                        messageDataWriter.Write(storedRecordMediaDataRequest.StoredRecordId);
                        messageDataWriter.Write(storedRecordMediaDataRequest.Offset);
                        break;
                    case StoredRecordMediaDataResponse storedRecordMediaDataResponse:
                        messageDataWriter.Write(storedRecordMediaDataResponse.Data.Length);
                        messageDataWriter.Write(storedRecordMediaDataResponse.Data);
                        break;
                    case PartialMessageResponse partialMessageResponse:
                        messageDataWriter.Write(partialMessageResponse.Final);
                        messageDataWriter.Write(partialMessageResponse.Data.Length);
                        messageDataWriter.Write(partialMessageResponse.Data);
                        break;
                    default:
                        throw new NotSupportedException($"Message of type {message.Type} is not supported.");
                }
                messageDataWriter.Seek(8, SeekOrigin.Begin);
                messageDataWriter.Write((UInt32)messageDataStream.Length - 12);

                Data = new Byte[messageDataStream.Length];
                messageDataStream.Position = 0;
                messageDataStream.Read(Data, 0, (Int32)messageDataStream.Length);
            }
        }

        public Message(IServiceMessage message)
        {
            using (var messageDataStream = new MemoryStream())
            using (var messageDataWriter = new BinaryWriter(messageDataStream))
            {
                messageDataWriter.Write(new Byte[] { 0xFF, 0xFF, 0xFF, 0xFD });
                messageDataWriter.Write(new Byte[] { 0x00, 0x00, 0x00, 0x00 });
                messageDataWriter.Write(new Byte[] { 0x00, 0x00, 0x00, 0x00 });
                messageDataWriter.Write((UInt32)0);
                messageDataWriter.Write((Byte)message.Type);
                switch (message)
                {
                    case PushChannelUri pushChannelUri:
                        var previousChannelUriData = Encoding.UTF8.GetBytes(pushChannelUri.PreviousChannelUri);
                        var channelUriData = Encoding.UTF8.GetBytes(pushChannelUri.ChannelUri);
                        messageDataWriter.Write(previousChannelUriData.Length);
                        messageDataWriter.Write(previousChannelUriData);
                        messageDataWriter.Write(channelUriData.Length);
                        messageDataWriter.Write(channelUriData);
                        break;
                    default:
                        throw new NotSupportedException($"Service Message of type {message.Type} is not supported.");
                }
                messageDataWriter.Seek(8, SeekOrigin.Begin);
                messageDataWriter.Write((UInt32)messageDataStream.Length - 12);

                Data = new Byte[messageDataStream.Length];
                messageDataStream.Position = 0;
                messageDataStream.Read(Data, 0, (Int32)messageDataStream.Length);
            }
        }

        public static IMessage Create(Message message)
        {
            using (var messageDataStream = new MemoryStream(message.Data, 17, message.Data.Length - 17))
            using (var messageDataReader = new BinaryReader(messageDataStream))
            {
                switch (message.Type)
                {
                    case MessageId.StoredRecordsMetadataRequest:
                        return new StoredRecordsMetadataRequest();
                    case MessageId.StoredRecordsMetadataResponse:
                        var recordsCount = (UInt32)messageDataReader.ReadUInt16();
                        var recordsMetadata = new List<(String Id, DateTime Date)>();
                        for (var i = 0; i < recordsCount; i++)
                            recordsMetadata.Add((messageDataReader.ReadString(), new DateTime(messageDataReader.ReadInt64())));
                        return new StoredRecordsMetadataResponse(recordsMetadata);
                    case MessageId.LiveMediaData:
                        var mediaDataType = (MediaDataType)messageDataReader.ReadByte();
                        var dataLength = messageDataReader.ReadInt32();
                        var data = messageDataReader.ReadBytes(dataLength);
                        var timestamp = new DateTime(messageDataReader.ReadInt64(), DateTimeKind.Utc);
                        var duration = TimeSpan.FromMilliseconds(messageDataReader.ReadUInt32());
                        return new LiveMediaDataResponse(mediaDataType, data, timestamp, duration);
                    case MessageId.StoredRecordMediaDescriptorsRequest:
                        return new StoredRecordMediaDescriptorsRequest(messageDataReader.ReadString());
                    case MessageId.StoredRecordMediaDescriptorsResponse:
                        recordsCount = messageDataReader.ReadUInt32();
                        var mediaDataDescriptors = new List<StoredRecordFile.MediaDataDescriptor>();
                        for (var i = 0; i < recordsCount; i++)
                        {
                            mediaDataDescriptors.Add(new StoredRecordFile.MediaDataDescriptor()
                            {
                                Type = (MediaDataType)messageDataReader.ReadByte(),
                                Offset = messageDataReader.ReadUInt32(),
                                Timestamp = new DateTime(messageDataReader.ReadInt64(), DateTimeKind.Utc),
                                Duration = TimeSpan.FromMilliseconds(messageDataReader.ReadDouble())
                            });
                        }
                        return new StoredRecordMediaDescriptorsResponse(mediaDataDescriptors);
                    case MessageId.StoredRecordMediaDataRequest:
                        return new StoredRecordMediaDataRequest(messageDataReader.ReadString(), messageDataReader.ReadUInt32());
                    case MessageId.StoredRecordMediaDataResponse:
                        dataLength = messageDataReader.ReadInt32();
                        return new StoredRecordMediaDataResponse(messageDataReader.ReadBytes(dataLength));
                    case MessageId.PartialMessageResponse:
                        var final = messageDataReader.ReadBoolean();
                        dataLength = messageDataReader.ReadInt32();
                        return new PartialMessageResponse(final, messageDataReader.ReadBytes(dataLength));
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
