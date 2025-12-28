using System.IO;
using UnityEngine;

/// <summary>
/// Centralized path management for user story storage.
/// All runtime story data is accessed via WorkingStory - this class
/// only handles persistent storage paths for import/export on desktop.
/// </summary>
public static class PathManager
{
    /// <summary>
    /// Root path for user-created stories (in persistent data).
    /// Desktop only - WebGL uses browser download/upload instead.
    /// </summary>
    public static string UserStoriesPath =>
        Path.Combine(Application.persistentDataPath, "UserStories");

    /// <summary>
    /// Ensures the UserStories directory exists.
    /// Call before listing or saving user stories.
    /// </summary>
    public static void EnsureUserStoriesDirectory()
    {
        if (!Directory.Exists(UserStoriesPath))
        {
            Directory.CreateDirectory(UserStoriesPath);
        }
    }
}
