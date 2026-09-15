using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Keep uGUI editing, but fit an insertion before publishing it to the story.
// Configure without character validation or a character-count limit.
public class RM_TextInputField : InputField
{
    public Func<string, bool> Fits;
    public Action OnCrop;
    public Action OnActivate;
    public Action OnEscape;

    int? requestedCaret;

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable()) return;
        bool hadFocus = isFocused;
        OnActivate?.Invoke();
        base.OnPointerDown(eventData);
        if (hadFocus || readOnly) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform,
            eventData.position, eventData.pressEventCamera, out var position);
        FocusAt(GetCharacterIndexFromPosition(position) + m_DrawStart);
    }

    public void FocusAt(int position)
    {
        requestedCaret = position;
        ActivateInputField();
    }

    protected override void LateUpdate()
    {
        var correctedSelection = FitTouchKeyboard();
        base.LateUpdate();
        if (!isFocused) return;
        if (requestedCaret.HasValue)
        {
            caretPosition = requestedCaret.Value;
            requestedCaret = null;
        }
        if (correctedSelection.HasValue)
        {
            selectionAnchorPosition = correctedSelection.Value.start;
            selectionFocusPosition = correctedSelection.Value.end;
            UpdateLabel();
        }
    }

    public override void OnUpdateSelected(BaseEventData eventData)
    {
        // Native Escape restores the value from activation; Maker keeps accepted edits.
        if (isFocused && Input.GetKeyDown(KeyCode.Escape))
        {
            requestedCaret = null;
            DeactivateInputField();
            eventData.Use();
            OnEscape?.Invoke();
            return;
        }
        base.OnUpdateSelected(eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        requestedCaret = null;
        base.OnDeselect(eventData);
    }

    protected override void Append(char value)
    {
        // uGUI's keyboard path does not support surrogate key events. A pasted
        // string still keeps complete surrogate pairs and combining sequences.
        if (!char.IsSurrogate(value)) Append(value.ToString());
    }

    protected override void Append(string value)
    {
        if (readOnly || string.IsNullOrEmpty(value)) return;
        value = NormalizeNewlines(value);

        int start = Mathf.Min(m_CaretPosition, m_CaretSelectPosition);
        int end = Mathf.Max(m_CaretPosition, m_CaretSelectPosition);
        string before = text.Substring(0, start);
        string after = text.Substring(end);
        int acceptedLength = AcceptedPrefixLength(before, value, after);

        if (acceptedLength > 0)
        {
            string candidate = before + value.Substring(0, acceptedLength) + after;
            bool changed = candidate != text;
            SetTextWithoutNotify(candidate);
            caretPosition = start + acceptedLength;
            if (changed) onValueChanged.Invoke(text);
            UpdateLabel();
        }
        if (acceptedLength < value.Length) OnCrop?.Invoke();
    }

    // uGUI consumes native keyboard text directly in LateUpdate, bypassing Append.
    // Constrain that input first; the base method still owns applying/notifying it.
    RangeInt? FitTouchKeyboard()
    {
        if (!isFocused || readOnly || m_Keyboard == null || !TouchScreenKeyboard.isSupported
            || TouchScreenKeyboard.isInPlaceEditingAllowed) return null;
        string raw = m_Keyboard.text;
        if (raw == text) return null;
        ReplacementRange(raw, out int start, out int end, out int incomingEnd);
        string before = text.Substring(0, start);
        string after = text.Substring(end);
        string rawAddition = raw.Substring(start, incomingEnd - start);
        string addition = NormalizeNewlines(rawAddition);
        int acceptedLength = AcceptedPrefixLength(before, addition, after);
        bool cropped = acceptedLength < addition.Length;
        bool unchanged = cropped && acceptedLength == 0;
        string candidate = unchanged ? text : before + addition.Substring(0, acceptedLength) + after;
        if (candidate == raw) return null;

        RangeInt selection;
        if (unchanged)
            selection = new RangeInt(Mathf.Min(m_CaretPosition, m_CaretSelectPosition),
                Mathf.Abs(m_CaretPosition - m_CaretSelectPosition));
        else if (cropped || !m_Keyboard.canGetSelection)
            selection = new RangeInt(start + acceptedLength, 0);
        else
        {
            var original = m_Keyboard.selection;
            int anchor = NormalizedPosition(original.start, start, incomingEnd, rawAddition, addition.Length);
            int focus = NormalizedPosition(original.end, start, incomingEnd, rawAddition, addition.Length);
            selection = new RangeInt(anchor, focus - anchor);
        }
        m_Keyboard.text = candidate;
        if (m_Keyboard.canSetSelection) m_Keyboard.selection = selection;
        if (cropped) OnCrop?.Invoke();
        return selection;
    }

    void ReplacementRange(string incoming, out int start, out int end, out int incomingEnd)
    {
        start = Mathf.Min(m_CaretPosition, m_CaretSelectPosition);
        end = Mathf.Max(m_CaretPosition, m_CaretSelectPosition);
        incomingEnd = incoming.Length - (text.Length - end);

        // Prefer the preceding selection when it explains the edit. Autocorrection
        // may replace elsewhere, so derive its unchanged prefix/suffix instead.
        if (incomingEnd < start || !incoming.StartsWith(text.Substring(0, start), StringComparison.Ordinal)
            || !incoming.EndsWith(text.Substring(end), StringComparison.Ordinal))
        {
            start = 0;
            while (start < text.Length && start < incoming.Length && text[start] == incoming[start]) start++;
            end = text.Length;
            incomingEnd = incoming.Length;
            while (end > start && incomingEnd > start && text[end - 1] == incoming[incomingEnd - 1])
            {
                end--;
                incomingEnd--;
            }
        }

        // A shared UTF-16 prefix/suffix may end inside an accent or surrogate pair.
        // Include that complete element in the replacement on both sides.
        var previousElements = StringInfo.ParseCombiningCharacters(text);
        var incomingElements = StringInfo.ParseCombiningCharacters(incoming);
        while (!IsBoundary(previousElements, text.Length, start) || !IsBoundary(incomingElements, incoming.Length, start)) start--;
        while (!IsBoundary(previousElements, text.Length, end) || !IsBoundary(incomingElements, incoming.Length, incomingEnd))
        {
            end++;
            incomingEnd++;
        }
    }

    static bool IsBoundary(int[] elements, int length, int position) => position == length
        || Array.BinarySearch(elements, position) >= 0;

    static string NormalizeNewlines(string value) => value.Replace("\r\n", "\n").Replace('\r', '\n');

    static int NormalizedPosition(int position, int start, int end, string addition, int normalizedLength)
    {
        if (position <= start) return position;
        if (position >= end) return position + normalizedLength - addition.Length;
        return start + NormalizeNewlines(addition.Substring(0, position - start)).Length;
    }

    int AcceptedPrefixLength(string before, string value, string after)
    {
        var elements = StringInfo.GetTextElementEnumerator(value);
        int acceptedLength = 0;

        // Simulate typing from left to right: the first overflowing element
        // ends this insertion, without skipping ahead to later characters.
        while (elements.MoveNext())
        {
            string element = elements.GetTextElement();
            char c = element[0];
            if ((c < ' ' && c != '\n' && c != '\t') || c == '\u007f'
                || (!multiLine && (c == '\n' || c == '\t'))) break;
            int length = elements.ElementIndex + element.Length;
            if (!Fits(before + value.Substring(0, length) + after)) break;
            acceptedLength = length;
        }
        return acceptedLength;
    }
}
