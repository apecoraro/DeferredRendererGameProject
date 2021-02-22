using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace DeskWars.gui
{
    public class MsgMouseEvent : com.GameObjectMessage
    {
        public enum EventType
        {
            MouseEnter,
            MouseExit,
            MouseMove,
            MouseLeftButtonPressed,
            MouseLeftButtonReleased
        }

        EventType _evtType;
        public EventType Type
        {
            get { return _evtType; }
        }

        Widget _widget;
        public Widget Widget
        {
            get { return _widget; }
            set { _widget = value; }
        }

        MouseState _mouseState;
        public MouseState MouseState
        {
            get { return _mouseState; }
        }

        bool _msgHandled = false;
        public bool Handled
        {
            get
            {
                return _msgHandled;
            }

            set
            {
                _msgHandled = value;
            }
        }
        
        public MsgMouseEvent(EventType type,
                             Widget widget,
                             MouseState mouseState)
        {
            _evtType = type;
            _widget = widget;
            _mouseState = mouseState;
        }
    }
}
