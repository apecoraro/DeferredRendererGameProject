using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeskWars.gui
{
    public class Widget : core.Component
    {
        static public Widget RootWidget = new Widget("Root");

        private Widget(string name)
        {
            Construct(name, 0.0f, 0.0f, 1.0f, "NoParent");
        }
        string _parent = null;
        
        public Widget ParentWidget = null;

        List<Widget> _children;

        Vector2 _translation;

        Matrix _offsetTranslation = Matrix.Identity;


        private core.GameObject _gameObject = null;
        public core.GameObject GameObject { get { return _gameObject; } }
        
        string _name = null;
        
        float _width;
        float _height;

        float _depth = 1.0f;

        Rectangle _boundingRect = new Rectangle();
        bool _inside = false;
        bool _leftMouseBtnPressed = false;
        bool _isActive = true;
        public bool ReceiveEvents = true;

        public float Width { get { return _width; } set { _width = value; } }
        public float Height { get { return _height; } set { _height = value; } }
        public float Depth { get { return _depth; } set { _depth = value; } }

        public bool MouseIsInside()
        {
            if (_inside)
                return true;

            for (int i = 0; i < _children.Count; ++i)
            {
                Widget widget = _children[i];

                if (widget.MouseIsInside())
                    return true;
            }

            return false;
        }

        public bool Active
        {
            get { return _isActive; }
        }

        //activate or deactivate this widget
        //set overrideChildren to true if you want to activate/deactivate
        //the children of this widget
        public void SetIsActive(bool flag, bool overrideChildWidgets)
        {
            _isActive = flag;
            if(overrideChildWidgets)
            {
                for (int i = 0; i < _children.Count; ++i)
                {
                    Widget widget = _children[i];
                    
                    widget.SetIsActive(flag, overrideChildWidgets);
                }
            }
        }

        public List<Widget> Children
        {
            get { return _children; }
        }

        public Vector2 Translation
        {
            get { return _translation; }
        }

        public void SetOffsetTranslation(float x, float y)
        {
            _offsetTranslation = Matrix.CreateTranslation(x, y, 0.0f);
        }

        public void SetScreenCoords(int xScreen, int yScreen, int wScreen, int hScreen)
        {
            float width, height;
            GetGraphicsDeviceWidthHeight(out width, out height);

            float x = (float)xScreen / width;
            float y = (float)yScreen / height;

            SetOffsetTranslation(x, y);

            this.Width = (float)wScreen / width;
            this.Height = (float)hScreen / height;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            _gameObject = null;
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            if (this.ParentWidget == null)
            {
                //if not already connected to the tree of widgets then try to connect it
                gui.Widget parentComp = (_parent == null || _parent == "Root" ? RootWidget : RootWidget.FindWidget(_parent));
                if (parentComp != null)
                {
                    parentComp.AddChild(this);
                }
                else
                {
                    throw new Exception("Widget::Widget(): unable to find parent: " + _parent);
                }
            }

            AttachChildWidgetsToGameObject(_children);
        }

        protected void AttachChildWidgetsToGameObject(List<Widget> children)
        {
            for (int i = 0; i < children.Count; ++i)
            {
                Widget child = children[i];

                if(child.GameObject == null)
                    _gameObject.AddComponent(child);

                AttachChildWidgetsToGameObject(child.Children);
            }
        }
        //TODO change subclasses
        //Text and Rect need to construct from componentconfig
        public Widget(DeskWars.core.ComponentConfig config)
        {
            string name = config.GetParameterValue("Name");
            if (name == null)
                name = config.Name;

            float[] wh = config.GetFloatArrayParameterValue("WidthHeight");
            string depth = config.GetParameterValue("Depth");
            float fDepth = -1.0f;
            if (depth != null)
                fDepth = float.Parse(depth);
            string parent = config.GetParameterValue("Parent");

            float[] offsetXY = config.GetFloatArrayParameterValue("OffsetXY");
            if (offsetXY != null)
            {
                SetOffsetTranslation(offsetXY[0], offsetXY[1]);
            }

            Construct(name, wh[0], wh[1], fDepth, parent);
        }

        public Widget(DeskWars.core.ComponentConfig config, Vector2 wh)
        {
            string name = config.GetParameterValue("Name");
            if (name == null)
                name = config.Name;

            string depth = config.GetParameterValue("Depth");
            float fDepth = -1.0f;
            if (depth != null)
                fDepth = float.Parse(depth);
            string parent = config.GetParameterValue("Parent");

            float[] offsetXY = config.GetFloatArrayParameterValue("OffsetXY");
            if (offsetXY != null)
            {
                SetOffsetTranslation(offsetXY[0], offsetXY[1]);
            }

            Construct(name, wh.X, wh.Y, fDepth, parent);
        }

        public Widget(string name,
                      float width,
                      float height,
                      float depth)
        {
            Construct(name, width, height, depth, "Root");
        }

        public Widget(string name, Vector2 wh, float depth)
        {
            Construct(name, wh.X, wh.Y, depth, "Root");
        }

        private void Construct(string name,
                      float width,
                      float height,
                      float depth,
                      string parent)
        {
            _name = name;
            _width = width;
            _height = height;
            _depth = depth;
            _children = new List<Widget>();
            _parent = parent;
            if (parent == null || parent == "Root")
                RootWidget.AddChild(this);
        }

        public Widget FindWidget(string widgetName)
        {
            for (int i = 0; i < _children.Count; ++i)
            {
                Widget widget = _children[i];
                
                if (widget.Name == widgetName)
                    return widget;
                Widget child = widget.FindWidget(widgetName);
                if (child != null)
                    return child;
            }

            return null;
        }

        public void AddChild(Widget child)
        {
            _children.Add(child);

            if (_gameObject != null)//make sure it gets updated with the game object
                _gameObject.AddComponent(child);
            
            if (child.ParentWidget != null)
                child.ParentWidget.RemoveChild(child);

            child.ParentWidget = this;
        }

        public void RemoveChild(Widget child)
        {
            _children.Remove(child);
            child.ParentWidget = null;
        }

        public String Name
        {
            get 
            {
                return _name;
            }
        }

        public void UpdateScreenTransformation(Matrix parentMatrix)
        {
            Matrix worldTransform;
            if (ParentWidget == RootWidget)//if the parent is the root widget then the position comes from the gameobject
            {
                worldTransform = Matrix.Multiply(Matrix.Multiply(parentMatrix,
                                                                 _gameObject.WorldTransform),
                                                                 _offsetTranslation);
                if (this.Depth < 0.0f)
                    this.Depth = 1.0f;
            }
            else//otherwise the position comes from the parent widget's position
                worldTransform = Matrix.Multiply(parentMatrix, _offsetTranslation);

            Vector3 translation = Vector3.Transform(Vector3.Zero, worldTransform);
            _translation.X = translation.X;
            _translation.Y = translation.Y;

            foreach (Widget child in _children)
            {
                if(child.Depth < 0.0f)
                    child.Depth = this.Depth - 0.01f;
                child.UpdateScreenTransformation(worldTransform);
            }
        }

        private void GetGraphicsDeviceWidthHeight(out float width, out float height)
        {

            GraphicsDevice dev = core.Game.instance().GraphicsDevice;
            width = dev.Viewport.Width;
            height = dev.Viewport.Height;
        }

        public Rectangle BoundingRect
        {
            get
            {
                float width, height;
                GetGraphicsDeviceWidthHeight(out width, out height);

                _boundingRect.X = (int)(this.Translation.X * width);
                _boundingRect.Y = (int)(this.Translation.Y * height);
                _boundingRect.Width = (int)(this.Width * width);
                _boundingRect.Height = (int)(this.Height * height);    

                return _boundingRect;
            }
        }

        protected bool IsInside(int x, int y)
        {
            return BoundingRect.Contains(x, y);
        }

        public bool HandleMouseInput(MouseState mouseState)
        {
            bool stopPassingEvent = false;

            if(IsInside(mouseState.X, mouseState.Y))
            {
                if(!_inside)
                {
                    _inside = true;
                    stopPassingEvent = HandleMouseEnter(mouseState);
                }
                
                if(mouseState.LeftButton == ButtonState.Pressed)
                {
                    _leftMouseBtnPressed = true;
                    stopPassingEvent = HandleLeftMouseButtonPressed(mouseState);
                }

            }
            else if(_inside)
            {
                _inside = false;
                stopPassingEvent = HandleMouseExit(mouseState);
            }
            
            if(_leftMouseBtnPressed && mouseState.LeftButton == ButtonState.Released)
            {
                _leftMouseBtnPressed = false;
                stopPassingEvent = HandleLeftMouseButtonReleased(mouseState);
            }
            else if(_leftMouseBtnPressed || _inside)
            {
                stopPassingEvent = HandleMouseMove(mouseState);
            }
            
            //return true as long as a button is held down
            return _leftMouseBtnPressed || stopPassingEvent;
        }

        protected bool HandleMouseMove(MouseState mouseState)
        {
            MsgMouseEvent msg = new MsgMouseEvent(MsgMouseEvent.EventType.MouseMove, this, mouseState);
            _gameObject.Send(msg);
            return msg.Handled;
        }

        protected bool HandleLeftMouseButtonReleased(MouseState mouseState)
        {
            MsgMouseEvent msg = new MsgMouseEvent(MsgMouseEvent.EventType.MouseLeftButtonReleased, this, mouseState);
            _gameObject.Send(msg);
            return msg.Handled;
        }

        protected bool HandleMouseExit(MouseState mouseState)
        {
            MsgMouseEvent msg = new MsgMouseEvent(MsgMouseEvent.EventType.MouseExit, this, mouseState);
            _gameObject.Send(msg);
            return msg.Handled;
        }

        protected bool HandleLeftMouseButtonPressed(MouseState mouseState)
        {
            MsgMouseEvent msg = new MsgMouseEvent(MsgMouseEvent.EventType.MouseLeftButtonPressed, this, mouseState);
            _gameObject.Send(msg);
            return msg.Handled;
        }

        protected bool HandleMouseEnter(MouseState mouseState)
        {
            MsgMouseEvent msg = new MsgMouseEvent(MsgMouseEvent.EventType.MouseEnter, this, mouseState);
            _gameObject.Send(msg);
            return msg.Handled;
        }

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            SetIsActive(true, true);
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            SetIsActive(false, true);
        }
    }
}
