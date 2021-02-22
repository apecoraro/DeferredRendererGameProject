using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class PlaneComponent : gfx.Component
    {
        #region Private Members

        Quad _quad;
        VertexDeclaration _quadVertexDecl;

        #endregion
        public PlaneComponent(GraphicsDevice device, 
                     Vector3 origin, Vector3 normal, Vector3 up, float width, float length)
        {
            _quad = new Quad(origin, normal, up, width, length);

            _quadVertexDecl = new VertexDeclaration(device,
                                                    VertexPositionNormalTexture.VertexElements);
        }

        public override BoundingBox ComputeBoundingBox()
        {
            float halfWidth = _quad.Width * 0.5f;
            float halfHeight = _quad.Height * 0.5f;
            Vector3 min = new Vector3(-halfWidth, -0.0f, -halfHeight);
            Vector3 max = new Vector3(halfWidth, 0.0f, halfHeight);
            return new BoundingBox(min, max);
        }

        //----------------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        /// The center point for the plane.
        /// </summary>
        public Vector3 Origin
        {
            get { return _quad.Origin; }
        }

        /// <summary>
        /// The normal vector for the plane.
        /// </summary>
        public Vector3 Normal
        {
            get { return _quad.Normal; }
        }

        #endregion

        class PlaneTriangleIterator : TriangleIterator
        {
            int _curTriIndex = 0;
            Quad _quad;
            public PlaneTriangleIterator(Quad quad)
            {
                _quad = quad;
            }

            public Triangle GetNextTriangle()
            {
                if (_curTriIndex == 2)
                    return null;

                int vertIndex = _curTriIndex * 3;
                Vector3 p1 = _quad.Vertices[_quad.Indices[vertIndex++]].Position;
                Vector3 p2 = _quad.Vertices[_quad.Indices[vertIndex++]].Position;
                Vector3 p3 = _quad.Vertices[_quad.Indices[vertIndex++]].Position;

                ++_curTriIndex;
                return new Triangle(p1, p2, p3);
            }

            public void Reset()
            {
                _curTriIndex = 0;
            }
        }

        public override TriangleIterator GetTriangleIterator()
        {
            PlaneTriangleIterator itr = new PlaneTriangleIterator(_quad);

            return itr;
        }

        public override void InitializeDrawables()
        {
            this.Drawables = null;

            //GraphicsDevice device = core.Game.instance().GraphicsDevice;
            //this.Drawables = new Drawable[1];
            //this.Drawables[0] = new Drawable(this);
            //this.Drawables[0].VertexBuffer = new VertexBuffer(device,
            //                                              VertexPositionNormalTexture.SizeInBytes * 4,
            //                                              BufferUsage.WriteOnly);
            
            //this.Drawables[0].VertexBuffer.SetData<VertexPositionNormalTexture>(_quad.Vertices);

            //this.Drawables[0].IndexBuffer = new IndexBuffer(device, 
            //                                            sizeof(int) * 6, 
            //                                            BufferUsage.WriteOnly, 
            //                                            IndexElementSize.ThirtyTwoBits);

            //this.Drawables[0].IndexBuffer.SetData<int>(_quad.Indices);
            //this.Drawables[0].DrawableParts = new Drawable.DrawablePart[1];
            //this.Drawables[0].DrawableParts[0] = new Drawable.DrawablePart(this.Drawables[0]);
            //this.Drawables[0].DrawableParts[0].BaseVertex = 0;
            //this.Drawables[0].DrawableParts[0].Effect = this.DefaultEffect;
            //this.Drawables[0].DrawableParts[0].MinVertexIndex = 0;
            //this.Drawables[0].DrawableParts[0].NumVertices = 4;
            //this.Drawables[0].DrawableParts[0].PrimitiveCount = 2;
            //this.Drawables[0].DrawableParts[0].PrimitiveType = PrimitiveType.TriangleList;
            //this.Drawables[0].DrawableParts[0].StartIndex = 0;
            //this.Drawables[0].DrawableParts[0].StreamOffset = 0;
            //this.Drawables[0].DrawableParts[0].VertexDeclaration = _quadVertexDecl;
            //this.Drawables[0].DrawableParts[0].VertexStride = VertexPositionNormalTexture.SizeInBytes;
        }

        //----------------------------------------------------------------------------

        #region Public Methods

        //public override void ConfigureEffect(BaseEffect effect,
        //                         Matrix worldMat,
        //                         Matrix viewMat,
        //                         Matrix projMat,
        //                         GameTime gameTime)
        //{

        //    effect.GraphicsDevice.VertexDeclaration = _quadVertexDecl;

        //    effect.SetState(this.State, worldMat, viewMat, projMat, gameTime);

        //    effect.Begin();
        //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Begin();

        //        effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
        //            PrimitiveType.TriangleList, _quad.Vertices, 0, 4, _quad.Indices, 0, 2);

        //        pass.End();
        //    }
        //    effect.End();
        //}

        #endregion

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }
    }
}
