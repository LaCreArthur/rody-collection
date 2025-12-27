using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Useful to be used in callbacks & BetterEvent
 */
namespace UnityReusables.Managers
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Managers/Socials Manager")]
    public class SocialsManagerSO : ScriptableObject
    {
        public string twitterName = "LaCreArthur";
        public string facebookName = "BretzelStudio";
        public string youtubeName = "UC56vS-xXi_y2DICsHsZJV8w";
        public string instagramName = "lacrearthur";
        
        public enum Social
        {
            Twitter, Facebook, Youtube, Instagram
        }
        
        public void OpenSocial(Social social)
        {
            switch (social)
            {
                case Social.Twitter:
                    Application.OpenURL($"https://twitter.com/{twitterName}");
                    break;
                case Social.Facebook:
                    Application.OpenURL($"https://www.facebook.com/{facebookName}");
                    break;
                case Social.Youtube:
                    Application.OpenURL($"https://www.youtube.com/channel/{youtubeName}");
                    break;
                case Social.Instagram:
                    Application.OpenURL($"https://www.instagram.com/{instagramName}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(social), social, null);
            }
        }
    }
}