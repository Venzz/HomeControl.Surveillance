using System.Collections.Generic;

namespace HomeControl.Surveillance.Data.Camera
{
    public class StoredRecordMediaDescriptorsResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordMediaDescriptorsResponse;
        public IReadOnlyCollection<Storage.StoredRecordFile.MediaDataDescriptor> MediaDescriptors { get; }

        public StoredRecordMediaDescriptorsResponse(IReadOnlyCollection<Storage.StoredRecordFile.MediaDataDescriptor> mediaDescriptors)
        {
            MediaDescriptors = mediaDescriptors;
        }
    }
}
