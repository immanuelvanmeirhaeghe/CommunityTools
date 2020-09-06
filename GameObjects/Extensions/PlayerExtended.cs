using UnityEngine;

namespace CommunityTools
{
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(CommunityTools)}__").AddComponent<CommunityTools>();
        }
    }
}
