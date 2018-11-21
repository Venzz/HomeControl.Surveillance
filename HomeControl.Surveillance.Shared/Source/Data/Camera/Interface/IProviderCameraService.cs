﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Data.Camera
{
    public interface IProviderCameraService
    {
        event TypedEventHandler<IProviderCameraService, Command> CommandReceived;
        event TypedEventHandler<IProviderCameraService, (UInt32 Id, IMessage Message)> MessageReceived;
        event TypedEventHandler<IProviderCameraService, (String Message, String Parameter)> LogReceived;
        event TypedEventHandler<IProviderCameraService, (String Message, Exception Exception)> ExceptionReceived;

        Task SendAsync(Byte[] data);
        Task SendStoredRecordsMetadataAsync(UInt32 id, IReadOnlyCollection<DateTime> storedRecordsMetadata);
    }
}
