﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera
{
    public interface IConsumerCameraService
    {
        event TypedEventHandler<IConsumerCameraService, Byte[]> DataReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IConsumerCameraService, (String Message, Exception Exception)> ExceptionReceived;

        Task PerformAsync(Command command);
        Task<IReadOnlyCollection<DateTime>> GetStoredRecordsMetadataAsync();
    }
}
