using IPA.Utilities;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FeetSaberModifier
{
    public static class Config
    {
        private static BS_Utils.Utilities.Config _config = new BS_Utils.Utilities.Config(Plugin.Name);

        public static bool hideSabers = false;

        public static bool feetSaber = false;
        public static float feetNotesY = 0.12f;

        public static bool onTrackers = false;

        public static bool fourSabers = false;
        public static bool topNotesToFeet = false;
        public static bool middleNotesToFeet = false;
        public static bool bottomNotesToFeet = false;

        public static float avatarFootPosX = 0;
        public static float avatarFootPosY = 0;
        public static float avatarFootPosZ = 0;
        public static float avatarFootRotX = -90;
        public static float avatarFootRotY = 0;
        public static float avatarFootRotZ = 0;
        public static float trackerFootPosX = 0;
        public static float trackerFootPosY = 0;
        public static float trackerFootPosZ = 0;
        public static float trackerFootRotX = 0;
        public static float trackerFootRotY = 0;
        public static float trackerFootRotZ = 0;

        public static FileSystemWatcher watcher = new FileSystemWatcher(UnityGame.UserDataPath)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = Plugin.Name + ".ini",
            EnableRaisingEvents = true
        };

        private static bool _init;
        private static bool _ignoreConfigChanged;

        private static void Init()
        {
            watcher.Changed += OnConfigChanged;
            _init = true;
        }

        private static void OnConfigChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            //Logger.log.Debug("OnConfigChanged");
            if (!_ignoreConfigChanged)
            {
                Config.Read();
                ModifierUI.instance.updateUI();
            }
        }

        public static void Read()
        {
            if (!_init)
            {
                Init();
            }

            hideSabers = _config.GetBool(Plugin.Name, "hideSabers", false, true);

            feetSaber = _config.GetBool(Plugin.Name, "feetSaber", false, true);
            feetNotesY = _config.GetFloat(Plugin.Name, "feetNotesY", 0.12f, true);
            onTrackers = _config.GetBool(Plugin.Name, "onTrackers", false, true);
            ReadFeetPosRot();

            fourSabers = _config.GetBool(Plugin.Name, "fourSabers", false, true);
            topNotesToFeet = _config.GetBool(Plugin.Name, "topNotesToFeet", false, true);
            middleNotesToFeet = _config.GetBool(Plugin.Name, "middleNotesToFeet", false, true);
            bottomNotesToFeet = _config.GetBool(Plugin.Name, "bottomNotesToFeet", false, true);

        }

        public static void ReadFeetPosRot()
        {
            avatarFootPosX = _config.GetFloat(Plugin.Name, "avatarFootPosX", 0, true);
            avatarFootPosY = _config.GetFloat(Plugin.Name, "avatarFootPosY", 0, true);
            avatarFootPosZ = _config.GetFloat(Plugin.Name, "avatarFootPosZ", 0, true);

            avatarFootRotX = _config.GetFloat(Plugin.Name, "avatarFootRotX", 0, true);
            avatarFootRotY = _config.GetFloat(Plugin.Name, "avatarFootRotY", 0, true);
            avatarFootRotZ = _config.GetFloat(Plugin.Name, "avatarFootRotZ", 0, true);

            trackerFootPosX = _config.GetFloat(Plugin.Name, "trackerFootPosX", 0, true);
            trackerFootPosY = _config.GetFloat(Plugin.Name, "trackerFootPosY", 0, true);
            trackerFootPosZ = _config.GetFloat(Plugin.Name, "trackerFootPosZ", 0, true);

            trackerFootRotX = _config.GetFloat(Plugin.Name, "trackerFootRotX", 0, true);
            trackerFootRotY = _config.GetFloat(Plugin.Name, "trackerFootRotY", 0, true);
            trackerFootRotZ = _config.GetFloat(Plugin.Name, "trackerFootRotZ", 0, true);
        }

        public static void Write()
        {
            //PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(DisableWatcherTemporaryCoroutine());
            SharedCoroutineStarter.instance.StartCoroutine(DisableWatcherTemporaryCoroutine());


            _config.SetBool(Plugin.Name, "feetSaber", feetSaber);
            _config.SetBool(Plugin.Name, "hideSabers", hideSabers);
            _config.SetBool(Plugin.Name, "onTrackers", onTrackers);

            _config.SetBool(Plugin.Name, "fourSabers", fourSabers);
            _config.SetBool(Plugin.Name, "topNotesToFeet", topNotesToFeet);
            _config.SetBool(Plugin.Name, "middleNotesToFeet", middleNotesToFeet);
            _config.SetBool(Plugin.Name, "bottomNotesToFeet", bottomNotesToFeet);

        }

        private static IEnumerator DisableWatcherTemporaryCoroutine()
        {
            _ignoreConfigChanged = true;
            yield return new WaitForSecondsRealtime(1f);
            _ignoreConfigChanged = false;
        }
    }
}
