using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gui;

namespace DeskWars.gameplay
{
    class RepeatLevelClicker : WidgetMessageHandler
    {
        public RepeatLevelClicker(core.ComponentConfig config) : base(config) { }
        public RepeatLevelClicker(string widgetToHandle) : base(widgetToHandle) { }

        public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
        {
            TextureWidget rectWidget = (TextureWidget)msgMouseEvt.Widget;
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        com.MsgReloadLevel msg = new com.MsgReloadLevel();
                        core.Game.instance().PostOffice.Send(msg);
                    }
                    break;
            }
        }
    }
}
