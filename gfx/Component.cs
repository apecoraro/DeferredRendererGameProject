using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public abstract class Component : DeskWars.core.Component
    {
        ShadowMapEffect _defaultEffect = null;
        public ShadowMapEffect DefaultEffect
        {
            get { return _defaultEffect; }
            set { _defaultEffect = value; }
        }

        Drawable[] _drawables;
        public Drawable[] Drawables
        {
            get { return _drawables; }
            set { _drawables = value; }
        }

        public Component()
        {
        }

        public abstract void InitializeDrawables();

        public void AddDrawablesToRenderBin(gfx.Engine.RenderBin renderBin)
        {
            if (this.Drawables == null)
                return;

            for (int i = 0; i < this.Drawables.Length; ++i)
            {
                if(this.Drawables[i].Enabled)
                    renderBin.AddDrawable(this.Drawables[i]);
            }
        }

        public virtual void PreDraw(Matrix worldMat, GameTime gameTime)
        {
            SetDrawablesWorldTransform(worldMat);
        }

        public virtual void ConfigureEffect(ShadowMapEffect baseEffect,
                                            Matrix worldMat,
                                            ComponentData data)
        {
            baseEffect.World = worldMat;
        }

        public void SetDrawablesWorldTransform(Matrix worldMat)
        {
            if (this.Drawables != null)
            {
                for (int i = 0; i < this.Drawables.Length; ++i)
                {
                    this.Drawables[i].WorldTransform = worldMat;
                }
            }
        }

        public virtual TriangleIterator GetTriangleIterator()
        {
            return null;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
        }
    }
}