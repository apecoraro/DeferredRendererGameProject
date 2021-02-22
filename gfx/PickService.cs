using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gfx
{
    public class PickService : core.PickService
    {
        Viewport _viewport;
        CameraComponent _camera;

        #region PickService Members

        public PickService(Viewport vp, CameraComponent camera)
        {
            _viewport = vp;
            _camera = camera;
        }

        public Ray ComputeRayFromScreenXY(int screenX, int screenY)
        {
            Vector3 pos1 = _viewport.Unproject(new Vector3(screenX, screenY, 0), 
                                               _camera.ProjMat, 
                                               _camera.ViewMat, 
                                               Matrix.Identity);
            Vector3 pos2 = _viewport.Unproject(new Vector3(screenX, screenY, 1),
                                               _camera.ProjMat,
                                               _camera.ViewMat,
                                               Matrix.Identity);
            return new Ray(pos1, Vector3.Normalize(pos2 - pos1));
        }

        #endregion
    }
}
