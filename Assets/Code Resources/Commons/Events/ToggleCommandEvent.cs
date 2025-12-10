using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resources
{
    public enum ToggleSettingType
    {
        SoundFX,
        Music,
        Haptic
    };

    public struct ToggleCommandEvent
    {
        
        public readonly ToggleSettingType settingType;
        public readonly bool isOn;

        public ToggleCommandEvent(ToggleSettingType settingType, bool isOn)
        {
            this.settingType = settingType;
            this.isOn = isOn;
        }
    }
}



