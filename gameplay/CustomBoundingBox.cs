using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    class CustomBoundingBox : core.Component
    {
        Microsoft.Xna.Framework.BoundingBox _boundingBox;
        public CustomBoundingBox(float width, float height, float length)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float halfLength = length * 0.5f;

            Vector3 pt = new Vector3(halfWidth, 
                                     halfHeight, 
                                     halfLength);

            _boundingBox = new Microsoft.Xna.Framework.BoundingBox(-pt, pt);
        }
        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
        }

        public override Microsoft.Xna.Framework.BoundingBox ComputeBoundingBox()
        {
            return _boundingBox;
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
        }
    }
}
