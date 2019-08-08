using HomeControl.Surveillance.Data.Camera;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControl.Surveillance.Player.Model
{
    public class ProviderController
    {
        private IConsumerCameraService ConsumerService;



        public ProviderController(IConsumerCameraService consumerService)
        {
            ConsumerService = consumerService;
        }

        public async Task<IReadOnlyCollection<String>> GetFileListAsync()
        {
            await ConsumerService.EnsureConnectedAsync().ConfigureAwait(false);
            var fileList = await ConsumerService.GetFileListAsync().ConfigureAwait(false);
            return fileList;
        }

        public async Task<Byte[]> GetFileDataAsync(String id, UInt32 offset, UInt32 length, CancellationToken cancellationToken)
        {
            await ConsumerService.EnsureConnectedAsync().ConfigureAwait(false);
            var fileData = await ConsumerService.GetFileDataAsync(id, offset, length, cancellationToken).ConfigureAwait(false);
            return fileData;
        }
    }
}
