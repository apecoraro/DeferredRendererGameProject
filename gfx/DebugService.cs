using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    class DebugService : core.Component, core.DebugService
    {
        class TextLine
        {
            public float lifeTime;
            public core.GameObject line;
            public Vector2 wh;
        };
        List<TextLine> _textLines;
        SpriteFont _font;
        float _xOffset = 0.01f;
        float _yOffset = 0.05f;

        public DebugService(SpriteFont font,
                            float xoffset, float yoffset)
        {
            _textLines = new List<TextLine>();
            _font = font;
            _xOffset = xoffset;
            _yOffset = yoffset;
            if (core.Game.instance().Services.GetService(typeof(core.DebugService)) != null)
                core.Game.instance().Services.RemoveService(typeof(core.DebugService));

            core.Game.instance().Services.AddService(typeof(core.DebugService), this);
        }

        public int AddDebugText(string text, float lifetime)
        {
            core.Game game = core.Game.instance();

            core.GameObjectFactory factory =
                                (core.GameObjectFactory)game.Services.GetService(typeof(core.GameObjectFactory));


            TextLine tl = new TextLine();
            tl.line = factory.CreateGameObject("DebugText", 
                                               "DebugText" + _textLines.Count().ToString());
            tl.lifeTime = lifetime;

            tl.wh = _font.MeasureString(text);
            if (_textLines.Count() > 0)
            {

                tl.line.Position = new Vector3(_xOffset,
                                                 _textLines[_textLines.Count - 1].line.Position.Y + 
                                                (_textLines[_textLines.Count - 1].wh.Y/game.GraphicsDevice.Viewport.Height),
                                                 0.0f);
            }
            else
            {
                tl.line.Position = new Vector3(_xOffset,
                                               _yOffset,
                                               0.0f);
            }
            
            tl.line.LookAt = Vector3.UnitZ;//do i need to do this?

            gfx.Text2DComponent.TextUpdater updater = new gfx.Text2DComponent.TextUpdater(text);
            tl.line.VisitComponents(updater);
            
            _textLines.Add(tl);

            return _textLines.Count() - 1;
        }

        public void DeleteDebugText(int id)
        {
            core.Game game = core.Game.instance();
            core.GameObjectFactory factory =
                                (core.GameObjectFactory)game.Services.GetService(typeof(core.GameObjectFactory));

            factory.DeleteGameObject(_textLines[id].line);

            _textLines.RemoveAt(id);
        }

        public override void Update(core.GameObject gameObject, GameTime gameTime)
        {
            float x = _xOffset;
            float y = _yOffset;
            for(int i = 0; i <  _textLines.Count(); ++i)
            {
                if (_textLines[i].lifeTime != -1.0f)
                {
                    _textLines[i].lifeTime -= (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
                    if (_textLines[i].lifeTime > 0.0f)
                    {
                        _textLines[i].line.Position = new Vector3(x, y, 0.0f);
                        y += (_textLines[i].wh.Y / core.Game.instance().GraphicsDevice.Viewport.Height);
                    }
                    else
                    {
                        DeleteDebugText(i);
                        --i;
                    }
                }
            }
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
            //throw new NotImplementedException();
        }
    }
}
