using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public class BoundingBoxLines : gfx.LinesComponent
    {
        core.GameObject _gameObject;
        public BoundingBoxLines(GraphicsDevice device) : base(device, 24) { }
        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            _gameObject = gameObject;
            
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            _gameObject = null;
            base.OnRemoveFromGameObject(gameObject);
        }

        public override void PreDraw(Matrix worldMat, GameTime gameTime)
        {
            if (_gameObject == null)
                return;

            ClearLines();

            BoundingBox bbox = _gameObject.BoundingBox;
            Vector3 lowerLeftFront = new Vector3(bbox.Min.X,
                                                 bbox.Min.Y,
                                                 bbox.Max.Z);
            Vector3 lowerRightFront = new Vector3(bbox.Max.X,
                                                 bbox.Min.Y,
                                                 bbox.Max.Z);
            Vector3 upperLeftFront = new Vector3(bbox.Min.X,
                                                 bbox.Max.Y,
                                                 bbox.Max.Z);
            Vector3 upperRightFront = bbox.Max;

            Vector3 lowerLeftBack = bbox.Min;
            Vector3 lowerRightBack = new Vector3(bbox.Max.X,
                                                 bbox.Min.Y,
                                                 bbox.Min.Z);
            Vector3 upperLeftBack = new Vector3(bbox.Min.X,
                                                 bbox.Max.Y,
                                                 bbox.Min.Z);
            Vector3 upperRightBack = new Vector3(bbox.Max.X,
                                                 bbox.Max.Y,
                                                 bbox.Min.Z);
            Color clr = Color.Green;
            AddLine(lowerLeftFront, lowerRightFront, clr);
            AddLine(lowerLeftFront, upperLeftFront, clr);
            AddLine(upperLeftFront, upperRightFront, clr);
            AddLine(upperRightFront, lowerRightFront, clr);

            AddLine(lowerLeftFront, lowerLeftBack, clr);
            AddLine(upperLeftFront, upperLeftBack, clr);
            AddLine(lowerLeftBack, upperLeftBack, clr);

            AddLine(lowerRightFront, lowerRightBack, clr);
            AddLine(upperRightFront, upperRightBack, clr);
            AddLine(lowerRightBack, upperRightBack, clr);

            AddLine(lowerLeftBack, lowerRightBack, clr);
            AddLine(upperLeftBack, upperRightBack, clr);

            base.PreDraw(Matrix.Identity, gameTime);
        }
    }
}
