using System.Collections.Generic;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class StoredRecordMediaDescriptorsResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordMediaDescriptorsResponse;
        public IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> MediaDescriptors { get; }

        public StoredRecordMediaDescriptorsResponse(IReadOnlyCollection<StoredRecordFile.MediaDataDescriptor> mediaDescriptors)
        {
            MediaDescriptors = mediaDescriptors;
        }
    }
}
