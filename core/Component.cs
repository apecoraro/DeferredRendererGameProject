using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    abstract public class Component
    {
        BoundingSphere _boundingSphere;
        BoundingBox _boundingBox;
        bool _needToComputeBounds = true;
        public BoundingBox NoBoundingBox = new BoundingBox();

        public BoundingSphere BoundingSphere
        {
            get
            {
                if (_needToComputeBounds)
                {
                    _boundingBox = ComputeBoundingBox();
                    _boundingSphere = BoundingSphere.CreateFromBoundingBox(_boundingBox);
                    _needToComputeBounds = false;
                }

                return _boundingSphere;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                if (_needToComputeBounds)
                {
                    _boundingBox = ComputeBoundingBox();
                    _boundingSphere = BoundingSphere.CreateFromBoundingBox(_boundingBox);
                    _needToComputeBounds = false;
                }

                return _boundingBox;
            }
        }

        public virtual BoundingBox ComputeBoundingBox() 
        {
            return NoBoundingBox;
        }
        //called right after a component is added to a gameobject
        public abstract void OnAddToGameObject(GameObject gameObject);
        //called right after a component is removed from a gameobject
        public abstract void OnRemoveFromGameObject(GameObject gameObject);
        //called right before a new level is started
        public abstract void OnLoadLevel(GameObject gameObject, int level);
        //called when a component's game object is activated
        public abstract void OnGameObjectActivated(GameObject gameObject);
        //called when a component's game object is deactivated
        public abstract void OnGameObjectDeactivated(GameObject gameObject);
        //called once every frame
        public abstract void Update(GameObject gameObject, GameTime gameTime);
    }
}
