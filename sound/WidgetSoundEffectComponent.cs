using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.sound
{
    class WidgetSoundEffectComponent : SoundComponent
    {
        class WidgetSoundEffectMessageHandler : gui.WidgetMessageHandler
        {
            WidgetSoundEffectComponent _player = null;
            public WidgetSoundEffectMessageHandler(WidgetSoundEffectComponent player, core.ComponentConfig config) :
                base(config)
            {
                _player = player;
            }

            public WidgetSoundEffectMessageHandler(WidgetSoundEffectComponent player, string widgetToHandle) :
                base(widgetToHandle)
            {
                _player = player;
            }

            public WidgetSoundEffectMessageHandler(WidgetSoundEffectComponent player, gui.Widget widget) :
                base(widget)
            {
                _player = player;
            }

            public override void OnMouseEvent(DeskWars.gui.MsgMouseEvent msgMouseEvt)
            {
                if (msgMouseEvt.Type == DeskWars.gui.MsgMouseEvent.EventType.MouseLeftButtonReleased)
                {
                    Request req = new Request();
                    req.RequestedAction = Request.Action.START;
                    req.SoundName = "MouseButtonReleased";
                    _player.AddRequest(req);
                }
            }
        }

        WidgetSoundEffectMessageHandler _widgetMessageHandler;
        
        public WidgetSoundEffectComponent(core.ComponentConfig config)
        {
            _widgetMessageHandler = new WidgetSoundEffectMessageHandler(this, config);
        }

        public WidgetSoundEffectComponent(string widgetToHandle)
        {
            _widgetMessageHandler = new WidgetSoundEffectMessageHandler(this, widgetToHandle);
        }

        public WidgetSoundEffectComponent(gui.Widget widgetToHandle)
        {
            _widgetMessageHandler = new WidgetSoundEffectMessageHandler(this, widgetToHandle);
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            _widgetMessageHandler.OnAddToGameObject(gameObject);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            _widgetMessageHandler.OnRemoveFromGameObject(gameObject);
            base.OnRemoveFromGameObject(gameObject);
        }
    }
}
