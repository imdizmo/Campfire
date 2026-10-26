using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Jatify.Campfire
{
    // Replaces Valheim's character select with a WoW-style one: up to four characters stand at the
    // campfire, the list of all characters sits on the right, and worlds / servers on the left of the
    // same screen. Every save operation (start, join, create, delete) is Valheim's own code; the mod
    // only collects the choices. Before Valheim deletes a character or world, the files are copied
    // to <save dir>/campfire-backups, and nothing is deleted if that copy fails.
    // The look is the Jatify design's: Jatify UI's own when it is installed (found at runtime, JatifyLook), the
    // design's colours with Valheim's fonts otherwise.
    [BepInPlugin(Guid, "Campfire", "1.1.0")]
    [BepInDependency("jatify.ui", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "jatify.campfire";
        internal static Plugin Instance;
        internal static ConfigEntry<string> Order, Slots;
        internal static ConfigEntry<float> CamDistance;
        internal static ConfigEntry<string> LastTab, LastWorld, LastServer, LastCharacter;
        internal static ConfigEntry<bool> HostServer, Crossplay, Community, RememberPasswords;

        private void Awake()
        {
            Instance = this;
            Log.Bind(Logger);
            SaveBackup.DataDir = Application.persistentDataPath;
            Order = Config.Bind("Characters", "Order", "", "Character file names in list order (comma separated); the first four stand at the fire. Set by dragging cards or their up/down arrows.");
            Slots = Config.Bind("Characters", "Slots", "0,0; -1.7,1.4; -3.8,2.1; -5.6,0.4",
                "Where the four characters stand: metres right (+) / left (-) and away from the camera (+), relative to Valheim's own character spot. Ctrl+R in the menu re-reads this file.");
            CamDistance = Config.Bind("Characters", "CameraDistance", 0.85f, "Camera distance to the selected character, as a share of Valheim's own preview distance.");
            LastTab = Config.Bind("Remembered", "Tab", "worlds", "Worlds or servers: the tab the screen opens on (the last one you entered from).");
            LastWorld = Config.Bind("Remembered", "World", "", "File name of the last world you entered.");
            LastServer = Config.Bind("Remembered", "Server", "", "Address of the last server you joined.");
            LastCharacter = Config.Bind("Remembered", "Character", "", "File name of the last character you played.");
            HostServer = Config.Bind("Worlds", "HostServer", true, "Remembered 'Host server' switch.");
            Crossplay = Config.Bind("Worlds", "Crossplay", false, "Remembered 'Crossplay' switch.");
            RememberPasswords = Config.Bind("Servers", "RememberPasswords", true, "Remember server passwords (encrypted in Valheim's prefs) and fill them in when joining.");
            Community = Config.Bind("Worlds", "Community", false, "Remembered 'Community server' switch.");
            // One id per load: on a ScriptEngine reload the old copy unpatches (OnDestroy) after the new one patched.
            _harmony = new Harmony(Guid + "." + System.Guid.NewGuid().ToString("N"));
            _harmony.PatchAll(typeof(Plugin).Assembly);
            _reshow = FejdStartup.instance != null && FejdStartup.instance.m_selectCharacterPanel.activeSelf;   // reloaded on the character screen
        }

        private Harmony _harmony;
        private bool _reshow;

        // A reload (ScriptEngine) takes everything of ours away and gives Valheim its screen back.
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            bool shown = CharScreen.Visible || NewCharView.Visible, making = NewCharView.Visible || (FejdStartup.instance != null && FejdStartup.instance.m_newCharacterPanel.activeSelf);
            NewCharView.Hide();
            if (making && FejdStartup.instance != null) FejdStartup.instance.OnNewCharacterCancel();   // reloaded mid New character: back to the list, not both at once
            CharScreen.Unload();
            PasswordPrompt.Unload();
            LoadBar.Unload();
            LoadingArt.Unload();
            if (shown && FejdStartup.instance != null) FejdStartup.instance.m_selectCharacterPanel.SetActive(true);
        }

        private void Update()
        {
            if (_reshow)
            {
                _reshow = false;
                var f = FejdStartup.instance;
                if (f != null && f.m_selectCharacterPanel.activeSelf && SaveSystem.GetAllPlayerProfiles().Count > 0) { f.m_selectCharacterPanel.SetActive(false); CharScreen.Show(); }
            }
            Passwords.Tick();
            PasswordPrompt.Tick();
            LoadBar.Tick();
            if (NewCharView.Visible) { CharScreen.HideForeign(); NewCharView.Tick(); }
            Fejd.TitleTick();
            if (!CharScreen.Visible) return;
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.R))
            {
                Config.Reload();
                Party.Rebuild();
                Log.Info("config re-read, party re-placed");
            }
            CharScreen.Tick();
        }
    }
}
