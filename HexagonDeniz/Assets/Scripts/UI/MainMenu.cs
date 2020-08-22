using UnityEngine;
using UnityEngine.UI;

namespace HexDeniz
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        public Button ContinueButton;
        public GameObject Game;
        public GameObject Settings;

        private void Awake()
        {
            Instance = this;

            //Check if objects are set
            if (ContinueButton == null)
                throw new System.NullReferenceException("Menu ContinueButton is null");
            if (Game == null)
                throw new System.NullReferenceException("Menu Game is null");
        }

        private void OnEnable()
        {
            ContinueButton.interactable = StatsManager.Instance.HasSave;
        }

        public void GoBackToMenu()
        {
            SoundEffectManager.Instance.Play(SoundEffects.Back);
            ShowMenu();
        }

        public void ShowMenu()
        {
            Game.SetActive(false);
            Settings.SetActive(false);
            gameObject.SetActive(true);
        }

        public void MenuNewGame()
        {
            StatsManager.Instance.NewGame();
            Game.SetActive(true);
            Settings.SetActive(false);
            gameObject.SetActive(false);
            SoundEffectManager.Instance.Play(SoundEffects.Button);
        }

        public void MenuContinue()
        {
            StatsManager.Instance.LoadGame();
            Game.SetActive(true);
            Settings.SetActive(false);
            gameObject.SetActive(false);
            SoundEffectManager.Instance.Play(SoundEffects.Button);
        }

        public void MenuOptions()
        {
            Game.SetActive(false);
            Settings.SetActive(true);
            gameObject.SetActive(false);
            SoundEffectManager.Instance.Play(SoundEffects.Button);
        }

        public void MenuAbout()
        {
            MessageBox.Show("About!", "A demo game made for Vertigo Games.\nMusic by\nKevin MecLeod");
            SoundEffectManager.Instance.Play(SoundEffects.Button);
        }
    }
}