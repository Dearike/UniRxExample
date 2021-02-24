namespace Planner.Authentication.Models
{
    public enum UserRight
    {
        None = 1,
        AddObject = 1,
        EditObjects = 2,
        ViewApartmentProject = 3,
        ViewFloorProject = 4,
        AddToTemplate = 6,
        Generate3DTour = 7,
        ShowRenderGallery = 8,
        AddApartment = 9,
        EditApartment = 10,
        AddFloor = 11,
        EditFloor = 12,
        SvgExport = 14,
        ObjExport = 15,
        Render = 16,
        FullArchive = 17,
        SetProjectPriority = 18
    }
}