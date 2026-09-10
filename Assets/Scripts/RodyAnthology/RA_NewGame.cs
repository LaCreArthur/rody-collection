using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Prepares a new-story candidate; only StoryRoot may replace the workspace.</summary>
public class RA_NewGame : MonoBehaviour
{
    public InputField titleInput;
    public InputField imgInput;
    public GameObject newGamePanel;
    public Button buttonAccept, buttonCancel;
    Texture2D _cover;

    void Start() => titleInput.characterValidation = InputField.CharacterValidation.Name;

    void Update()
    {
        buttonAccept.interactable = StoryRoot.IsReady && !StoryRoot.IsBusy && !string.IsNullOrWhiteSpace(titleInput.text);
        buttonCancel.interactable = !StoryRoot.IsBusy;
    }

    public void NG_OnAcceptClick()
    {
        if (StoryRoot.IsBusy || !StoryRoot.IsReady || string.IsNullOrWhiteSpace(titleInput.text)) return;
        var candidate = StorySession.CreateNewStory(titleInput.text.Trim());
        if (_cover != null) candidate.sprites[SpriteCache.CoverName] = Convert.ToBase64String(_cover.EncodeToPNG());
        StoryRoot.RequestWorkspace(candidate);
    }

    public void NG_OnCancelClick()
    {
        if (!StoryRoot.IsBusy) newGamePanel.SetActive(false);
    }

    public void NG_ImgClick()
    {
        if (StoryRoot.IsBusy) return;
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", (data, error) =>
        {
            if (error != null) { StoryRoot.ShowMessage(error); return; }
            if (data == null) return;
            Texture2D texture = null;
            try
            {
                texture = WebGLFileBrowser.DataUrlToTexture(data);
                if (texture == null) throw new InvalidOperationException("Cette image ne peut pas être ouverte.");
                RM_TextureScale.Point(texture, 320, 200);
                AtariPalette.ApplyPalette(texture);
                if (_cover != null) Destroy(_cover);
                _cover = texture;
                imgInput.text = "Image choisie";
            }
            catch (Exception e) { if (texture != null) Destroy(texture); StoryRoot.ShowMessage(e.Message); }
        });
    }

    void OnDestroy() { if (_cover != null) Destroy(_cover); }
}
