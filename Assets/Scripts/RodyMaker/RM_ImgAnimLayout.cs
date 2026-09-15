using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RM_ImgAnimLayout : RM_Layout
{
    public int offset;
    public Button[] frameBtn, removeFrameBtn;

    int previewedFrame = -1;
    Canvas canvas;

    protected override void Awake()
    {
        base.Awake();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (!gm.CanEdit) return;
        for (int i = 0; i < frameBtn.Length; i++)
        {
            bool focused = EventSystem.current.currentSelectedGameObject == frameBtn[i].gameObject;
            bool hovered = RectTransformUtility.RectangleContainsScreenPoint(
                frameBtn[i].GetComponent<RectTransform>(), Input.mousePosition, canvas.worldCamera);
            if ((focused || hovered) && i != previewedFrame)
            {
                PreviewFrame(i);
                return;
            }
        }
    }

    public void SetActiveBtn()
    {
        previewedFrame = -1;
        int count = StoryRoot.Session.DraftFrameCount(StoryRoot.Session.EditorSceneIndex) - 1;
        for (int i = 0; i < frameBtn.Length; i++)
        {
            frameBtn[i].interactable = i + offset <= count;
            removeFrameBtn[i].interactable = i + offset < count;
        }
    }

    public void ReturnClick()
    {
        if (!gm.CanEdit) return;
        gm.mainLayout.GetComponent<RM_MainLayout>().ShowBaseImage();
        gm.ShowPanel(RM_Panel.Images);
        gm.imagesLayout.GetComponent<RM_ImagesLayout>().SetActiveBtn();
    }

    public void PreviewFrame(int index)
    {
        if (!gm.CanEdit) return;
        int scene = StoryRoot.Session.EditorSceneIndex;
        int frame = index + offset + 2;
        if (frame > StoryRoot.Session.DraftFrameCount(scene)) return;
        previewedFrame = index;
        gm.scenePreview.sprite = StoryRoot.Session.LoadSprite(SpriteCache.SceneFrameName(scene, frame));
    }

    public void RemoveFrame(int index)
    {
        if (!gm.CanEdit) return;
        StoryRoot.Session.RemoveFrame(StoryRoot.Session.EditorSceneIndex, index + offset + 2);
        StoryRoot.FlushWorkspace();
        gm.Refresh();
        SetActiveBtn();
    }

    public void ImportClick(int index)
    {
        if (!gm.CanEdit) return;
        int scene = StoryRoot.Session.EditorSceneIndex;
        int frame = index + offset + 2;
        if (index < 0 || index >= frameBtn.Length || frame > StoryRoot.Session.DraftFrameCount(scene) + 1) return;
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", (data, error) =>
        {
            if (error != null) { StoryRoot.ShowMessage("L’image n’a pas pu être ouverte.\n" + error); return; }
            if (data == null) return;
            Texture2D texture = null;
            try
            {
                texture = WebGLFileBrowser.DataUrlToTexture(data);
                if (texture == null) throw new InvalidOperationException("Format d’image illisible.");
                RM_TextureScale.Point(texture, 320, 130);
                error = StoryRoot.Session.SaveSprite(SpriteCache.SceneFrameName(scene, frame), texture);
                if (error != null) throw new InvalidOperationException(error);
                StoryRoot.FlushWorkspace();
                gm.scenePreview.sprite = StoryRoot.Session.LoadSprite(SpriteCache.SceneFrameName(scene, frame));
                SetActiveBtn();
            }
            catch (Exception e) { StoryRoot.ShowMessage("L’image d’animation n’a pas été modifiée.\n" + e.Message); }
            finally { if (texture != null) Destroy(texture); }
        });
    }
}
