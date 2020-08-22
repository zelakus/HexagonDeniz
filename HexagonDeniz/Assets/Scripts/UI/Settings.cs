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
            if (MusicToggle.isOn)
                BGM.Play();
            else
                BGM.Pause();
        }

        public void OkayButton()
        {
            MainMenu.Instance.ShowMenu();
        }
    }
}