/*
 using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ExampleMessagesManagerComponent : MonoBehaviour
{
    [SerializeField] private List<GameObject> _examples = new List<GameObject>();
    [SerializeField] private GameObject _examplePrefab = default;
    private readonly string[] _names = {"Kiki", "Chatouille", "Prout"};

    private void Start()
    {
        MessagingSystem.Instance.AttachListener(typeof(CreateExampleMessage), HandleCreateExample);
    }

    private bool HandleCreateExample(Message msg)
    {
        GameObject exampleObject = Instantiate(_examplePrefab, 5.0f * Random.insideUnitSphere, Quaternion.identity);
        Debug.Log("a new CreateExampleMessage was handle", this);
        string exampleName = _names[Random.Range(0, _names.Length)];
        exampleObject.name = exampleName;
        _examples.Add(exampleObject);
        MessagingSystem.Instance.QueueMessage(new ExampleCreatedMessage(exampleObject, exampleName));
        return true;
    }

    private void OnDestroy()
    {
        if (MessagingSystem.IsAlive)
        {
            MessagingSystem.Instance.DetachListener(typeof(ExampleCreatedMessage), HandleCreateExample);
        }
    }
}

public class CreateExampleMessage : Message {}

public class ExampleCreatedMessage : Message
{
    public readonly GameObject exampleObject;
    public readonly string exampleName;

    public ExampleCreatedMessage(GameObject exampleObject, string exampleName)
    {
        this.exampleObject = exampleObject;
        this.exampleName = exampleName;
    }
}
*/