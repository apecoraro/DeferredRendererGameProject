using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    public class MiniMapWidget : gui.Widget
    {
        class MouseEventHandler : gui.WidgetMessageHandler
        {
            com.MsgUpdateCamera _updateCamMsg = new com.MsgUpdateCamera();
            MiniMapWidget _miniMap = null;

            public MouseEventHandler(MiniMapWidget miniMap) : base("MiniMapDesk")
            {
                _miniMap = miniMap;
            }

            public override void OnMouseEvent(gui.MsgMouseEvent msgMouseEvt)
            {
                switch (msgMouseEvt.Type)
                {
                    case gui.MsgMouseEvent.EventType.MouseLeftButtonReleased:
                        {
                            Vector3 xyz = _miniMap.ConvertMouseToWorldXYZ(msgMouseEvt.MouseState.X, msgMouseEvt.MouseState.Y);
                            _updateCamMsg.LookAt = xyz;
                            core.Game.instance().PostOffice.Send(_updateCamMsg);
                        }
                        break;
                }
            }
        }
        MouseEventHandler _msgHandler = null;

        List<core.GameObject> _ownUnits;
        List<core.GameObject> _enemyUnits;

        Texture2D _ownUnitTexture;
        Texture2D _enemyUnitTexture;

        Rectangle _spriteRect = new Rectangle();

        public class OwnTeamFilter : core.ComponentTypeFilter
        {
            #region ComponentTypeFilter Members

            public bool Accept(DeskWars.core.GameObject gameObject,
                               DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                {
                    if (unit.UnitRatings.Team == "Player")
                        return true;
                }

                return false;
            }

            #endregion
        }

        public class EnemyTeamFilter : core.ComponentTypeFilter
        {
            #region ComponentTypeFilter Members

            public bool Accept(DeskWars.core.GameObject gameObject,
                               DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                {
                    if (unit.UnitRatings.Team == "Enemy")
                        return true;
                }

                return false;
            }

            #endregion
        }

        core.GameObjectTypeFilter _ownTeamFilter =
            new core.GameObjectTypeFilter(new OwnTeamFilter());

        core.GameObjectTypeFilter _enemyTeamFilter =
            new core.GameObjectTypeFilter(new EnemyTeamFilter());
        
        public MiniMapWidget(Texture2D ownUnitTexture,
                             Texture2D enemyUnitTexture,
                             core.ComponentConfig config) :
            base(config)
        {
            _msgHandler = new MouseEventHandler(this);
            _ownUnits = new List<core.GameObject>();
            _enemyUnits = new List<core.GameObject>();
            _ownUnitTexture = ownUnitTexture;
            _enemyUnitTexture = enemyUnitTexture;
        }

        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.AddComponent(_msgHandler);
            base.OnAddToGameObject(gameObject);
        }

        public Vector3 ConvertMouseToWorldXYZ(int mouseX, int mouseY)
        {
            float upperLeftX = this.BoundingRect.X;
            float upperLeftY = this.BoundingRect.Y;
            float width = this.BoundingRect.Width;
            float height = this.BoundingRect.Height;

            float deskWidth = 3000.0f;//TODO this should not be hard coded
            float deskHeight = 3000.0f;

            float scaleX = width / deskWidth;
            float scaleY = height / deskHeight;

            float originX = upperLeftX + (width * 0.5f);
            float originY = upperLeftY + (height * 0.5f);

            Vector3 world;

            world.X = (mouseX - originX) / scaleX;
            world.Z = (mouseY - originY) / scaleY;
            world.Y = 0.0f;

            return world;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float upperLeftX = this.BoundingRect.X;
            float upperLeftY = this.BoundingRect.Y;
            float width = this.BoundingRect.Width;
            float height = this.BoundingRect.Height;

            float deskWidth = 3000.0f;//TODO this should not be hard coded
            float deskHeight = 3000.0f;

            float scaleX = width / deskWidth;
            float scaleY = height / deskHeight;

            float originX = upperLeftX + (width * 0.5f);
            float originY = upperLeftY + (height * 0.5f);

            foreach (core.GameObject gameObj in _ownUnits)
            {
                _spriteRect.X = (int)((gameObj.BoundingBox.Min.X * scaleX) + originX) - 2;
                _spriteRect.Y = (int)((gameObj.BoundingBox.Max.Z * scaleY) + originY) - 2;
                if (this.BoundingRect.Contains(_spriteRect.X, _spriteRect.Y))
                {
                    float maxX = ((gameObj.BoundingBox.Max.X * scaleX) + originX);
                    float minY = ((gameObj.BoundingBox.Min.Z * scaleY) + originY);
                    _spriteRect.Width = ((int)maxX - _spriteRect.X) + 4;
                    _spriteRect.Height = (_spriteRect.Y - (int)minY) + 4;
                    spriteBatch.Draw(_ownUnitTexture, _spriteRect, null, Color.White,
                                 0.0f,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 this.Depth - 0.1f);
                }
            }

            foreach (core.GameObject gameObj in _enemyUnits)
            {
                _spriteRect.X = (int)((gameObj.BoundingBox.Min.X * scaleX) + originX) - 2;
                _spriteRect.Y = (int)((gameObj.BoundingBox.Max.Z * scaleY) + originY) - 2;
                if (this.BoundingRect.Contains(_spriteRect.X, _spriteRect.Y))
                {
                    float maxX = ((gameObj.BoundingBox.Max.X * scaleX) + originX);
                    float minY = ((gameObj.BoundingBox.Min.Z * scaleY) + originY);
                    _spriteRect.Width = ((int)maxX - _spriteRect.X) + 4;
                    _spriteRect.Height = (_spriteRect.Y - (int)minY) + 4;

                    spriteBatch.Draw(_enemyUnitTexture, _spriteRect, null, Color.White,
                                 0.0f,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 this.Depth - 0.1f);
                }
            }
        }

        public override void Update(core.GameObject gameObject, GameTime gameTime)
        {
            core.GameObjectDB gameObjectDB = core.Game.instance().GameObjectDB;

            _ownUnits.Clear();
            gameObjectDB.GetGameObjectsByType(_ownTeamFilter, _ownUnits);

            _enemyUnits.Clear();
            gameObjectDB.GetGameObjectsByType(_enemyTeamFilter, _enemyUnits);
        }

    }
}
