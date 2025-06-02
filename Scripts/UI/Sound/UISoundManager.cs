using System;
using UnityEngine;

namespace CLGameToolkit.UI
{
    public class UISoundManager : MonoSingleton<UISoundManager>
    {
        [SerializeField] private UISoundSet[] SoundSets;

        private readonly CallThrottler throttler = new(.1f, true);

        public void Play(UISoundType type)
        {
            if (!throttler.CanCall())
                return;

            foreach (UISoundSet soundSet in SoundSets)
            {
                if (soundSet.Type == type)
                {
                    AudioManager.PlayUI(soundSet.Sound.Clip, soundSet.Sound.Volume, soundSet.Sound.Pitch);
                    return;
                }
            }
        }

        public enum UISoundType
        {
            None = 0,
            Select = 1,
            Hover = 2,
            Click = 3,
            Primary = 4,
            Secondary = 5,
        }

        [Serializable]
        public class UISoundSet
        {
            public UISoundType Type;
            public SoundContainer Sound;
        }
    }
}
