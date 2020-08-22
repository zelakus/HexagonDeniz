using UnityEngine;
using UnityEngine.UI;

namespace HexDeniz
{
    public class Settings : MonoBehaviour
    {
        public Toggle MusicToggle;
        public AudioSource BGM;

        public void ResetHighscore()
        {
            StatsManager.Instance.ClearHighscore();
        }

        public void MusicToggleChanged()
        {
            SoundEffectManager.Instance.SoundOn = MusicToggle.isOn;
            if (MusicToggle.isOn)
                BGM.Play();
            else
                BGM.Pause();
        }

        public void OkayButton()
        {
            MainMenu.Instance.ShowMenu();
            SoundEffectManager.Instance.Play(SoundEffects.Button);
        }
    }
}