using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public class ModelComponent : gfx.Component
    {
        Model _model = null;
        float _scale = 1.0f;
        Matrix _scaleMtx = Matrix.Identity;
        bool _computeBoundingBox = true;

        List<ShadowMapEffect> _effects = new List<ShadowMapEffect>();

        public Model Model
        {
            get { return _model; }
            set 
            { 
                _model = value;
            }
        }

        public void ApplyScale(float scale)
        {
            _scale = scale;
            _scaleMtx = Matrix.CreateScale(_scale);
        }

        public Matrix ScaleTransform
        {
            get { return _scaleMtx; }
        }

        protected BoundingBox ComputeBoundingBoxInsideSphere(BoundingSphere sphere)
        {
            Vector3 max = new Vector3(1.0f, 1.0f, 1.0f);
            max.Normalize();
            max *= sphere.Radius;

            Vector3 min = new Vector3(-1.0f, -1.0f, -1.0f);
            min.Normalize();
            min *= sphere.Radius;

            return new BoundingBox(sphere.Center + min, sphere.Center + max);
        }

        public void SetComputeBoundingBox(bool flag)
        {
            _computeBoundingBox = flag;
        }

        public override BoundingBox ComputeBoundingBox()
        {
            if (!_computeBoundingBox)
                return NoBoundingBox;

            BoundingSphere bsphere = new BoundingSphere();
            foreach (ModelMesh mesh in _model.Meshes)
            {
                bsphere = BoundingSphere.CreateMerged(bsphere, mesh.BoundingSphere);
            }

            bsphere.Radius *= _scale;

            return ComputeBoundingBoxInsideSphere(bsphere);
        }

        public ModelComponent() : base() { }


        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void PreDraw(Matrix worldMat, GameTime gameTime)
        {
            Matrix world = _model.Root.Transform * _scaleMtx * worldMat;
            SetDrawablesWorldTransform(world);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void InitializeDrawables()
        {
            this.Drawables = new Drawable[_model.Meshes.Count];
            int meshIndex = 0;
            foreach (ModelMesh mesh in _model.Meshes)
            {
                this.Drawables[meshIndex] = new Drawable(this);
                this.Drawables[meshIndex].IndexBuffer = mesh.IndexBuffer;
                this.Drawables[meshIndex].VertexBuffer = mesh.VertexBuffer;
                this.Drawables[meshIndex].DrawableParts = new Drawable.DrawablePart[mesh.MeshParts.Count];
                for (int i = 0; i < mesh.MeshParts.Count; ++i)
                {
                    ModelMeshPart part = mesh.MeshParts[i];
                    this.Drawables[meshIndex].DrawableParts[i] = new Drawable.DrawablePart(this.Drawables[meshIndex]);
                    Drawable.DrawablePart drawPart = this.Drawables[meshIndex].DrawableParts[i];
                    drawPart.BaseVertex = part.BaseVertex;
                    drawPart.Effect = (ShadowMapEffect)part.Effect;
                    drawPart.MinVertexIndex = 0;
                    drawPart.NumVertices = part.NumVertices;
                    drawPart.PrimitiveCount = part.PrimitiveCount;
                    drawPart.PrimitiveType = PrimitiveType.TriangleList;
                    drawPart.StartIndex = part.StartIndex;
                    drawPart.StreamOffset = part.StreamOffset;
                    drawPart.VertexDeclaration = part.VertexDeclaration;
                    drawPart.VertexStride = part.VertexStride;
                }
                ++meshIndex;
            }
        }
    }
}

