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
        private DateTime InstancePingedDate;

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
                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                    var now = DateTime.Now;
                    if ((now.TimeOfDay > IdlePeriod.From) && !IsIdlingActive)
                    {
                        IdlingStartedDate = new DateTime(now.Year, now.Month, now.Day, IdlePeriod.From.Hours, IdlePeriod.From.Minutes, IdlePeriod.From.Seconds, 0, now.Kind);
                        Log(this, ($"{nameof(HerokuInstanceLifetimeManager)}", "Idling started."));
                    }
                    else if (!IsIdlingActive)
                    {
                        if (IdlingStartedDate != default(DateTime))
                        {
                            IdlingStartedDate = default(DateTime);
                            Log(this, ($"{nameof(HerokuInstanceLifetimeManager)}", "Idling finished."));
                        }
                        if (now - InstancePingedDate >= TimeSpan.FromMinutes(15))
                        {
                            InstancePingedDate = now;
                            await HttpClient.GetAsync(PrivateData.HerokuServiceUrl).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Exception(this, ($"{nameof(HerokuInstanceLifetimeManager)}", null, exception));
                }
            }
        });
    }
}
