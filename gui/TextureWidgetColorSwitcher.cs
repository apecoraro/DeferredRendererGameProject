using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gui
{
    class TextureWidgetColorSwitcher : WidgetMessageHandler
    {
        Color _mouseOverColor = Color.LightBlue;
        Color _mouseOutColor = Color.White;
        Color _mouseBtnPressedColor = Color.LawnGreen;

        public TextureWidgetColorSwitcher(core.ComponentConfig config)
            : base(config)
        {
        }

        public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
        {
            TextureWidget rectWidget = (TextureWidget)msgMouseEvt.Widget;
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseEnter:
                    {
                        rectWidget.TintColor = _mouseOverColor;
                    }
                    break;
                case MsgMouseEvent.EventType.MouseExit:
                    {
                        rectWidget.TintColor = _mouseOutColor;
                    }
                    break;
                case MsgMouseEvent.EventType.MouseLeftButtonPressed:
                    {
                        rectWidget.TintColor = _mouseBtnPressedColor;
                    }
                    break;
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        rectWidget.TintColor = _mouseOverColor;
                    }
                    break;
            }
        }
    }
}
