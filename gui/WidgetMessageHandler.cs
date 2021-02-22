using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.gui
{
    public abstract class WidgetMessageHandler : core.Component
    {
        public class MessageHandler : com.MessageHandler
        {
            WidgetMessageHandler _comp = null;
            
            public MessageHandler(WidgetMessageHandler comp)
            {
                _comp = comp;
            }
            #region MessageHandler Members
            public void receive(DeskWars.com.Message msg)
            {
                gui.MsgMouseEvent me = (gui.MsgMouseEvent)msg;
                if (_comp.AcceptMouseEvent(me))
                {
                    _comp.OnMouseEvent(me);
                }
            }

            #endregion
        }

        MessageHandler _msgHandler;
        HashSet<string> _widgetNamesToHandle;
        HashSet<Widget> _widgetsToHandle;
        public WidgetMessageHandler(string widgetToHandle)
        {
            if (widgetToHandle != null)
            {
                _widgetNamesToHandle = new HashSet<string>();
                _widgetNamesToHandle.Add(widgetToHandle);
            }
            _msgHandler = new WidgetMessageHandler.MessageHandler(this);
        }

        public WidgetMessageHandler(Widget widgetToHandle)
        {
            _widgetsToHandle = new HashSet<Widget>();
            _widgetsToHandle.Add(widgetToHandle);

            _msgHandler = new WidgetMessageHandler.MessageHandler(this);
        }
        
        public WidgetMessageHandler(core.ComponentConfig config)
        {
            string[] widgetsToHandle = config.GetArrayParameterValue("Widgets");
            if (widgetsToHandle != null)
            {
                _widgetNamesToHandle = new HashSet<string>();
                for (int i = 0; i < widgetsToHandle.Length; ++i)
                {
                    _widgetNamesToHandle.Add(widgetsToHandle[i]);
                }
            }
            _msgHandler = new WidgetMessageHandler.MessageHandler(this);
        }

        //set this WidgetMessageHandler to handled events for this named widget
        public void SetAsWidgetHandler(Widget widget)
        {
            _widgetsToHandle.Add(widget);
        }

        protected void AddWidgetByName(string widgetName)
        {
            Widget widget = gui.Widget.RootWidget.FindWidget(widgetName);
            if (widget != null)
                _widgetsToHandle.Add(widget);
            else
                throw new Exception("WidgetMessageHandler::SetAsWidgetHandler(): Unable to find widget named: " + widgetName);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            if (_widgetNamesToHandle != null && _widgetNamesToHandle.Count > 0)
            {
                if(_widgetsToHandle == null)
                    _widgetsToHandle = new HashSet<Widget>();
                foreach (string widgetName in _widgetNamesToHandle)
                {
                    AddWidgetByName(widgetName);
                }
            }
        }

        public virtual bool AcceptMouseEvent(gui.MsgMouseEvent msgMouseEvt)
        {
            //if no widgets in list then handle all widgets events
            if (_widgetsToHandle == null || _widgetsToHandle.Count == 0)
                return true;

            return _widgetsToHandle.Contains(msgMouseEvt.Widget);
        }

        public abstract void OnMouseEvent(gui.MsgMouseEvent msgMouseEvt);

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(gui.MsgMouseEvent), _msgHandler);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(gui.MsgMouseEvent), _msgHandler);
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
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
    }
}
