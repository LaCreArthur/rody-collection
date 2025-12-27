/*using UnityEngine;

public class ExamplePoolableMessageListener : MonoBehaviour, IPoolableComponent
{
    public void Spawned()
    {
        MessagingSystem.Instance.AttachListener(typeof(MyCustomMessage), this.HandleMyCustomMessage);
    }

    bool HandleMyCustomMessage(Message msg)
    {
        if (msg is MyCustomMessage castedMsg) 
            Debug.Log($"Got the message! {castedMsg.intValue}, {castedMsg.floatValue}");
        return true;
    }

    public void Despawned()
    {
        if (MessagingSystem.IsAlive)
        {
            MessagingSystem.Instance.DetachListener(typeof(MyCustomMessage), HandleMyCustomMessage);
        }
    }
}

public class MyCustomMessage : Message
{
    public readonly int intValue;
    public readonly float floatValue;

    public MyCustomMessage(int intValue, float floatValue)
    {
        this.intValue = intValue;
        this.floatValue = floatValue;
    }
}*/