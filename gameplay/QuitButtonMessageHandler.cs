using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gui;

namespace DeskWars.gameplay
{
    class QuitButtonMessageHandler : gui.WidgetMessageHandler
    {
        public QuitButtonMessageHandler(string widgetToHandle) : base(widgetToHandle) { }
        public QuitButtonMessageHandler(core.ComponentConfig config) : base(config) { }
        public QuitButtonMessageHandler(gui.Widget widgetToHandle) : base(widgetToHandle) { }

        public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
        {
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        core.Game.instance().Quit();
                    }
                    break;
            }
        }
    }
}
