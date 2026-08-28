using UnityEngine;

namespace CLGameToolkit.Gameplay
{
    public class MusicZone : TriggerZone
    {
        [SerializeField] private AudioClip Music;

        protected override void ExecuteTrigger(Collider other)
        {
            base.ExecuteTrigger(other);
            Play();
        }

        public void Play()
        {
            MusicPlayer.PlayMusic(Music, 2.5f, 1f, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (Tag != string.Empty && !other.CompareTag(Tag) || !enabled)
                return;

            if (MusicPlayer.CurrentClip == Music)
                MusicPlayer.PlayMusicPopHistory();
        }
    }
}
