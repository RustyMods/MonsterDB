using BepInEx;
using BepInEx.Configuration;

namespace SleepTime;

class AcceptableShortcuts() : AcceptableValueBase(typeof(KeyboardShortcut))
{
    public override object Clamp(object value) => value;
    public override bool IsValid(object value) => true;

    public override string ToDescriptionString() =>
        "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
}