using UnityEngine;

namespace Progress
{
    [CreateAssetMenu(menuName = "Game/Level")]
    public class LevelData : ScriptableObject
    {
        public int cellCount;

        public int[] initialState;
    }
}
