using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    class BoxComponent : gfx.Component
    {
        List<Quad> _sides = new List<Quad>(6);
        
        VertexDeclaration _quadVertexDecl;
        float _halfHeight;
        float _halfWidth;
        float _halfLength;

        public BoxComponent(GraphicsDevice device,
                            Vector3 origin, 
                            float width, float height, float length)
        {
            _halfHeight = height * 0.5f;
            _halfWidth = width * 0.5f;
            _halfLength = length * 0.5f;

            //we want the origin of all model's to be at Y == 0
            origin.Y += _halfHeight;

            //front
            Quad side = new Quad(origin + (-Vector3.UnitZ * _halfLength), -Vector3.UnitZ, Vector3.UnitY, width, height);
            _sides.Add(side);
            //back
            side = new Quad(origin + (Vector3.UnitZ * _halfLength), Vector3.UnitZ, Vector3.UnitY, width, height);
            _sides.Add(side);
            //right
            side = new Quad(origin + (Vector3.UnitX * _halfWidth), Vector3.UnitX, Vector3.UnitY, length, height);
            _sides.Add(side);
            //left
            side = new Quad(origin + (-Vector3.UnitX * _halfWidth), -Vector3.UnitX, Vector3.UnitY, length, height);
            _sides.Add(side);
            //top
            side = new Quad(origin + (Vector3.UnitY * _halfHeight), Vector3.UnitY, Vector3.UnitZ, width, length);
            _sides.Add(side);
            //bottom
            side = new Quad(origin + (-Vector3.UnitY * _halfHeight), -Vector3.UnitY, Vector3.UnitZ, width, length);
            _sides.Add(side);
            
            _quadVertexDecl = new VertexDeclaration(device,
                VertexPositionNormalTexture.VertexElements);

            
        }

        public override BoundingBox ComputeBoundingBox()
        {
            Vector3 min = new Vector3(-_halfWidth, 0.0f, -_halfLength);
            Vector3 max = new Vector3(_halfWidth, _halfHeight + _halfHeight, _halfLength);
            return new BoundingBox(min, max);
        }

        class BoxTriangleIterator : TriangleIterator
        {
            int _curQuadIndex = 0;
            int _curTriIndex = 0;
            List<Quad> _boxSides = null;
            public BoxTriangleIterator(List<Quad> boxSides)
            {
                _boxSides = boxSides;
            }

            public Triangle GetNextTriangle()
            {
                if (_curQuadIndex == 6)
                    return null;

                Quad quad = _boxSides[_curQuadIndex];

                int vertIndex = _curTriIndex * 3;
                Vector3 p1 = quad.Vertices[quad.Indices[vertIndex++]].Position;
                Vector3 p2 = quad.Vertices[quad.Indices[vertIndex++]].Position;
                Vector3 p3 = quad.Vertices[quad.Indices[vertIndex++]].Position;

                ++_curTriIndex;
                if (_curTriIndex == 3)
                {
                    ++_curQuadIndex;
                    _curTriIndex = 0;
                }

                return new Triangle(p1, p2, p3);
            }

            public void Reset()
            {
                _curQuadIndex = 0;
                _curTriIndex = 0;
            }
        }

        public override TriangleIterator GetTriangleIterator()
        {
            BoxTriangleIterator itr = new BoxTriangleIterator(_sides);

            return itr;
        }

        public override void InitializeDrawables()
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[24];
            int[] indices = new int[36];

            int vtxIndex = 0;
            int index = 0;
            foreach (Quad quad in _sides)
            {
                quad.Vertices.CopyTo(vertices, vtxIndex);
                indices[index] = quad.Indices[0]+vtxIndex;
                indices[index+1] = quad.Indices[1]+vtxIndex;
                indices[index+2] = quad.Indices[2]+vtxIndex;
                indices[index+3] = quad.Indices[3]+vtxIndex;
                indices[index+4] = quad.Indices[4]+vtxIndex;
                indices[index+5] = quad.Indices[5]+vtxIndex;
                
                vtxIndex += quad.Vertices.Length;
                index += quad.Indices.Length;
            }

            GraphicsDevice device = core.Game.instance().GraphicsDevice;
            this.Drawables = new Drawable[1];
            this.Drawables[0] = new Drawable(this);
            this.Drawables[0].VertexBuffer = new VertexBuffer(device,
                                                          VertexPositionNormalTexture.SizeInBytes * vertices.Length,
                                                          BufferUsage.WriteOnly);

            this.Drawables[0].VertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            this.Drawables[0].IndexBuffer = new IndexBuffer(device,
                                                        sizeof(int) * indices.Length,
                                                        BufferUsage.WriteOnly,
                                                        IndexElementSize.ThirtyTwoBits);

            this.Drawables[0].IndexBuffer.SetData<int>(indices);
            this.Drawables[0].DrawableParts = new Drawable.DrawablePart[1];
            this.Drawables[0].DrawableParts[0] = new Drawable.DrawablePart(this.Drawables[0]);
            this.Drawables[0].DrawableParts[0].BaseVertex = 0;
            this.Drawables[0].DrawableParts[0].Effect = this.DefaultEffect;
            this.Drawables[0].DrawableParts[0].MinVertexIndex = 0;
            this.Drawables[0].DrawableParts[0].NumVertices = 24;
            this.Drawables[0].DrawableParts[0].PrimitiveCount = 12;
            this.Drawables[0].DrawableParts[0].PrimitiveType = PrimitiveType.TriangleList;
            this.Drawables[0].DrawableParts[0].StartIndex = 0;
            this.Drawables[0].DrawableParts[0].StreamOffset = 0;
            this.Drawables[0].DrawableParts[0].VertexDeclaration = _quadVertexDecl;
            this.Drawables[0].DrawableParts[0].VertexStride = VertexPositionNormalTexture.SizeInBytes;
        }

        //----------------------------------------------------------------------------

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

        //        foreach (Quad quad in _sides)
        //        {
        //            effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
        //                            PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indices, 0, 2);
        //        }

        //        pass.End();
        //    }

        //    effect.End();
        //}

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }
    }
}
