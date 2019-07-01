using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Venz.UI.Xaml
{
    public partial class Page
    {
        private SystemNavigationManager NavigationManager;
        private IList<Action> BackKeyActions;



        private void Page_BackKey()
        {
            BackKeyActions = new List<Action>();
            if (!DesignMode.DesignModeEnabled)
            {
                NavigationManager = SystemNavigationManager.GetForCurrentView();
                NavigationManager.BackRequested += OnBackRequested;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs args)
        {
            base.OnKeyUp(args);
            if (args.Key == VirtualKey.F1)
                Back();
        }

        private void BackKey_OnNavigatedTo(NavigationEventArgs args)
        {
            if (!DesignMode.DesignModeEnabled)
            {
                NavigationManager.AppViewBackButtonVisibility = (Frame.CanGoBack || (BackKeyActions.Count > 0)) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                NavigationManager.BackRequested -= OnBackRequested;
                NavigationManager.BackRequested += OnBackRequested;
            }
        }

        private void BackKey_OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            if (!DesignMode.DesignModeEnabled)
                NavigationManager.BackRequested -= OnBackRequested;
        }

        public void AddBackKeyAction(Action backKeyAction) => AddBackKeyAction(backKeyAction, BackKeyActions.Count);

        public void AddBackKeyAction(Action backKeyAction, Int32 index)
        {
            BackKeyActions.Insert(index, backKeyAction);
            NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        public void RemoveBackKeyAction(Action backKeyAction)
        {
            BackKeyActions.Remove(backKeyAction);
            if (BackKeyActions.Count == 0)
                NavigationManager.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(Object sender, BackRequestedEventArgs args)
        {
            args.Handled = (BackKeyActions.Count > 0) || Frame.CanGoBack;
            Back();
        }

        private void Back()
        {
            if (BackKeyActions.Count > 0)
            {
                var action = BackKeyActions[BackKeyActions.Count - 1];
                action.Invoke();
                RemoveBackKeyAction(action);
            }
            else
            {
                OnBackRequested();
            }
        }

        protected virtual void OnBackRequested()
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
