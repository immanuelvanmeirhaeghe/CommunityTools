using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.GameObjects
{
    class MapCoordinates : MonoBehaviour
    {
        public int GpsLat { get; set; }

        public int GpsLong { get; set; }

        public override string ToString()
        {
            return $"GPS Watch: Latitude: {GpsLat.ToString()}, Longitude {GpsLong.ToString()}";
        }
    }
}
