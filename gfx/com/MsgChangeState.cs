using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.gfx.com
{
    class MsgChangeState : DeskWars.com.GameObjectMessage
    {
        //set to null to cause component to return to
        //default state
        public ComponentState NewState;
        public float TimeInSeconds = -1.0f;
    }
}
