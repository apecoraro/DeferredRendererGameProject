using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class ComponentState
    {
        public Vector3 DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
        public float Alpha = 1.0f;

        public void Apply(BaseEffect effect)
        {
            effect.DiffuseColor = DiffuseColor;
            effect.Alpha = Alpha;
        }
    }
}
