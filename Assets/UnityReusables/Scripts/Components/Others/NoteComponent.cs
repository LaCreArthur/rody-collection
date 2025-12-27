using UnityEngine;

namespace UnityReusables.Utils.Components
{
    public class NoteComponent : MonoBehaviour
    {
        [TextArea] [SerializeField] private string note;
    }
}
