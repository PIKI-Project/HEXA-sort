using UnityEngine;

namespace HexaSort.Settings
{
    [CreateAssetMenu(fileName = "FirebaseConfig", menuName = "HexaSort/Firebase Config")]
    public class FirebaseConfig : ScriptableObject
    {
        public string apiKey;
        public string projectId;
    }
}