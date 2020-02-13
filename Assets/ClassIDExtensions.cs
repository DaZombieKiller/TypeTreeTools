public static class ClassIDExtensions
{
    public static bool IsSingletonType(this ClassID id)
    {
        switch (id)
        {
        case ClassID.MonoManager:
        case ClassID.AnnotationManager:
        case ClassID.InspectorExpandedState:
        case ClassID.SceneVisibilityState:
        case ClassID.SpriteAtlasDatabase:
            return true;
        default:
            return false;
        }
    }
}
