using UnityEngine;

namespace CommunityTools.GameObjects
{
    class Topic : MonoBehaviour
    {
        /// <summary>
        /// Official game version
        /// </summary>
        public string GameVersion { get; set; }

        /// <summary>
        /// One sentence describing the issue
        /// </summary>
        public string Description { get; set; }

        public override string ToString()
        {
            return $"[{GameVersion}] - {Description}";
        }
    }
}
