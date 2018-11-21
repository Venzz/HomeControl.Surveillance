using System;
using System.Collections.Generic;

namespace HomeControl.Surveillance.Data.Camera.Heroku
{
    public class StoredRecordsMetadataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordsMetadataResponse;
        public IReadOnlyCollection<DateTime> RecordsMetadata { get; }

        public StoredRecordsMetadataResponse(IReadOnlyCollection<DateTime> recordsMetadata)
        {
            RecordsMetadata = recordsMetadata;
        }
    }
}
