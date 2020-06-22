using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects.Extensions
{
    class PlayerExtended : Player
    {
        /// <summary>
        /// Inject this mod into  the game
        /// </summary>
        protected override void Start()
        {
            base.Start();
            new GameObject($"__{nameof(CommunityToolsModule)}__").AddComponent<CommunityToolsModule>();
        }
    }
}
