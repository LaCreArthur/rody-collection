using UnityEngine;

/// <summary>
/// Firebase configuration for REST API access.
/// </summary>
public static class FirebaseConfig
{
    public const string ProjectId = "rody-maker";
    public const string ApiKey = "AIzaSyDaI4TeCPyylviBzob5TswJ9xsruU5huKo";
    public const string StorageBucket = "rody-maker.firebasestorage.app";

    // Firestore REST API base URL
    // Note: (default) needs to be URL-encoded for UnityWebRequest
    public static string FirestoreBaseUrl =>
        $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/%28default%29/documents";

    // Storage REST API base URL
    public static string StorageBaseUrl =>
        $"https://firebasestorage.googleapis.com/v0/b/{StorageBucket}/o";

    /// <summary>
    /// Gets the Firestore URL for a specific document path.
    /// Path segments with spaces will be URL-encoded.
    /// </summary>
    public static string GetDocumentUrl(string path)
    {
        string encodedPath = EncodePathSegments(path);
        return $"{FirestoreBaseUrl}/{encodedPath}?key={ApiKey}";
    }

    /// <summary>
    /// Gets the Firestore URL for a collection query.
    /// </summary>
    public static string GetCollectionUrl(string collectionPath)
    {
        string encodedPath = EncodePathSegments(collectionPath);
        return $"{FirestoreBaseUrl}/{encodedPath}?key={ApiKey}";
    }

    /// <summary>
    /// URL-encodes each segment of a path while preserving slashes.
    /// </summary>
    private static string EncodePathSegments(string path)
    {
        var segments = path.Split('/');
        for (int i = 0; i < segments.Length; i++)
        {
            segments[i] = UnityEngine.Networking.UnityWebRequest.EscapeURL(segments[i]);
        }
        return string.Join("/", segments);
    }

    /// <summary>
    /// Gets the Storage download URL for a file.
    /// </summary>
    public static string GetStorageDownloadUrl(string filePath)
    {
        // URL-encode the path (replace / with %2F)
        string encodedPath = UnityEngine.Networking.UnityWebRequest.EscapeURL(filePath);
        return $"{StorageBaseUrl}/{encodedPath}?alt=media";
    }

    /// <summary>
    /// Gets the Storage upload URL for a file.
    /// </summary>
    public static string GetStorageUploadUrl(string filePath)
    {
        string encodedPath = UnityEngine.Networking.UnityWebRequest.EscapeURL(filePath);
        return $"{StorageBaseUrl}/{encodedPath}";
    }
}
