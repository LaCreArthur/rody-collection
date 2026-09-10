using UnityEngine.UI;

public class RM_DialoguesLayout : RM_Layout
{
    public Button dial1Btn, dial2Btn, dial3Btn;

    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.introLayout);
        UnsetLayouts(gm.dialoguesLayout, gm.title);
    }

    public void RM_DialClick(int dial)
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.dialLayout);
        UnsetLayouts(gm.dialoguesLayout, gm.introTextObj);
        gm.dialLayout.GetComponent<RM_DialLayout>().Bind(dial);
    }

    public void SetDialButtons()
    {
        dial2Btn.interactable = !string.IsNullOrEmpty(gm.CurrentScene.texts.intro1);
        dial3Btn.interactable = !string.IsNullOrEmpty(gm.CurrentScene.texts.intro2);
    }
}
