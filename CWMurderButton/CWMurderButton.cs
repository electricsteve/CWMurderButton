global using Plugin = CWMurderButton.CWMurderButton;
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using UnityEngine;
using Photon;
using Steamworks;
using UnityEngine.SceneManagement;

namespace CWMurderButton;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CWMurderButton : BaseUnityPlugin
{
    public static CWMurderButton Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    private bool MenuVisible = false;
    private int selectedIndex = 0;
    private Rect windowRect = new Rect(100f, 100f, 400f, 300f);
    private GUIStyle buttonStyle;
    private GUIStyle labelStyle;
    private GUIStyle windowStyle;
    private bool stylesInitialized = false;
    private List<Player> players;


    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        Logger.LogDebug("Hooking...");
        Patches.init();
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        Logger.LogDebug("Finished Hooking!");

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        MenuVisible = false;
    }

    private void OnDestroy()
    {
        Logger.LogDebug("Unhooking...");

        /*
         *  HookEndpointManager is from MonoMod.RuntimeDetour.HookGen, and is used by the MMHOOK assemblies.
         *  We can unhook all methods hooked with HookGen using this.
         *  Or we can unsubscribe specific patch methods with 'On.Namespace.Type.Method -= CustomMethod;'
         */
        HookEndpointManager.RemoveAllOwnedBy(Assembly.GetExecutingAssembly());
        SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;

        Logger.LogDebug("Finished Unhooking!");
    }

    private void Update()
    {
        if (Patches.MurderKey.GetKeyDown() && SurfaceNetworkHandler.HasStarted)
        {
            MenuVisible = !MenuVisible;
        }

        if (MenuVisible)
        {
            HandleKeyboardInput();
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex += 1;
            if (selectedIndex >= players.Count) selectedIndex = 0;
        } else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex -= 1;
            if (selectedIndex < 0) selectedIndex = players.Count - 1;
        } else if (Input.GetKeyDown(KeyCode.Return))
        {
            players[selectedIndex].CallDie();
        }
    }

    private void OnGUI()
    {
        if (MenuVisible)
        {
            if (!stylesInitialized)
            {
                InitializeGUIStyles();
                Logger.LogDebug("Bitch it should init style now.");
            }
            windowRect = GUILayout.Window(0, windowRect, new GUI.WindowFunction(DrawMenu), "Who do you want to murder?", windowStyle, Array.Empty<GUILayoutOption>());
        }
    }

    private void InitializeGUIStyles()
    {
        GUIStyle val = new GUIStyle(GUI.skin.window)
        {
            padding = new RectOffset(10, 10, 25, 10)
        };
        val.normal.background = MakeTex(600, 400, new Color(0.15f, 0.15f, 0.15f, 0.85f));
        windowStyle = val;
        GUIStyle val3 = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        val3.normal.textColor = Color.white;
        val3.normal.background = MakeTex(1, 1, new Color(0.25f, 0.25f, 0.25f, 0.85f));
        val3.margin = new RectOffset(5, 5, 5, 5);
        val3.padding = new RectOffset(10, 10, 10, 10);
        buttonStyle = val3;
        GUIStyle val4 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        val4.normal.textColor = Color.white;
        val4.margin = new RectOffset(5, 5, 5, 5);
        val4.padding = new RectOffset(10, 10, 10, 10);
        labelStyle = val4;
        stylesInitialized = true;
        Logger.LogDebug("Initialized gui styles.");
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] array = new Color[width * height];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = col;
        }
        Texture2D val = new Texture2D(width, height);
        val.SetPixels(array);
        val.Apply();
        return val;
    }

    private void DrawMenu(int windowID)
    {
        if (!stylesInitialized)
        {
            InitializeGUIStyles();
            stylesInitialized = true;
        }
        GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
        players = PlayerHandler.instance.playersAlive;
        foreach (Player player in players)
        {
            SteamAvatarHandler.TryGetSteamIDForPlayer(player.refs.view.Owner, out var _steamId);
            string playerName = SteamFriends.GetFriendPersonaName(_steamId);
            GUIStyle val = ((selectedIndex == players.IndexOf(player)) ? buttonStyle : labelStyle);
            GUILayout.Button(playerName, val);
        }
        GUILayout.EndVertical();
    }
}