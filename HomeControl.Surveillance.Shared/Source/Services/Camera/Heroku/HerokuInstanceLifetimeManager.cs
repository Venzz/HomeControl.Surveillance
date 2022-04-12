using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Services
{
    public class HerokuInstanceLifetimeManager
    {
        private HttpClient HttpClient;
        private (TimeSpan From, TimeSpan Duration) IdlePeriod;
        private DateTime IdlingStartedDate;

        public Boolean IsIdlingActive => DateTime.Now - IdlingStartedDate < IdlePeriod.Duration;

        public event TypedEventHandler<HerokuInstanceLifetimeManager, (String Source, String Message)> Log = delegate { };
        public event TypedEventHandler<HerokuInstanceLifetimeManager, (String Source, String Details, Exception Exception)> Exception = delegate { };



        public HerokuInstanceLifetimeManager((TimeSpan From, TimeSpan Duration) idlePeriod)
        {
            IdlePeriod = idlePeriod;
            HttpClient = new HttpClient();
            StartReconnectionMaintaining();
        }

        private async void StartReconnectionMaintaining() => await Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(14)).ConfigureAwait(false);
                    var now = DateTime.Now;
                    if ((now.TimeOfDay > IdlePeriod.From) && !IsIdlingActive)
                    {
                        IdlingStartedDate = new DateTime(now.Year, now.Month, now.Day, IdlePeriod.From.Hours, IdlePeriod.From.Minutes, IdlePeriod.From.Seconds, 0, now.Kind);
                        Log(this, ($"{nameof(HerokuProviderCameraService)}", "Idling started."));
                    }
                    else
                    {
                        if (IdlingStartedDate != default(DateTime))
                        {
                            IdlingStartedDate = default(DateTime);
                            Log(this, ($"{nameof(HerokuProviderCameraService)}", "Idling finished."));
                        }
                        await HttpClient.GetAsync("https://home-security-proxy-dev.herokuapp.com/").ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    Exception(this, ($"{nameof(HerokuProviderCameraService)}", null, exception));
                }
            }
        });
    }
}
