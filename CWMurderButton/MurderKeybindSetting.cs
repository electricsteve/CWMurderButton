using UnityEngine;
using Zorro.Settings;

namespace CWMurderButton;

public class MurderKeybindSetting : KeyCodeSetting,IExposedSetting
{
    protected override KeyCode GetDefaultKey() => KeyCode.M;

    public SettingCategory GetSettingCategory() => SettingCategory.Controls;

    public string GetDisplayName() => "Murder";
}