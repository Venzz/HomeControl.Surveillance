using System;
using System.Collections.Generic;

namespace HomeControl.Surveillance.Data.Camera
{
    public class FileListResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.FileListResponse;
        public IReadOnlyCollection<String> Files { get; }

        public FileListResponse(IReadOnlyCollection<String> files)
        {
            Files = files;
        }
    }
}
