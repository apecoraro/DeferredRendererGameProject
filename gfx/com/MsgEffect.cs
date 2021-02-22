using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.gfx.com
{
    public class MsgEffect : DeskWars.com.GameObjectMessage
    {
        public MsgEffect(string effectName)
        {
            EffectName = effectName;
        }

        public string EffectName;
        public bool Enable = true;
        //TODO add other things
    }
}
