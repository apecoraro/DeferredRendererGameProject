using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    public class CameraComponent : core.Component, core.Camera
    {
        float _fieldOfView;
        float _aspectRatio;
        float _nearPlane;
        float _farPlane;

        Vector3 _cameraPos;
        Vector3 _cameraLookAt;
        Matrix _cameraProjMat;
        Matrix _cameraViewMat;

        BoundingFrustum _frustum = null;
        public BoundingFrustum Frustum
        {
            get { return _frustum; }
        }

        public CameraComponent(float fieldOfView, float aspectRatio,
                               float nearPlane, float farPlane)
        {
            _fieldOfView = fieldOfView;
            _aspectRatio = aspectRatio;
            _nearPlane = nearPlane;
            _farPlane = farPlane;


            _cameraProjMat = Matrix.CreatePerspectiveFieldOfView(_fieldOfView,
                                                                 _aspectRatio,
                                                                 _nearPlane,
                                                                 _farPlane);

            _cameraViewMat = Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up);

            _frustum = new BoundingFrustum(_cameraViewMat * _cameraProjMat);

        }

        public Matrix ProjMat
        {
            get { return _cameraProjMat; }
        }

        public Matrix ViewMat
        {
            get { return _cameraViewMat; }
        }

        public Vector3 Position
        {
            get { return _cameraPos; }
        }

        public Vector3 LookAt
        {
            get { return _cameraLookAt; }
        }

        #region Component Members

        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            core.Game.instance().Services.AddService(typeof(core.Camera), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            core.Game.instance().Services.RemoveService(typeof(core.Camera));
        }

        public override void Update(core.GameObject gameObject, 
                           Microsoft.Xna.Framework.GameTime gameTime)
        {
            _cameraPos = gameObject.Position;
            _cameraLookAt = gameObject.LookAt;
            _cameraViewMat = Matrix.CreateLookAt(gameObject.Position, 
                                                 gameObject.LookAt, 
                                                 Vector3.Up);

            _frustum.Matrix = _cameraViewMat * _cameraProjMat;
        }

        #endregion

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        #region Camera Members

        public Vector3 GetPosition()
        {
            return _cameraPos;
        }

        public Vector3 GetLookAt()
        {
            return _cameraLookAt;
        }

        public Vector3 GetUp()
        {
            return Vector3.Up;
        }

        public Matrix GetViewMatrix()
        {
            return _cameraViewMat;
        }

        public Matrix GetProjectionMatrix()
        {
            return _cameraProjMat;
        }

        #endregion
    }
}