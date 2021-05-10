using System;
using System.Collections.Generic;

namespace HomeControl.Surveillance.Services.Heroku
{
    public class StoredRecordsMetadataResponse: IMessage
    {
        public MessageId Type { get; } = MessageId.StoredRecordsMetadataResponse;
        public IReadOnlyCollection<(String Id, DateTime Date)> RecordsMetadata { get; }

        public StoredRecordsMetadataResponse(IReadOnlyCollection<(String Id, DateTime Date)> recordsMetadata)
        {
            RecordsMetadata = recordsMetadata;
        }
    }
}
