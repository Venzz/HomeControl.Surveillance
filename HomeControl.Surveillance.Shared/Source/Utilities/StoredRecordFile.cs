using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HomeControl.Surveillance
{
    public class StoredRecordFile
    {
        private BinaryReader MediaStreamReader;
        private BinaryWriter MediaStreamWriter;

        public StoredRecordFile(Stream stream)
        {
            MediaStreamReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            MediaStreamWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        }

        public IReadOnlyCollection<MediaDataDescriptor> ReadMediaDescriptors()
        {
            var mediaDataDescriptor = new List<MediaDataDescriptor>();
            while (MediaStreamReader.BaseStream.Position < MediaStreamReader.BaseStream.Length)
            {
                var previousHeaderOffset = (UInt32)MediaStreamReader.BaseStream.Position;
                var generalTimestamp = new DateTime(MediaStreamReader.ReadInt64(), DateTimeKind.Utc);
                var nextHeaderOffset = previousHeaderOffset + MediaStreamReader.ReadUInt32();
                var mediaDescriptorsCount = MediaStreamReader.ReadUInt32();
                for (var i = 0; i < mediaDescriptorsCount; i++)
                {
                    var mediaDataType = (MediaDataType)MediaStreamReader.ReadByte();
                    var timestampOffset = TimeSpan.FromMilliseconds(MediaStreamReader.ReadUInt32());
                    var duration = TimeSpan.FromMilliseconds(MediaStreamReader.ReadUInt16());
                    var offset = previousHeaderOffset + MediaStreamReader.ReadUInt32();
                    mediaDataDescriptor.Add(new MediaDataDescriptor() { Type = mediaDataType, Offset = offset, Timestamp = generalTimestamp.Add(timestampOffset), Duration = duration });
                }
                MediaStreamReader.BaseStream.Seek(nextHeaderOffset, SeekOrigin.Begin);
            }
            return mediaDataDescriptor;
        }

        public void WriteSlice(IReadOnlyCollection<(MediaDataDescriptor Descriptor, Byte[] Data)> items)
        {
            if (items.Count == 0)
                return;

            // Writing header

            var generalTimestamp = items.First().Descriptor.Timestamp;
            MediaStreamWriter.Write((UInt64)generalTimestamp.Ticks);
            MediaStreamWriter.Write((UInt32)0);
            MediaStreamWriter.Write((UInt32)items.Count);
            foreach (var item in items)
            {
                MediaStreamWriter.Write((Byte)item.Descriptor.Type);
                MediaStreamWriter.Write((UInt32)(item.Descriptor.Timestamp - generalTimestamp).TotalMilliseconds);
                MediaStreamWriter.Write((UInt16)item.Descriptor.Duration.TotalMilliseconds);
                MediaStreamWriter.Write((UInt32)0);
            }

            // Writing media data

            var i = 0;
            foreach (var item in items)
            {
                MediaStreamWriter.BaseStream.Seek(0, SeekOrigin.End);
                var mediaDataOffset = MediaStreamWriter.BaseStream.Position;

                MediaStreamWriter.Write((UInt32)item.Data.Length);
                MediaStreamWriter.Write((Byte[])item.Data);

                // Writing media data offset to the header

                MediaStreamWriter.BaseStream.Seek((8 + 4 + 4) + ((1 + 4 + 2 + 4) * i) + (1 + 4 + 2), SeekOrigin.Begin);
                MediaStreamWriter.Write((UInt32)mediaDataOffset);
                i++;
            }

            // Writing next header offset

            MediaStreamWriter.BaseStream.Seek(8, SeekOrigin.Begin);
            MediaStreamWriter.Write((UInt32)MediaStreamWriter.BaseStream.Length);
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
