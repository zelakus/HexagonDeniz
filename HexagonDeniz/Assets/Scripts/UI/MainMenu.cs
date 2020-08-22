using UnityEngine;
using UnityEngine.UI;

namespace HexDeniz
{
    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance;

        public Button ContinueButton;
        public GameObject Game;

        private void Awake()
        {
            Instance = this;

            //Check if objects are set
            if (ContinueButton == null)
                throw new System.NullReferenceException("Menu ContinueButton is null");
            if (Game == null)
                throw new System.NullReferenceException("Menu Game is null");
        }

        private void Start()
        {
            ContinueButton.interactable = StatsManager.Instance.HasSave;
        }

        public void ShowMenu()
        {
            Game.SetActive(false);
            gameObject.SetActive(true);
        }

        public void MenuNewGame()
        {
            StatsManager.Instance.NewGame();
            Game.SetActive(true);
            gameObject.SetActive(false);
        }

        public void MenuContinue()
        {
            StatsManager.Instance.LoadGame();
            Game.SetActive(true);
            gameObject.SetActive(false);
        }

        public void MenuOptions()
        {
            //TODO
        }

        public void MenuAbout()
        {
            MessageBox.Show("About", "Demo game made for Vertigo Games.\nMusic by Kevin MecLeod");
        }
    }
}