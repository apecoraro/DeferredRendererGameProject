using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.com;

namespace DeskWars.gfx.com
{
    public class MsgAnimate : GameObjectMessage
    {
        public string Animation;
        public float SpeedFactor = 1.0f;
        public bool Loop;
    }
}
