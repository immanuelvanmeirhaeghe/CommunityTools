using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools
{
    /// <summary>
    /// Inject modding interface into game
    /// </summary>
    class PlayerExtended : Player
    {
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(CommunityToolsMod)}__").AddComponent<CommunityToolsMod>();
        }
    }
}
