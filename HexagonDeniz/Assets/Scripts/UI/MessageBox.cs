using TMPro;
using UnityEngine;

namespace HexDeniz
{
    public class MessageBox : MonoBehaviour
    {
        private static MessageBox Instance;

        public TMP_Text Title;
        public TMP_Text Message;
        
        private void Awake()
        {
            Instance = this;
        }

        public static void Show(string title, string msg)
        {
            Instance.Title.SetText(title);
            Instance.Message.SetText(msg);
            Instance.transform.GetChild(0).gameObject.SetActive(true);
        }

        public void CloseMessage()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}