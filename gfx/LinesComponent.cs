using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public class LinesComponent : gfx.Component
    {
        List<VertexPositionColor[]> _vertices = null;
        int _numVertices = 0;
        int _vtxBufferSize = 0;
        VertexDeclaration _vertexDecl;
        bool _drawableNeedsUpdate = false;

        public LinesComponent(GraphicsDevice device, int vtxBufferSize)
        {
            _vertices = new List<VertexPositionColor[]>();
            _vtxBufferSize = vtxBufferSize;
            VertexPositionColor[] buffer = new VertexPositionColor[_vtxBufferSize];
            _vertices.Add(buffer);
            _vertexDecl = new VertexDeclaration(device, VertexPositionColor.VertexElements);
        }

        public void AddLine(Vector3 p1, Vector3 p2, Color c)
        {
            if (_numVertices+2 > _vtxBufferSize)
            {
                _numVertices = 0;
                VertexPositionColor[] buffer = new VertexPositionColor[_vtxBufferSize];
                _vertices.Add(buffer);
            }

            _vertices[_vertices.Count - 1][_numVertices] = new VertexPositionColor(p1, c);
            _vertices[_vertices.Count - 1][_numVertices + 1] = new VertexPositionColor(p2, c);
            _numVertices += 2;
            _drawableNeedsUpdate = true;
        }

        public void ClearLines()
        {
            _numVertices = 0;
            _vertices.Clear();
            VertexPositionColor[] buffer = new VertexPositionColor[_vtxBufferSize];
            _vertices.Add(buffer);
            _drawableNeedsUpdate = true;
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            if (_drawableNeedsUpdate)
            {
                InitializeDrawables();
            }
        }

        public override void InitializeDrawables()
        {
            if (_numVertices == 0)
                return;

            GraphicsDevice device = core.Game.instance().GraphicsDevice;
            VertexPositionColor[] allVerts = new VertexPositionColor[_vertices.Count * _vtxBufferSize];
            int totalVtxCount = 0;
            for (int i = 0; i < _vertices.Count; ++i)
            {
                VertexPositionColor[] verts = _vertices[i];
                int numVerts = _vtxBufferSize;
                if (i + 1 == _vertices.Count)
                    numVerts = _numVertices;
                verts.CopyTo(allVerts, totalVtxCount);
                
                totalVtxCount += numVerts;
            }

            this.Drawables = new Drawable[1];
            this.Drawables[0] = new Drawable(this);
            this.Drawables[0].VertexBuffer = new VertexBuffer(device,
                                                          totalVtxCount * VertexPositionColor.SizeInBytes,
                                                          BufferUsage.WriteOnly);
            this.Drawables[0].VertexBuffer.SetData<VertexPositionColor>(allVerts);

            this.Drawables[0].IndexBuffer = null;
            this.Drawables[0].DrawableParts = new Drawable.DrawablePart[1];
            this.Drawables[0].DrawableParts[0] = new Drawable.DrawablePart(this.Drawables[0]);
            this.Drawables[0].DrawableParts[0].BaseVertex = 0;
            this.Drawables[0].DrawableParts[0].Effect = this.DefaultEffect;
            this.Drawables[0].DrawableParts[0].MinVertexIndex = 0;
            this.Drawables[0].DrawableParts[0].NumVertices = allVerts.Length;
            this.Drawables[0].DrawableParts[0].PrimitiveCount = (int)(allVerts.Length * 0.5f);
            this.Drawables[0].DrawableParts[0].PrimitiveType = PrimitiveType.LineList;
            this.Drawables[0].DrawableParts[0].StartIndex = 0;
            this.Drawables[0].DrawableParts[0].StreamOffset = 0;
            this.Drawables[0].DrawableParts[0].VertexDeclaration = _vertexDecl;
            this.Drawables[0].DrawableParts[0].VertexStride = VertexPositionColor.SizeInBytes;
        }

        //public override void ConfigureEffect(BaseEffect effect, 
        //                          Matrix worldMat, 
        //                          Matrix viewMat, 
        //                          Matrix projMat, 
        //                          GameTime gameTime)
        //{
        //    effect.GraphicsDevice.VertexDeclaration = _vertexDecl;

        //    effect.SetState(this.State, worldMat, viewMat, projMat, gameTime);

        //    for (int i = 0; i < _vertices.Count; ++i)
        //    {
        //        int numVerts = _vtxBufferSize;
        //        if (i + 1 == _vertices.Count)
        //            numVerts = _numVertices;

        //        effect.Begin();
        //        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //        {
        //            pass.Begin();

        //            effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
        //                                  _vertices[i], 0, (int)(numVerts * 0.5f));
        //            pass.End();
        //        }
        //        effect.End();
        //    }
        //}

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }
    }
}
