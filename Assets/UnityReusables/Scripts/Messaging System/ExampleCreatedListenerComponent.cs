/*
using UnityEngine;

public class ExampleCreatedListenerComponent : MonoBehaviour
{
    private void Start()
    {
        MessagingSystem.Instance.AttachListener(typeof(ExampleCreatedMessage), HandleExampleCreatedMessage);
    }

    private bool HandleExampleCreatedMessage(Message message)
    {
        if (!(message is ExampleCreatedMessage castMsg)) return false;
        Debug.Log($"A new ExampleCreatedMessage was handle! {castMsg.exampleName}");
        return true;
    }
}
*/
