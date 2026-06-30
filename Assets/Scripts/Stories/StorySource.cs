/// <summary>
/// Provenance of a story. Read in exactly one place (the edit affordance);
/// it never branches a gameplay read path.
/// </summary>
public enum StorySource
{
    Builtin,
    User,
}
