using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
 
public class XCodePostProcess
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
 
            string targetGuid = proj.GetUnityMainTargetGuid();
 
            foreach (var framework in new[] { targetGuid, proj.GetUnityFrameworkTargetGuid() })
            {
                proj.SetBuildProperty(framework, "ENABLE_BITCODE", "NO");
                proj.SetBuildProperty(framework, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");
                proj.SetBuildProperty(framework, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
                proj.SetBuildProperty(framework, "SWIFT_VERSION", "5.0");
            }
 
            proj.WriteToFile(projPath);
        }
    }
}