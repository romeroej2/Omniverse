using System;
using System.Threading;
using Syntgoht.Models;
using Syntgoht.Utilities;
using Syntgoht.ViewModels;
using Syntgoht.Views;
using ff14bot;
using ff14bot.Navigation;
using TreeSharp;
using HotkeyManager = Syntgoht.Utilities.HotkeyManager;

#pragma warning disable 4014
#pragma warning disable CS1998

namespace Syntgoht
{
    public class Syntgoht
    {
        private SyntgohtWindow _form;
        private static int rbVersion;
        private static DateTime pulseLimiter;

        public Syntgoht()
        {
        }

        public static void OnButtonPress()
        {
            FormManager.OpenForms();
        }

        public void OnInitialize(int version)
        {
            Logger.SyntgohtLog("Initializing version: {0}", version);

            rbVersion = version;

            FormManager.SaveFormInstances();
        }

        public void OnShutdown()
        {
            Navigator.PlayerMover = new NullMover();
            var g = Navigator.NavigationProvider as GaiaNavigator;
            g?.Dispose();
            Navigator.NavigationProvider = new NullProvider();
            _root = null;

            HotkeyManager.UnregisterAllHotkeys();
            TreeRoot.TicksPerSecond = 30;
        }

        public Composite GetRoot()
        {
            return Root;
        }

        private Composite _root;

        private Composite Root
        {
            get
            {
                return _root ?? (_root = new Decorator(r => TreeTick()
                                && !MainSettingsModel.Instance.UsePause,
                                new PrioritySelector(DesynthLogic.Execute())));
            }
        }

        public void Start()
        {
            if (Navigator.NavigationProvider == null)
            {
                Navigator.NavigationProvider = new GaiaNavigator();
            }

            Navigator.PlayerMover = new SlideMover();
            Logger.SyntgohtLog(@"Starting Syntgoht!");

            HotkeyManager.RegisterHotkeys();

            TreeRoot.TicksPerSecond = 1;
        }

        public void Stop()
        {
            Navigator.PlayerMover = new NullMover();
            var g = Navigator.NavigationProvider as GaiaNavigator;
            g?.Dispose();
            Navigator.NavigationProvider = new NullProvider();
            _root = null;

            HotkeyManager.UnregisterAllHotkeys();
            TreeRoot.TicksPerSecond = 30;
        }

        private static bool TreeTick()
        {
            if (!TreeRoot.IsRunning) return false;
            OverlayViewModel.Instance.IsPausedString = MainSettingsModel.Instance.UsePause ? "Syntgoht Paused" : "Syntgoht Unpaused";

            if (DesynthLogic.Done)
            {
                TreeRoot.Stop();
                Thread.Sleep(500);
            }

            return true;
        }
    }
}