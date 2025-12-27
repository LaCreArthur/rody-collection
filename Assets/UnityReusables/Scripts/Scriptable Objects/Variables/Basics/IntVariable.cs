using UnityEngine;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Basic Variable/Int")]
    public class IntVariable : BaseVariable<int>
    {
        public override void Save()
        {
            EncryptedPlayerPrefs.SetInt(this.name, v);
        }

        public override int Load()
        {
            v = EncryptedPlayerPrefs.GetInt(this.name, defaultValue);
            return v;
        }
        
        public void Add(int x)
        {
            v += x;
        }

        public static IntVariable operator ++(IntVariable a)
        {
            a.v++;
            return a;
        }

        public static IntVariable operator --(IntVariable a)
        {
            a.v--;
            return a;
        }

        public static IntVariable operator +(IntVariable a)
        {
            return a;
        }

        public static IntVariable operator +(IntVariable a, IntVariable b)
        {
            var res = CreateInstance<IntVariable>();
            res.v = a.v + b.v;
            return res;
        }

        public static IntVariable operator -(IntVariable a)
        {
            a.v = -a.v;
            return a;
        }

        public static IntVariable operator -(IntVariable a, IntVariable b)
        {
            var res = CreateInstance<IntVariable>();
            res.v = a.v - b.v;
            return res;
        }
    }
}