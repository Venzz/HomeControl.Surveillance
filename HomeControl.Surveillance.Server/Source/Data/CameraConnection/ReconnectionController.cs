using System;

namespace HomeControl.Surveillance.Server.Data
{
    public class ReconnectionController
    {
        private DateTime LastPermissionGrantedDate = DateTime.Now.AddSeconds(10);
        private UInt32 Attempts;

        public Boolean IsAllowed()
        {
            var isAllowed = false;
            if (Attempts < 5)
            {
                isAllowed = (DateTime.Now - LastPermissionGrantedDate) > TimeSpan.FromMinutes(2);
            }
            else if (Attempts < 10)
            {
                isAllowed = (DateTime.Now - LastPermissionGrantedDate) > TimeSpan.FromMinutes(10);
            }
            else
            {
                isAllowed = (DateTime.Now - LastPermissionGrantedDate) > TimeSpan.FromHours(1);
            }

            if (isAllowed)
            {
                Attempts++;
                LastPermissionGrantedDate = DateTime.Now;
            }
            return isAllowed;
        }

        public void ResetPermissionGrantedDate()
        {
            LastPermissionGrantedDate = DateTime.Now;
        }

        public void Reset()
        {
            Attempts = 0;
            LastPermissionGrantedDate = DateTime.Now;
        }
    }
}
