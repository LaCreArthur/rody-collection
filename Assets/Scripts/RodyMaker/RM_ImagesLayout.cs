using System;
using UnityEngine;
using UnityEngine.UI;

public class RM_ImagesLayout : RM_Layout
{
    public Button imgAnimBtn1, imgAnimBtn2;

    public void SetActiveBtn()
    {
        int scene = StoryRoot.Session.EditorSceneIndex;
        imgAnimBtn1.interactable = scene != 0;
        imgAnimBtn2.interactable = scene != 0 && StoryRoot.Session.DraftFrameCount(scene) >= 4;
    }

    public void ReturnClick()
    {
        if (!gm.CanEdit) return;
        gm.mainLayout.GetComponent<RM_MainLayout>().ShowBaseImage();
        SetLayouts(gm.mainLayout);
        UnsetLayouts(gm.imagesLayout);
    }

    public void ImgAnimClick(bool isSecond)
    {
        if (!gm.CanEdit || StoryRoot.Session.EditorSceneIndex == 0) return;
        SetLayouts(gm.imgAnimLayout);
        UnsetLayouts(gm.imagesLayout);
        var animation = gm.imgAnimLayout.GetComponent<RM_ImgAnimLayout>();
        animation.offset = isSecond ? 3 : 0;
        animation.SetActiveBtn();
    }

    public void ImportClick()
    {
        if (!gm.CanEdit) return;
        int scene = StoryRoot.Session.EditorSceneIndex;
        WebGLFileBrowser.Instance.OpenImageAsBase64("image/png,image/jpeg", (data, error) =>
        {
            if (error != null) { StoryRoot.ShowMessage("L’image n’a pas pu être ouverte.\n" + error); return; }
            if (data == null) return;
            Texture2D texture = null;
            try
            {
                texture = WebGLFileBrowser.DataUrlToTexture(data);
                if (texture == null) throw new InvalidOperationException("Format d’image illisible.");
                RM_TextureScale.Point(texture, 320, scene == 0 ? 200 : 130);
                error = StoryRoot.Session.SaveSprite(scene == 0 ? SpriteCache.TitleName : SpriteCache.SceneFrameName(scene, 1), texture);
                if (error != null) throw new InvalidOperationException(error);
                StoryRoot.FlushWorkspace();
                gm.mainLayout.GetComponent<RM_MainLayout>().LoadSprites();
            }
            catch (Exception e) { StoryRoot.ShowMessage("L’image n’a pas été modifiée.\n" + e.Message); }
            finally { if (texture != null) Destroy(texture); }
        });
    }
}
