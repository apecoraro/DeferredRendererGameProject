using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    class LookLines : gfx.LinesComponent
    {
        public LookLines(GraphicsDevice device) : base(device, 4) { }
        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            Vector3 origin = new Vector3(0.0f, gameObject.BoundingSphere.Center.Y, 0.0f);

            Vector3 look = origin + (gameObject.LookVector * gameObject.BoundingSphere.Radius * 1.5f);
            AddLine(origin, look, Color.White);

            Vector3 right = origin + (gameObject.RightVector * gameObject.BoundingSphere.Radius);
            AddLine(origin, right, Color.Black);

            base.OnAddToGameObject(gameObject);
        }
    }
}
