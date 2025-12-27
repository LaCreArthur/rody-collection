using System;
using UnityEngine;

namespace UnityReusables.Utils.Components
{
    public class SimpleLogComponent : MonoBehaviour
    {
        public enum Type
        {
            Log,
            Warning,
            Error,
            Exception
        }

        public Type LogType;

        public string Message;

        public void Log(string log)
        {
            LogBase(log);
        }

        public void Log()
        {
            LogBase(Message);
        }

        private void LogBase(string message)
        {
            switch (LogType)
            {
                case Type.Log:
                    Debug.Log(message);
                    break;
                case Type.Warning:
                    Debug.LogWarning(message);
                    break;
                case Type.Error:
                    Debug.LogError(message);
                    break;
                case Type.Exception:
                    Debug.LogException(new Exception(message));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}