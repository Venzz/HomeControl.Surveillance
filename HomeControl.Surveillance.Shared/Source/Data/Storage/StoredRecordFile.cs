using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HomeControl.Surveillance.Data.Storage
{
    public class StoredRecordFile
    {
        public static IReadOnlyCollection<MediaDataDescriptor> ReadMediaDescriptors(Stream stream)
        {
            var mediaDataDescriptor = new List<MediaDataDescriptor>();
            var mediaStreamReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            while (stream.Position < stream.Length)
            {
                var previousHeaderOffset = (UInt32)stream.Position;
                var generalTimestamp = new DateTime(mediaStreamReader.ReadInt64(), DateTimeKind.Utc);
                var nextHeaderOffset = previousHeaderOffset + mediaStreamReader.ReadUInt32();
                var mediaDescriptorsCount = mediaStreamReader.ReadUInt32();
                for (var i = 0; i < mediaDescriptorsCount; i++)
                {
                    var mediaDataType = (MediaDataType)mediaStreamReader.ReadByte();
                    var timestampOffset = TimeSpan.FromMilliseconds(mediaStreamReader.ReadUInt32());
                    var duration = TimeSpan.FromMilliseconds(mediaStreamReader.ReadUInt16());
                    var offset = previousHeaderOffset + mediaStreamReader.ReadUInt32();
                    mediaDataDescriptor.Add(new MediaDataDescriptor() { Type = mediaDataType, Offset = offset, Timestamp = generalTimestamp.Add(timestampOffset), Duration = duration });
                }
                stream.Seek(nextHeaderOffset, SeekOrigin.Begin);
            }
            return mediaDataDescriptor;
        }

        public static void WriteSlice(Stream stream, IReadOnlyCollection<(MediaDataDescriptor Descriptor, Byte[] Data)> items)
        {
            if (items.Count == 0)
                return;

            var generalTimestamp = items.First().Descriptor.Timestamp;
            var mediaStreamWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            // Writing header

            mediaStreamWriter.Write((UInt64)generalTimestamp.Ticks);
            mediaStreamWriter.Write((UInt32)0);
            mediaStreamWriter.Write((UInt32)items.Count);
            foreach (var item in items)
            {
                mediaStreamWriter.Write((Byte)item.Descriptor.Type);
                mediaStreamWriter.Write((UInt32)(item.Descriptor.Timestamp - generalTimestamp).TotalMilliseconds);
                mediaStreamWriter.Write((UInt16)item.Descriptor.Duration.TotalMilliseconds);
                mediaStreamWriter.Write((UInt32)0);
            }

            // Writing media data

            var i = 0;
            foreach (var item in items)
            {
                stream.Seek(0, SeekOrigin.End);
                var mediaDataOffset = stream.Position;

                mediaStreamWriter.Write((UInt32)item.Data.Length);
                mediaStreamWriter.Write((Byte[])item.Data);

                // Writing media data offset to the header

                stream.Seek((8 + 4 + 4) + ((1 + 4 + 2 + 4) * i) + (1 + 4 + 2), SeekOrigin.Begin);
                mediaStreamWriter.Write((UInt32)mediaDataOffset);
                i++;
            }

            // Writing next header offset

            stream.Seek(8, SeekOrigin.Begin);
            mediaStreamWriter.Write((UInt32)stream.Length);
        }

        public class MediaDataDescriptor
        {
            public MediaDataType Type { get; set; }
            public UInt32 Offset { get; set; }
            public DateTime Timestamp { get; set; }
            public TimeSpan Duration { get; set; }
        }
    }
}
