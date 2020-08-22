using UnityEngine;

namespace HexDeniz
{
    public enum SoundEffects
    {
        Button,
        Back,
        GameEnd
    }

    [RequireComponent(typeof(AudioSource))]
    public class SoundEffectManager : MonoBehaviour
    {
        public static SoundEffectManager Instance;
        private AudioSource source;
        public AudioClip ButtonSound, BackSound, GameEndSound;
        public bool SoundOn = true;

        void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
        }

        public void Play(SoundEffects eff)
        {
            if (!SoundOn)
                return;

            switch (eff)
            {
                case SoundEffects.Button:
                    Instance.source.PlayOneShot(ButtonSound);
                    return;
                case SoundEffects.Back:
                    Instance.source.PlayOneShot(BackSound);
                    return;
                case SoundEffects.GameEnd:
                    Instance.source.PlayOneShot(GameEndSound);
                    return;
            }
        }
    }
}