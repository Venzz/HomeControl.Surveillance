using HomeControl.Surveillance.Data.Camera;
using System;
using System.Collections.Generic;
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
            ConsumerService.EnsureConnected();
            var fileList = await ConsumerService.GetFileListAsync().ConfigureAwait(false);
            return fileList;
        }

        public async Task<Byte[]> GetFileDataAsync(String id, UInt32 offset, UInt32 length)
        {
            ConsumerService.EnsureConnected();
            var fileData = await ConsumerService.GetFileDataAsync(id, offset, length).ConfigureAwait(false);
            return fileData;
        }
    }
}
