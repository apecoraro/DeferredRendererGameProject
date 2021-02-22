using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public class ComponentData
    {
    }

    public class Drawable
    {
        public bool Enabled = true;
        public gfx.Component GfxComponent;
        public Matrix WorldTransform = Matrix.Identity;
        public VertexBuffer VertexBuffer = null;
        public IndexBuffer IndexBuffer = null;

        public Drawable(gfx.Component gfxComp)
        {
            GfxComponent = gfxComp;
        }

        public class DrawablePart
        {
            public Drawable Drawable;
            public ShadowMapEffect Effect;
            public VertexDeclaration VertexDeclaration;
            public int StreamOffset;
            public int VertexStride;
            public PrimitiveType PrimitiveType;
            public int BaseVertex;
            public int MinVertexIndex;
            public int NumVertices;
            public int StartIndex;
            public int PrimitiveCount;
            public ComponentData ComponentData = null;
            //TODO each component can store custom data needed to configure the effect
            //by subclassing this class

            public DrawablePart(Drawable drawable)
            {
                this.Drawable = drawable;
            }
        }

        public DrawablePart[] DrawableParts;
    }
}
