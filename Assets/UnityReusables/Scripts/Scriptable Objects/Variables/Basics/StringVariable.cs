using UnityEngine;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Basic Variable/String")]
    public class StringVariable : BaseVariable<string>
    {
        public override void Save()
        {
            EncryptedPlayerPrefs.SetString(this.name, v);
        }

        public override string Load()
        {
            v = EncryptedPlayerPrefs.GetString(this.name, defaultValue);
            return v;
        }
    }
}