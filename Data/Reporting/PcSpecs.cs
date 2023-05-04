using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommunityTools.Data.Reporting
{
    public class PcSpecs
    {
        public string OS { get; set; }

        public string CPU { get; set; }

        public string GPU { get; set; }

        public string RAM { get; set; }

        public override string ToString()
        {
            return $" "
                + $"OS: {OS}"
                + $"CPU: {CPU}"
                + $"GPU: {GPU}"
                + $"RAM: {RAM}"
                + $"";
        }

    }
}
