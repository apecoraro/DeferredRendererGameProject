using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    class GridLines : gfx.LinesComponent
    {
        public GridLines(GraphicsDevice device) : base(device, 4) { }
        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            Vector3 min = gameObject.BoundingBox.Min;
            Vector3 max = gameObject.BoundingBox.Max;

            for (float x = min.X; x <= gameObject.BoundingBox.Max.X; x += 50.0f)
            {
                for (float z = min.Z; z <= gameObject.BoundingBox.Max.Z; z += 50.0f)
                {
                    Vector3 start = new Vector3(x, 1.0f, z);
                    Vector3 end = new Vector3(gameObject.BoundingBox.Max.X, 1.0f, z);
                    AddLine(start, end, Color.White);

                    Vector3 start2 = new Vector3(x, 1.0f, z);
                    Vector3 end2 = new Vector3(x, 1.0f, gameObject.BoundingBox.Max.Z);
                    AddLine(start2, end2, Color.White);
                }
            }

            base.OnAddToGameObject(gameObject);
        }
    }
}
