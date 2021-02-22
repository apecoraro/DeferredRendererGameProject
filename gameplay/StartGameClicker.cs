using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gui;

namespace DeskWars.gameplay
{
    class StartGameClicker : WidgetMessageHandler
    {
        public StartGameClicker(core.ComponentConfig config) : base(config) { }
        public StartGameClicker(string widgetToHandle) : base(widgetToHandle) { }

        public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
        {
            TextureWidget rectWidget = (TextureWidget)msgMouseEvt.Widget;
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        com.MsgLoadLevel msg = new com.MsgLoadLevel();
                        core.Game.instance().PostOffice.Send(msg);
                    }
                    break;
            }
        }
    }
}
