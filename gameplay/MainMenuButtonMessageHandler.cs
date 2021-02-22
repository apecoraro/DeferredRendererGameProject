using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gui;
using Microsoft.Xna.Framework;

namespace DeskWars.gameplay
{
    class MainMenuButtonMessageHandler : gui.WidgetMessageHandler
    {
        core.GameObject _mainMenu = null;
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

        public MainMenuButtonMessageHandler(string widgetToHandle) : base(widgetToHandle) { init();  }
        public MainMenuButtonMessageHandler(core.ComponentConfig cfg) : base(cfg) { init(); }
        public MainMenuButtonMessageHandler(gui.Widget widgetToHandle) : base(widgetToHandle) { init();  }

        public void init()
        {
            core.GameObjectFactory factory =
             (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));

            _mainMenu = factory.CreateGameObject("MainMenu", "MainMenu");
            _widgetActivator.Modify = true;
            _widgetActivator.Activate = false;
            _mainMenu.VisitComponents(_widgetActivator);
            _mainMenu.Position = new Vector3(0.25f, 0.25f, 0.0f);
        }

        public override void OnMouseEvent(DeskWars.gui.MsgMouseEvent msgMouseEvt)
        {
            switch (msgMouseEvt.Type)
            {
                case MsgMouseEvent.EventType.MouseLeftButtonReleased:
                    {
                        core.Game.instance().Pause();
                        _widgetActivator.Modify = false;
                        _mainMenu.VisitComponents(_widgetActivator);
                        if (!_widgetActivator.IsActive)
                        {
                            _widgetActivator.Modify = true;
                            _widgetActivator.Activate = true;
                            _mainMenu.VisitComponents(_widgetActivator);
                        }
                    }
                    break;
            }
        }
    }
}
