public class RM_ObjectsLayout : RM_Layout
{
    public void RM_ReturnClick()
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.mainLayout);
        UnsetLayouts(gm.introTextObj, gm.title, gm.objectsLayout);
    }

    public void RM_ObjClick(int objective)
    {
        if (!gm.CanEdit) return;
        SetLayouts(gm.objLayout);
        UnsetLayouts(gm.introTextObj, gm.objectsLayout);
        gm.objLayout.GetComponent<RM_ObjLayout>().Bind(objective);
    }
}
