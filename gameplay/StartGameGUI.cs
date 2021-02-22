using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    class StartGameGUI : gui.Widget
    {
        class StartGameGUIMouseEventHandler : gui.WidgetMessageHandler
        {
            StartGameGUI _startGameGUI = null;

            public StartGameGUIMouseEventHandler(StartGameGUI startGameGUI, core.ComponentConfig config) :
                base(config)
            {
                _startGameGUI = startGameGUI;
            }

            public override void OnMouseEvent(DeskWars.gui.MsgMouseEvent msgMouseEvt)
            {
                if (msgMouseEvt.Type == 
                    gui.MsgMouseEvent.EventType.MouseLeftButtonReleased)
                {
                    _startGameGUI.Advance();
                }
            }
        }

        Texture2D[] _textures;
        static int _currentTexture = 0;
        gui.TextureWidget _textureWidget = null;
        StartGameGUIMouseEventHandler _guiEvtHandler = null;
        float _timeInCurrentTexture = 0.0f;

        public StartGameGUI(Texture2D digiPenLogo,
                            Texture2D topGamesLogo,
                            Texture2D deskWarsLogo,
                            Texture2D instructionImage,
                            core.ComponentConfig config) :
            base(config)
        {
            _textures = new Texture2D[4];

            _textures[0] = digiPenLogo;
            _textures[1] = topGamesLogo;
            _textures[2] = deskWarsLogo;
            _textures[3] = instructionImage;

            _textureWidget = new gui.TextureWidget("StartGameGUITexture", 
                                                   this.Width, 
                                                   this.Height, 
                                                   -1.0f, 
                                                   _textures[_currentTexture], 
                                                   Color.White);

            this.AddChild(_textureWidget);

            _guiEvtHandler = new StartGameGUIMouseEventHandler(this, config);
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.AddComponent(_guiEvtHandler);

            base.OnAddToGameObject(gameObject);
        }

        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
            _timeInCurrentTexture += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timeInCurrentTexture > 2.0f && _currentTexture < 2)//only display the first two textures for 2 seconds
                Advance();
            base.Update(gameObject, gameTime);
        }

        public void Advance()
        {
            ++_currentTexture;
            _timeInCurrentTexture = 0.0f;
            if (_currentTexture == 4)
            {
                //next time skip the digipen logo and top games logo
                _currentTexture = 2;
                com.MsgLoadLevel msg = new com.MsgLoadLevel();
                core.Game.instance().PostOffice.Send(msg);
            }
            else
            {
                _textureWidget.Texture = _textures[_currentTexture];
            }
        }
    }
}
