using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DeskWars.gfx.ps;

namespace DeskWars.gfx.com
{
    public class MsgBillboard : DeskWars.com.GameObjectMessage
    {
        public bool Enable = false;
        public List<Billboard.BillboardData> Billboards = new List<Billboard.BillboardData>();
    }
}
