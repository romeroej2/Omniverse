using Boko.Models;
using Boko.Utilities;
using ff14bot;
using ff14bot.AClasses;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ff14bot.Managers;

//using Kefka.Utilities;

#pragma warning disable 4014
#pragma warning disable CS1998

namespace Boko
{
    public class BokoPlugin
    {
        private static DateTime _pulseLimiter, _pulseLimiterInInstance;
        private static bool _initialized, _inInstance;
        //private static readonly string VersionPath = Path.Combine(Environment.CurrentDirectory, @"Plugins\Boko\version.txt");

        public BokoPlugin()
        {
            if (PluginManager.GetEnabledPlugins().Contains("Boko"))
            {
                OnInitialize();
            }
        }

        public static void OnButtonPress()
        {
            Logger.BokoLog(@"Opening Boko Settings!");
            FormManager.OpenForms();
        }

        public void OnInitialize()
        {
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
            //Logger.BokoLog($"Initializing Version: {File.ReadAllText(VersionPath)}");
            Logger.BokoLog($"Initializing Version: GitHub 1.0.0");

            FormManager.SaveFormInstances();
            _initialized = true;
        }

        private void OnBotStart(BotBase bot)
        {
            if (!_initialized) OnInitialize();

            Logger.BokoLog(@"Starting Boko!");
            FormManager.SaveFormInstances();
        }

        private void OnBotStop(BotBase bot)
        {
            Logger.BokoLog(@"Stopping Boko!");
            FormManager.SaveFormInstances();
        }

        public void OnShutdown()
        {
            FormManager.SaveFormInstances();
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
        }

        public void OnEnabled()
        {
            if (!_initialized) OnInitialize();

            FormManager.SaveFormInstances();
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
        }

        public void OnDisabled()
        {
            FormManager.SaveFormInstances();
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
        }

        public void OnPulse()
        {
            if (DateTime.Now < _pulseLimiter) return;
            _pulseLimiter = DateTime.Now.AddSeconds(1);

            if (!_initialized) OnInitialize();

            FormManager.SaveFormInstances();

            if (DateTime.Now > _pulseLimiterInInstance)
            {
                _pulseLimiterInInstance = DateTime.Now.AddSeconds(60);
                _inInstance = DutyManager.InInstance;
            }

            if (_inInstance) return;

            if (Core.Me.InCombat)
            {
                ChocoboManager.ChocoboCare();
            }
        }
    }
}