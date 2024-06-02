namespace CWMurderButton;
using Zorro.Settings;

public class Patches
{
    public static GlobalInputHandler.InputKey MurderKey = new GlobalInputHandler.InputKey();

    internal static void init()
    {
        On.GlobalInputHandler.OnCreated += GlobalInputHandlerOnOnCreated;
        On.SettingsHandler.ctor += SettingsHandlerInit;
    }

    private static void GlobalInputHandlerOnOnCreated(On.GlobalInputHandler.orig_OnCreated orig, GlobalInputHandler self)
    {
        orig(self);
        MurderKey.SetKeybind((KeyCodeSetting) GameHandler.Instance.SettingsHandler.GetSetting<MurderKeybindSetting>());
    }

    private static void SettingsHandlerInit(On.SettingsHandler.orig_ctor orig, SettingsHandler self)
    {
        orig(self);
        MurderKeybindSetting murderKeybindSetting = new MurderKeybindSetting();
        murderKeybindSetting.Load(self._settingsSaveLoad);
        murderKeybindSetting.ApplyValue();
        self.settings.Add(murderKeybindSetting);
    }
}