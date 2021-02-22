using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace DeskWars.core
{
    public class GameObject 
    {
        public double delay; //for initialization of objects.
        String _name;
        String _class;
        List<Component> _components = new List<Component>();
        
        com.PostOffice _postOffice = new com.PostOffice();

        Vector3 _position = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 _lookAt = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 _lookVector = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 _rightVector = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 _upVector = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 _prevPosition = new Vector3(0.0f, 0.0f, 0.0f);

        bool _active = true;

        Matrix _worldTransform = Matrix.Identity;
        BoundingSphere _boundingSphere = new BoundingSphere();
        BoundingBox _boundingBox = new BoundingBox();
        bool _needToComputeBounds = true;
        bool _needToUpdateWorldTransform = true;
        
        public GameObject(String name, String objectClass)
        {
            _name = name;
            _class = objectClass;
            _postOffice.SendMode = com.PostOffice.Mode.IMMEDIATE_DELIVERY;
        }

        public String Name
        {
            get { return _name; }
        }

        public String Class
        {
            get { return _class; }
        }

        public Matrix WorldTransform
        {
            get 
            {
                if (_needToUpdateWorldTransform)
                {
                    UpdateWorldTransform();
                    _needToUpdateWorldTransform = false;
                }
                return _worldTransform; 
            }
        }

        protected void UpdateWorldTransform()
        {
            _worldTransform.Translation = _position;

            _worldTransform.M11 = _rightVector.X;
            _worldTransform.M12 = _rightVector.Y;
            _worldTransform.M13 = _rightVector.Z;

            _worldTransform.M21 = _upVector.X;
            _worldTransform.M22 = _upVector.Y;
            _worldTransform.M23 = _upVector.Z;

            _worldTransform.M31 = _lookVector.X;
            _worldTransform.M32 = _lookVector.Y;
            _worldTransform.M33 = _lookVector.Z;
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                if (_needToComputeBounds)
                {
                    ComputeBoundingBox();
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
                    ComputeBoundingBox();
                    _boundingSphere = BoundingSphere.CreateFromBoundingBox(_boundingBox);
                    _needToComputeBounds = false;
                }

                return _boundingBox;
            }
        }

        public void OverrideBoundingBox(BoundingBox bbox)
        {
            _boundingBox = bbox;
            _boundingSphere = BoundingSphere.CreateFromBoundingBox(_boundingBox);
        }

        protected void ComputeBoundingBox()
        {            
            BoundingBox boundingBox = new BoundingBox();
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                boundingBox = BoundingBox.CreateMerged(boundingBox, comp.BoundingBox);
            }

            Vector3[] corners = new Vector3[8];
            boundingBox.GetCorners(corners);
            for (int i = 0; i < corners.Length; ++i)
            {
                corners[i] = Vector3.Transform(corners[i], this.WorldTransform);
            }

            _boundingBox = BoundingBox.CreateFromPoints(corners);
        }

        public com.PostOffice PostOffice
        {
            get { return _postOffice; }
        }

        public List<Component> Components
        {
            get { return _components; }
        }

        public void AddComponent(Component comp)
        {
            _components.Add(comp);
            comp.OnAddToGameObject(this);
        }

        public void RemoveComponent(Component remove)
        {
            if (_components.Remove(remove))
                remove.OnRemoveFromGameObject(this);
        }

        public void RemoveComponents()
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                comp.OnRemoveFromGameObject(this);
            }
        }
        
        public void VisitComponents(ComponentVisitor vis)
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                vis.Apply(comp);
            }
        }

        public int GetComponentCount()
        {
            return _components.Count;
        }

        public Component GetComponent(int index, ComponentTypeFilter typeFilter)
        {
            if (typeFilter.Accept(this, _components[index]))
                return _components[index];

            return null;
        }

        public bool IsType(Type type)
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                if (comp.GetType() == type)
                    return true; 
            }

            return false;
        }

        public Component GetComponent(int index)
        {
            return _components[index];
        }

        public virtual void OnLoadLevel(int level)
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                comp.OnLoadLevel(this, level);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                comp.Update(this, gameTime);
            }
        }

        public void Delete()
        {
            for (int i = 0; i < _components.Count; ++i)
            {
                Component comp = _components[i];
                comp.OnRemoveFromGameObject(this);
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                Vector3 translation = value - _position;
                _needToUpdateWorldTransform = true;

                _position = value;
                _boundingSphere.Center += translation;
                _boundingBox.Max += translation;
                _boundingBox.Min += translation;
            }
        }

        public void SetPositionAndLookAt(Vector3 pos, Vector3 lookAt)
        {
            _position = pos;
            LookAt = lookAt;
        }


        public Vector3 LookAt
        {
            get { return _lookAt; }
            set
            {
                _lookAt = value;
                if (_lookAt == _position)
                    _lookAt = _position + _lookVector;
                this.LookVector = _lookAt - _position;
            }
        }

        public Vector3 LookVector
        {
            get { return _lookVector; }
            set
            {
                _lookVector = value;
                _lookVector.Normalize();
                _rightVector = Vector3.Cross(Vector3.UnitY, _lookVector);
                //_rightVector.Normalize();
                _upVector = Vector3.Cross(_lookVector, _rightVector);
                //_upVector.Normalize();
                _needToUpdateWorldTransform = true;
                _needToComputeBounds = true;
            }
        }

        public Vector3 RightVector
        {
            get { return _rightVector; }
        }

        public Vector3 UpVector
        {
            get { return _upVector; }
        }

        public Vector3 PrevPosition
        {
            get { return _prevPosition; }
            set
            {
                _prevPosition = value;
            }
        }

        public bool IsActive
        {
            get { return _active; }
            set 
            { 
                _active = value;
                for (int i = 0; i < _components.Count; ++i)
                {
                    Component comp = _components[i];
                    if (_active)
                        comp.OnGameObjectActivated(this);
                    else
                        comp.OnGameObjectDeactivated(this);
                }
            }
        }

        public void Send(com.GameObjectMessage message)
        {
            message.GameObject = this;
            _postOffice.Send(message);
        }


    }
}