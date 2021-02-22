using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeskWars.gui
{
    class Engine : core.Renderer, core.Engine
    {
        #region Engine Members

        Widget _focusWidget = null;

        SpriteBatch _spriteBatch;
        SpriteFont _font;

        public void Configure(string configFile)
        {        
        }

        public void LoadContent(DeskWars.core.Game game)
        {
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);
            _font = game.Content.Load<SpriteFont>("Fonts\\Kootenay");
        }

        public void CreateGameObjectComponents(DeskWars.core.GameObjectConfig config, DeskWars.core.GameObject gameObject)
        {
            for (int i = 0; i < config.GetComponentConfigCount(); ++i)
            {
                core.ComponentConfig compCfg = config.GetComponentConfig(i);
                CreateGameObjectComponent(compCfg, gameObject);
            }
        }

        public void CreateGameObjectComponent(DeskWars.core.ComponentConfig config, DeskWars.core.GameObject gameObject)
        {
            core.Component comp = CreateComponent(config);
            if (comp != null)
            {
                gameObject.AddComponent(comp);
            }
        }

        public DeskWars.core.Component CreateComponent(DeskWars.core.ComponentConfig config)
        {
            core.Game game = core.Game.instance();
            core.Component comp = null;
            switch (config.Name)
            {
                case "TextureWidget":
                    {
                        string textureFile = config.GetParameterValue("Texture");
                        Texture2D texture = game.Content.Load<Texture2D>(textureFile);

                        gui.TextureWidget guiComp = new gui.TextureWidget(texture, config);
                        comp = guiComp;
                    }
                    break;
                
                case "TextWidget":
                    {
                        gui.TextWidget guiComp = new gui.TextWidget(_font, config);
                        comp = guiComp;
                    }
                    break;
                case "MouseIcon":
                    {
                        string textureFile = config.GetParameterValue("Texture");
                        Texture2D texture = game.Content.Load<Texture2D>(textureFile);

                        gui.MouseIcon guiComp = new gui.MouseIcon(texture, config);
                        comp = guiComp;
                    }
                    break;
                case "TextureWidgetColorSwitcher":
                    {
                        TextureWidgetColorSwitcher colorSwitcher = new TextureWidgetColorSwitcher(config);
                        comp = colorSwitcher;
                    }
                    break;
            }

            return comp;
        }

        public void Update(DeskWars.core.GameObjectDB gameObjectDB, 
                           Microsoft.Xna.Framework.GameTime gameTime)
        {
            for (int i = 0; i < Widget.RootWidget.Children.Count;  ++i)
            {
                Widget widget = Widget.RootWidget.Children[i];
                widget.UpdateScreenTransformation(Matrix.Identity);
            }

            //if mouse button is pressed
                //then set focus widget to null

            core.Game.KeyboardMouseInput input = core.Game.instance().Input;
            //TODO event handling
            MouseState mouseState = input.GetMouseState();
            for (int i = 0; i < Widget.RootWidget.Children.Count; ++i)
            {
                Widget widget = Widget.RootWidget.Children[i];
                if (HandleMouseInput(widget, mouseState))
                {
                    input.MouseInputHandled = true;
                    break;
                }
                if (widget.Active && widget.MouseIsInside())
                    input.MouseInputHandled = true;
            }
            //KeyboardState keyboardState = Keyboard.GetState();

            //if(_focusWidget != null) //then handle keyboard
        }

        public bool HandleMouseInput(Widget widget, MouseState mouseState)
        {
            foreach (Widget child in widget.Children)
            {
                if (child.Children.Count > 0)
                {
                    if (HandleMouseInput(child, mouseState))
                        return true;
                }
            }

            if (widget.Active && widget.ReceiveEvents)
            {
                //only active widgets should handle mouse
                //events
                if (widget.HandleMouseInput(mouseState))
                {
                    _focusWidget = widget;
                    return true;
                }
            }

            return false;
        }

        #endregion

        protected void DrawWidgets(List<Widget> widgets)
        {
            foreach (Widget widget in widgets)
            {
                if(widget.Active)
                    widget.Draw(_spriteBatch);

                DrawWidgets(widget.Children);
            }
        }

        #region Renderer Members

        public void Draw(DeskWars.core.GameObjectDB gameObjectDB, GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteBlendMode.AlphaBlend,
                               SpriteSortMode.BackToFront,
                               SaveStateMode.None);

            DrawWidgets(Widget.RootWidget.Children);

            _spriteBatch.End();

            _spriteBatch.GraphicsDevice.RenderState.DepthBufferEnable = true;
        }

        #endregion

        protected void DeleteWidget(List<Widget> widgets, Widget del)
        {
            foreach (Widget widget in widgets)
            {
                if (widget == del)
                {
                    widgets.Remove(del);
                    break;
                }
                else
                    DeleteWidget(widget.Children, del);
            }
        }

        #region Engine Members

        public void OnGameObjectDeleted(DeskWars.core.GameObject gameObject)
        {
            foreach(core.Component comp in gameObject.Components)
            {
                Widget widget = comp as Widget;
                if (widget != null)
                {
                    DeleteWidget(Widget.RootWidget.Children, widget);
                }
            }
        }

        #endregion

        #region Engine Members

        public void OnLoadLevel(int level)
        {
        }

        public void OnUnloadLevel(int level)
        {
            Widget.RootWidget.Children.Clear();
        }

        #endregion
    }
}
