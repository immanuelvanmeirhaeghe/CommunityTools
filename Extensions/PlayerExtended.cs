using CommunityTools.Managers;
using UnityEngine;

namespace CommunityTools.Extensions
{
    public class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(CommunityTools)}__").AddComponent<CommunityTools>();
            new GameObject($"__{nameof(StylingManager)}__").AddComponent<StylingManager>();
        }
    }
}
