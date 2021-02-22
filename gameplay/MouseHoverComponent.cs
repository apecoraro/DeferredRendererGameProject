using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    class MouseHoverComponent : core.Component, com.MessageHandler
    {
        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(com.MsgMouseOver), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(com.MsgMouseOver), this);
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
        }

        public override void OnGameObjectActivated(DeskWars.core.GameObject gameObject)
        {
        }

        public override void OnGameObjectDeactivated(DeskWars.core.GameObject gameObject)
        {
        }
        
        public core.GameObject MouseOverObject = null;
        public gameplay.UnitComponent UnitComponent = null;
        float _secondsMouseOver = 0.0f;
        gfx.com.MsgBillboard _bbMsg = new DeskWars.gfx.com.MsgBillboard();
        public MouseHoverComponent()
        {
            DeskWars.gfx.ps.Billboard.BillboardData bbData =
                new DeskWars.gfx.ps.Billboard.BillboardData(Color.White, Vector2.Zero);
            _bbMsg.Billboards.Add(bbData);
            _bbMsg.Billboards.Add(bbData);
        }
        public override void Update(DeskWars.core.GameObject gameObject, Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (MouseOverObject != null)
            {
                _secondsMouseOver += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_secondsMouseOver > 0.05f)
                {
                    Vector3 center = MouseOverObject.BoundingSphere.Center;
                    float radius = MouseOverObject.BoundingSphere.Radius;
                    gameObject.Position = center + (radius * Vector3.UnitY * 1.25f);
                
                    _bbMsg.Enable = true;

                    float healthWidth = 50.0f * UnitComponent.UnitStats.health * 0.01f;
                    if (UnitComponent.UnitStats.health < 0.0f || MouseOverObject.IsActive == false)
                        healthWidth = 0.0f;

                    DeskWars.gfx.ps.Billboard.BillboardData bbData =
                        new DeskWars.gfx.ps.Billboard.BillboardData(new Color(Color.Red, 0.25f), 
                                                     new Vector2(healthWidth,
                                                                 10.0f));
                    bbData.OffsetXY.X -= ((50.0f - healthWidth)*0.5f);
                    _bbMsg.Billboards[0] = bbData;

                    bbData.Color = new Color(Color.Black, 0.25f);
                    bbData.WidthHeight = new Vector2(52.0f,
                                                     12.0f);
                    bbData.OffsetXY.X = 0.0f;

                    _bbMsg.Billboards[1] = bbData;

                    gameObject.PostOffice.Send(_bbMsg);
                }
            }
            else
            {
                if (_bbMsg.Enable)
                {
                    _bbMsg.Enable = false;
                    gameObject.PostOffice.Send(_bbMsg);
                }
                _secondsMouseOver = 0.0f;
            }
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            com.MsgMouseOver mouseOverMsg = (com.MsgMouseOver)msg;
            MouseOverObject = mouseOverMsg.MouseOverObject;
            UnitComponent = mouseOverMsg.UnitComponent;
        }

        #endregion
    }
}
