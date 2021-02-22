using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    class HaloComponent : gfx.Component
    {
        VertexBuffer _vertexBuffer;
        VertexDeclaration _vertexDecl;
        int _vertexCount;
        core.GameObject _selectedObject;
        
        public core.GameObject SelectedObject
        {
            get { return _selectedObject; }
            set 
            { 
                _selectedObject = value;
                this.Drawables[0].Enabled = (_selectedObject != null);
            }
        }

        class MsgObjectSelectedHandler : com.MessageHandler
        {
            HaloComponent _comp;
            public MsgObjectSelectedHandler(HaloComponent comp) { _comp = comp; }

            #region MessageHandler Members

            void DeskWars.com.MessageHandler.receive(DeskWars.com.Message msg)
            {
                com.MsgObjectSelected message = (com.MsgObjectSelected)msg;
                _comp.SelectedObject = message.SelectedObject;
            }

            #endregion
        }

        MsgObjectSelectedHandler _msgHandler = null;

        public HaloComponent(GraphicsDevice device, 
                             float elev, 
                             float haloRadius, 
                             int numSegments, 
                             float haloRingWidth)
        {
            //TODO this should be determined by the GameObject that I'm attached to
            _vertexCount = (numSegments * 2) + 2;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[_vertexCount];
            _vertexDecl = new VertexDeclaration(device,
                                                VertexPositionNormalTexture.VertexElements);

            float delta = 360.0f / numSegments;
            int vindex = 0;
            for(float t = 0.0f; t <= 360.0f; t += delta)
            {
                float radians = MathHelper.ToRadians(t);
                float x = (float)(haloRadius * Math.Cos(radians));
                float y = elev;
                float z = (float)(haloRadius * Math.Sin(radians));

                Vector3 position = new Vector3(x, y,  z);
                Vector3 normal = new Vector3(0.0f, 1.0f, 0.0f);
                Vector2 uv = new Vector2(1.0f * (t/360.0f), 0.0f);
                vertices[vindex++] = new VertexPositionNormalTexture(position, normal, uv);

                Vector3 push = new Vector3(x, 0.0f, z);
                push.Normalize();

                position += (push * haloRingWidth);
                uv.Y = 1.0f;
                vertices[vindex++] = new VertexPositionNormalTexture(position, normal, uv);
            }

            _vertexBuffer = new VertexBuffer(device,
                                             VertexPositionNormalTexture.SizeInBytes * _vertexCount,
                                             BufferUsage.WriteOnly);

            _vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            _msgHandler = new MsgObjectSelectedHandler(this);
        }

        public override void InitializeDrawables()
        {
            this.Drawables = new gfx.Drawable[1];
            this.Drawables[0] = new gfx.Drawable(this);
            this.Drawables[0].Enabled = false;
            this.Drawables[0].VertexBuffer = _vertexBuffer;
            this.Drawables[0].IndexBuffer = null;
            this.Drawables[0].DrawableParts = new gfx.Drawable.DrawablePart[1];
            this.Drawables[0].DrawableParts[0] = new gfx.Drawable.DrawablePart(this.Drawables[0]);
            this.Drawables[0].DrawableParts[0].Effect = this.DefaultEffect;
            this.Drawables[0].DrawableParts[0].PrimitiveType = PrimitiveType.TriangleStrip;
            this.Drawables[0].DrawableParts[0].BaseVertex = 0;
            this.Drawables[0].DrawableParts[0].PrimitiveCount = _vertexCount - 2;
            this.Drawables[0].DrawableParts[0].VertexDeclaration = _vertexDecl;
            this.Drawables[0].DrawableParts[0].StreamOffset = 0;
            this.Drawables[0].DrawableParts[0].VertexStride = VertexPositionNormalTexture.SizeInBytes;
        }

        public override void  Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            if (_selectedObject == null || _selectedObject.IsActive == false)
            {
                _selectedObject = null;
                this.Drawables[0].Enabled = false;
            }
            else
                gameObject.Position = _selectedObject.Position;

 	        base.Update(gameObject, gameTime);
        }

        //public override void ConfigureEffect(gfx.BaseEffect effect, 
        //                        Matrix worldMat, 
        //                        Matrix viewMat, 
        //                        Matrix projMat, 
        //                        GameTime gameTime)
        //{
        //    if (_selectedObject == null)
        //        return;

        //    effect.GraphicsDevice.VertexDeclaration = _vertexDecl;

        //    Matrix world = Matrix.CreateTranslation(_selectedObject.BoundingSphere.Center);

        //    effect.SetState(this.State, world, viewMat, projMat, gameTime);

        //    effect.Begin();

        //    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        //    {
        //        pass.Begin();

        //        effect.GraphicsDevice.DrawUserPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleStrip,
        //                                                                              _vertices, 0, _vertexCount-2);
        //        pass.End();
        //    }

        //    effect.End();
        //}

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgObjectSelected), _msgHandler);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgObjectSelected), _msgHandler);
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }
    }
}
