using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DeskWars.com;

namespace DeskWars.gfx.com
{
    class MsgDraw : GameObjectMessage
    {
        public BaseEffect Effect;
        public Matrix WorldTransform;
        public Matrix ViewTransform;
        public Matrix ProjTransform;
        public GameTime GameTime;
    }
}
