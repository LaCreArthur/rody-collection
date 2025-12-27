using UnityEngine;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.ScriptableObjects.Variables
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Basic Variable/Float")]
    public class FloatVariable : BaseVariable<float>
    {
        public override void Save()
        {
            EncryptedPlayerPrefs.SetFloat(this.name, v);
        }

        public override float Load()
        {
            v = EncryptedPlayerPrefs.GetFloat(this.name, defaultValue);
            return v;
        }
        
        public void Add(float x)
        {
            v += x;
        }
        
        public static FloatVariable operator ++(FloatVariable a)
        {
            a.v++;
            return a;
        }

        public static FloatVariable operator --(FloatVariable a)
        {
            a.v--;
            return a;
        }

        public static FloatVariable operator +(FloatVariable a)
        {
            return a;
        }

        public static FloatVariable operator +(FloatVariable a, FloatVariable b)
        {
            var res = CreateInstance<FloatVariable>();
            res.v = a.v + b.v;
            return res;
        }

        public static FloatVariable operator -(FloatVariable a)
        {
            a.v = -a.v;
            return a;
        }

        public static FloatVariable operator -(FloatVariable a, FloatVariable b)
        {
            var res = CreateInstance<FloatVariable>();
            res.v = a.v - b.v;
            return res;
        }
    }
}