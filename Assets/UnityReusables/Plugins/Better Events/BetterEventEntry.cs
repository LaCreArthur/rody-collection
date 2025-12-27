using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class BetterEventEntry : ISerializationCallbackReceiver
{
    public float delay;
    
    [NonSerialized]
    public Delegate Delegate;

    [NonSerialized]
    public object[] ParameterValues;

    public BetterEventEntry(Delegate del) 
    {
        if (del != null && del.Method != null)
        {
            this.Delegate = del;
            this.ParameterValues = new object[del.Method.GetParameters().Length];
        }
    }

    public void Invoke()
    {
        if (this.Delegate != null && this.ParameterValues != null)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                // This is faster than Dynamic Invoke.
                // Debug.Log($"BEE Target : {this.Delegate.Target} - Method : {this.Delegate.Method}", (Object) this.Delegate.Target);
                this.Delegate.Method.Invoke(this.Delegate.Target, this.ParameterValues); 
            });
        }
    } 

    #region OdinSerialization
    [SerializeField, HideInInspector]
    private List<UnityEngine.Object> unityReferences;

    [SerializeField, HideInInspector]
    private byte[] bytes;

    public void OnAfterDeserialize()
    {
        var val = SerializationUtility.DeserializeValue<OdinSerializedData>(this.bytes, DataFormat.Binary, this.unityReferences);
        this.Delegate = val.Delegate;
        this.ParameterValues = val.ParameterValues;
    }

    public void OnBeforeSerialize()
    {
        var val = new OdinSerializedData() { Delegate = this.Delegate, ParameterValues = this.ParameterValues };
        this.bytes = SerializationUtility.SerializeValue(val, DataFormat.Binary, out this.unityReferences);
    }

    private struct OdinSerializedData
    {
        public Delegate Delegate;
        public object[] ParameterValues;
    }
    #endregion
}
