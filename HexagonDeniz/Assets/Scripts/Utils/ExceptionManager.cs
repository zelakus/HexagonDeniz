using UnityEngine;

namespace HexDeniz
{
    public class ExceptionManager : MonoBehaviour
    {
        void Awake()
        {
#if !UNITY_EDITOR
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
#endif
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        int times = 0;

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            //Just incase if we start throwing errors inside a loop or Update method, better not get more than 5 errors
            if (times > 5)
                return;
            times++;

            if (type != LogType.Log && type != LogType.Warning)
            {
                string error = logString + "\n" + stackTrace + "\n";
                MessageBox.Show("Error", error);
            }
        }
    }
}