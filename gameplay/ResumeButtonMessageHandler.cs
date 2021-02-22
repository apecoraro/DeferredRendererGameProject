using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gui;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    class ResumeButtonMessageHandler : gui.WidgetMessageHandler
    {
        class WidgetActivator : core.ComponentVisitor
        {
            public bool Modify = false;
            public bool Activate = false;
            public bool IsActive = true;
            #region ComponentVisitor Members

            public void Apply(DeskWars.core.Component comp)
            {
                gui.Widget widget = comp as gui.Widget;
                if (widget != null)
                {
                    if(Modify)
                        widget.SetIsActive(Activate, false);
                    IsActive = widget.Active;
                }
            }

            #endregion
        }

        WidgetActivator _widgetActivator = new WidgetActivator();

        public ResumeButtonMessageHandler(string widgetToHandle) : base(widgetToHandle) { init();  }
        public ResumeButtonMessageHandler(core.ComponentConfig cfg) : base(cfg) { init(); }
        public ResumeButtonMessageHandler(gui.Widget widgetToHandle) : base(widgetToHandle) { init();  }

        public void init()
        {
        }

        public override void OnMouseEvent(DeskWars.gui.MsgMouseEvent msgMouseEvt)
        {
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        core.Game.instance().UnPause();
                        _widgetActivator.Modify = true;
                        _widgetActivator.Activate = false;
                        msgMouseEvt.GameObject.VisitComponents(_widgetActivator);
                    }
                    break;
            }
        }
    }
}
