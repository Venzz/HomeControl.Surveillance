using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Venz.UI.Xaml
{
    public partial class Page: Windows.UI.Xaml.Controls.Page
    {
        public Page(): this(NavigationCacheMode.Disabled) { }

        public Page(NavigationCacheMode cacheMode)
        {
            if (DesignMode.DesignModeEnabled)
                RequestedTheme = ElementTheme.Light;

            Page_BackKey();

            IsTabStop = true;
            NavigationCacheMode = cacheMode;
            Loaded += OnLoaded;
        }

        protected virtual void OnLoaded(Object sender, RoutedEventArgs args)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            BackKey_OnNavigatedTo(args);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            base.OnNavigatingFrom(args);
            BackKey_OnNavigatingFrom(args);
        }
    }
}
