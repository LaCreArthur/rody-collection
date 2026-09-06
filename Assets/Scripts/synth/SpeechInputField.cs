using UnityEngine.UI;

// Apply a requested caret after uGUI completes its deferred activation/select-all.
public class SpeechInputField : InputField
{
    int? requestedCaret;

    public void FocusAt(int position)
    {
        requestedCaret = position;
        ActivateInputField();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (!isFocused || !requestedCaret.HasValue) return;
        caretPosition = requestedCaret.Value;
        requestedCaret = null;
    }
}
