using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gameplay
{
    class KeyboardMouseController : core.Component, com.MessageHandler
    {
        int prevScrollWheelValue = 0;
        float _keyboardMoveScalar = 1.0f;
        float _mouseMoveScalar = 1.0f;
        float _mouseRotateScalar = 1.0f;
        MouseState _prevMouseState = new MouseState();
        MouseState _clickMouseState = new MouseState();
        Vector3 _clickMousePos;
        int _maxSelectedUnits = 4;
        gui.TextureWidget _selectObjectsBox = null;
        List<core.GameObject> _multiSelectedUnits = new List<core.GameObject>();
        List<core.GameObject> _multiHaloUnits = new List<core.GameObject>();
        gameplay.UnitComponent _selectedObjectUnit = null;
        core.GameObject _mouseHoverObject = null;
        com.MsgObjectSelected _msgObjSelected = new com.MsgObjectSelected(null);
        com.MsgUpdateCamera _updateCameraMsg = null;
        sound.com.MsgSoundEffectRequest _soundEffectRequest = new sound.com.MsgSoundEffectRequest();
        core.GameObject _lastAttacker = null;
        core.GameObject _lastAttackedObject = null;
        int _attackSound = 1;
        com.MsgUpdateMouseIcon _updateMouseIconMsg = new com.MsgUpdateMouseIcon();
        com.MsgMouseOver _msgMouseOver = new com.MsgMouseOver();
                        
        SpriteBatch _spriteBatch;
        Texture2D _defaultMouse;
        Texture2D _movementMouse;
        Texture2D _attackMouse;
        Texture2D _selectMouse;
        Texture2D _attackMouseFail;
        BoundingSphere cameraBounds;
        BoundingBox lookAtBounds;

        float _width;
        float _height;

        public class PlayerTeamFilter : core.ComponentTypeFilter
        {
            #region ComponentTypeFilter Members

            public bool Accept(DeskWars.core.GameObject gameObject,
                               DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                {
                    if (unit.UnitRatings.Team == "Player")
                    {
                        return true;
                    }
                }
                return false;
            }

            #endregion
        }

        PlayerTeamFilter _playerTeamFilter = new PlayerTeamFilter();
        class UnitComponentFinder : core.ComponentVisitor
        {
            #region ComponentVisitor Members

            public void Apply(DeskWars.core.Component comp)
            {
                gameplay.UnitComponent unit = comp as gameplay.UnitComponent;
                if (unit != null)
                    UnitComp = unit;
            }

            public void Reset()
            {
                UnitComp = null;
            }

            public gameplay.UnitComponent UnitComp = null;

            #endregion
        }
        UnitComponentFinder _findUnitComponent = new UnitComponentFinder();

        class HaloComponentFinder : core.ComponentVisitor
        {
            #region ComponentVisitor Members

            public void Apply(DeskWars.core.Component comp)
            {
                gameplay.HaloComponent halo = comp as gameplay.HaloComponent;
                if (halo != null)
                    HaloComp = halo;
            }

            public void Reset()
            {
                HaloComp = null;
            }

            public gameplay.HaloComponent HaloComp = null;

            #endregion
        }
        HaloComponentFinder _findHaloComponent = new HaloComponentFinder();

        core.GameObjectTypeFilter _playerTeamGameObjFilter = null;

        core.ComponentExactTypeFilter _deskCompFilter =
             new core.ComponentExactTypeFilter(typeof(gameplay.DeskComponent));

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

        EnemyTeamFilter _enemyTeamFilter = new EnemyTeamFilter();

        core.GameObjectTypeFilter _enemyTeamGameObjFilter = null;

        public KeyboardMouseController(Texture2D defaultMouse, 
                                       Texture2D movementMouse, 
                                       Texture2D attackMouse,
                                        Texture2D selectMouse,
                                        Texture2D attackMouseFail)
        {
            _spriteBatch = new SpriteBatch(core.Game.instance().GraphicsDevice);
            _defaultMouse = defaultMouse;
            _movementMouse = movementMouse;
            _attackMouse = attackMouse;
            _selectMouse = selectMouse;
            _attackMouseFail = attackMouseFail;
            _enemyTeamGameObjFilter =
                new core.GameObjectTypeFilter(_enemyTeamFilter);

            _playerTeamGameObjFilter =
                new core.GameObjectTypeFilter(_playerTeamFilter);

            // Set Camera Bounding Sphere
            Vector3 _cameraBSCenter;
            float _cameraBSRadius;
            _cameraBSCenter.X = 0.0f;
            _cameraBSCenter.Y = 0.0f;
            _cameraBSCenter.Z = 0.0f;
            _cameraBSRadius = 2500.0f;
            _soundEffectRequest.Request = new DeskWars.sound.SoundComponent.Request();
            cameraBounds = new BoundingSphere(_cameraBSCenter, _cameraBSRadius);
            lookAtBounds.Max.X = 2500.0f;
            lookAtBounds.Max.Y = 2500.0f;
            lookAtBounds.Max.Z = 2500.0f;
            lookAtBounds.Min.X = -2500.0f;
            lookAtBounds.Min.Y = -2500.0f;
            lookAtBounds.Min.Z = -2500.0f;

            // Set window height/width
            GraphicsDevice dev = core.Game.instance().GraphicsDevice;
            _width = dev.Viewport.Width;
            _height = dev.Viewport.Height;
        }

        public void SetMoveScalars(float keyboardMoveScalar, float mouseMoveScalar, float mouseRotateScalar)
        {
            
            _keyboardMoveScalar = keyboardMoveScalar;
            _mouseMoveScalar = mouseMoveScalar;
            _mouseRotateScalar = mouseRotateScalar;
        }

        protected void HandleKeyboardInput(core.Game.KeyboardMouseInput input,
                                           core.GameObject gameObject)
        {
            KeyboardState keyState = input.GetKeyboardState();

/*            if (keyState.IsKeyDown(Keys.F2))
            {
                com.MsgReloadLevel msg = new com.MsgReloadLevel();
                core.Game.instance().PostOffice.Send(msg);
            } */
            if (keyState.IsKeyDown(Keys.F3))
            {
                core.Game.instance().ToggleFullscreen();

                // Update height and width values.
                GraphicsDevice dev = core.Game.instance().GraphicsDevice;
                _width = dev.Viewport.Width;
                _height = dev.Viewport.Height;
            }
            else if (keyState.IsKeyDown(Keys.D) && _multiSelectedUnits.Count != 0)
            {
                // Send message to toggle Patrol state.
                com.MsgPatrol msg = new com.MsgPatrol();

                foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                {
                    selectedUnit.PostOffice.Send(msg);
                }
            }
            else if (keyState.IsKeyDown(Keys.W))
            {
                float diff = 10.0f;
                Vector3 moveVector = (gameObject.LookVector * diff);
                Vector3 nextPos = gameObject.Position + moveVector;

                if (cameraBounds.Contains(nextPos) == ContainmentType.Contains && nextPos.Y >= 10.0f)
                {
                    // Zoom around selected object
                    Vector3 lookAt = gameObject.LookAt + moveVector;

                    gameObject.SetPositionAndLookAt(nextPos, lookAt);
                }
            }
            else if (keyState.IsKeyDown(Keys.S))
            {
                float diff = -10.0f;
                Vector3 moveVector = (gameObject.LookVector * diff);
                Vector3 nextPos = gameObject.Position + moveVector;

                if (cameraBounds.Contains(nextPos) == ContainmentType.Contains && nextPos.Y >= 10.0f)
                {
                    // Zoom around selected object
                    Vector3 lookAt = gameObject.LookAt + moveVector;

                    gameObject.SetPositionAndLookAt(nextPos, lookAt);
                }
            }
            else if (keyState.IsKeyDown(Keys.A))
            {
                float diff = -10.0f;
                Vector3 moveVector = (gameObject.RightVector * diff);
                Vector3 nextPos = gameObject.Position + moveVector;

                gameObject.SetPositionAndLookAt(nextPos, gameObject.LookAt);
            }
            else if (keyState.IsKeyDown(Keys.D))
            {
                float diff = 10.0f;
                Vector3 moveVector = (gameObject.RightVector * diff);
                Vector3 nextPos = gameObject.Position + moveVector;

                gameObject.SetPositionAndLookAt(nextPos, gameObject.LookAt);
            }
        }

        protected void HandleMouseInput(core.Game.KeyboardMouseInput input,
                                        core.GameObject gameObject,
                                        GameTime gameTime)
        {
            KeyboardState keyState = input.GetKeyboardState();

            MouseState mouseState = input.GetMouseState();

            // Update the mouse icon
            //Do pick
            core.Game game = core.Game.instance();
            core.PickService svc =
                (core.PickService)game.Services.GetService(typeof(core.PickService));
            Ray ray = svc.ComputeRayFromScreenXY(mouseState.X, mouseState.Y);
            core.GameObjectDB gameObjDB = game.GameObjectDB;

            // Filter to check if attacking an enemy
            core.GameObject unit = gameObjDB.GetClosestGameObjectIntersectRay(ray, _enemyTeamGameObjFilter);
                
            core.GameObject desk = null;
            _playerTeamGameObjFilter.Filter = _deskCompFilter;
            int index = 0;
            desk = gameObjDB.GetGameObject(ref index, _playerTeamGameObjFilter);

            // Filter to check for selecting unit
            _playerTeamGameObjFilter.Filter = _playerTeamFilter;
            core.GameObject newSelObject = null;
            gameplay.UnitComponent newSelObjUnitComp = null;

            if (mouseState.LeftButton == ButtonState.Pressed )
            {
                if (_prevMouseState.LeftButton != ButtonState.Pressed)
                {
                    _clickMouseState = mouseState;

                    if (desk != null)
                    {
                        gfx.RayTriangleIntersector isector = new gfx.RayTriangleIntersector(ray);
                        desk.VisitComponents(isector);
                        if (isector.FoundIntersection())
                        {
                            _clickMousePos = isector.GetIntersection();
                        }
                    }
                }
                else
                {
                    int x;
                    int w;
                    if(_clickMouseState.X < mouseState.X)
                    {
                        x = _clickMouseState.X;
                        w = mouseState.X - _clickMouseState.X;
                    }
                    else
                    {
                        x = mouseState.X;
                        w = _clickMouseState.X - mouseState.X;
                    }

                    int y;
                    int h;
                    if(_clickMouseState.Y < mouseState.Y)
                    {
                        y = _clickMouseState.Y;
                        h = mouseState.Y - _clickMouseState.Y;
                    }
                    else
                    {
                        y = mouseState.Y;
                        h = _clickMouseState.Y - mouseState.Y;
                    }

                    _selectObjectsBox.SetScreenCoords(x, y, w, h);
                    _selectObjectsBox.SetIsActive(true, true);
                }

            }

            if (unit == null)
            {
                newSelObject = gameObjDB.GetClosestGameObjectIntersectRay(ray, _playerTeamGameObjFilter);
                _msgMouseOver.MouseOverObject = newSelObject;
                if (newSelObject != null)
                {
                    _findUnitComponent.Reset();
                    newSelObject.VisitComponents(_findUnitComponent);
                    newSelObjUnitComp = _findUnitComponent.UnitComp;
                    _msgMouseOver.UnitComponent = newSelObjUnitComp;
                }
            }
            else
            {
                _msgMouseOver.MouseOverObject = unit;
                _findUnitComponent.Reset();
                unit.VisitComponents(_findUnitComponent);
                _msgMouseOver.UnitComponent = _findUnitComponent.UnitComp;
            }
            _mouseHoverObject.PostOffice.Send(_msgMouseOver);

            // Filter to check for unit movement    
            if (unit != null && _multiSelectedUnits.Count != 0)
            {
                
                if (_multiSelectedUnits.Count == 1 && _multiSelectedUnits.First().Class == "Stapler")
                {
                    if (_selectedObjectUnit != null)
                    {
                        double dist = (unit.Position - _multiSelectedUnits.First().Position).Length();

                        if (dist < _selectedObjectUnit.UnitRatings.AttackRange * 0.1f)
                        {
                            _updateMouseIconMsg.Texture = _attackMouseFail;
                            core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                        }
                        else
                        {
                            _updateMouseIconMsg.Texture = _attackMouse;
                            core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                        }
                    }
                }
                else if (_updateMouseIconMsg.Texture != _attackMouse)
                {
                    _updateMouseIconMsg.Texture = _attackMouse;
                    core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                }
            }
            else if (newSelObject != null)
            {
                if (_updateMouseIconMsg.Texture != _selectMouse)
                {
                    _updateMouseIconMsg.Texture = _selectMouse;
                    core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                }
            }
            else if (desk != null)
            {
                if (_updateMouseIconMsg.Texture != _movementMouse)
                {
                    _updateMouseIconMsg.Texture = _movementMouse;
                    core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                }
            }
            else
            {
                if (_updateMouseIconMsg.Texture != _defaultMouse)
                {
                    _updateMouseIconMsg.Texture = _defaultMouse;
                    core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                }
            }

            // Screen Edge Scrolling
            if (mouseState.X >= _width - (_width * 0.01))
            {
                // Scroll right
                Vector3 lookLevel = new Vector3(gameObject.LookVector.X, 0.0f, gameObject.LookVector.Z);
                Vector3 newLook = gameObject.LookAt - (gameObject.RightVector * _mouseMoveScalar);

                if (lookAtBounds.Contains(newLook) == ContainmentType.Contains)
                {
                    gameObject.Position = gameObject.Position - (gameObject.RightVector * _mouseMoveScalar);
                    gameObject.LookAt = newLook;

                    cameraBounds.Center = newLook;
                }
            }
            else if (mouseState.X <= 0 + (_width * 0.01))
            {
                // Scroll left
                Vector3 lookLevel = new Vector3(gameObject.LookVector.X, 0.0f, gameObject.LookVector.Z);
                Vector3 newLook = gameObject.LookAt + (gameObject.RightVector * _mouseMoveScalar);

                if (lookAtBounds.Contains(newLook) == ContainmentType.Contains)
                {
                    gameObject.Position = gameObject.Position + (gameObject.RightVector * _mouseMoveScalar);
                    gameObject.LookAt = newLook;

                    cameraBounds.Center = newLook;
                }
            }
            //core.Game game = core.Game.instance();
            //core.DebugService debugSvc = (core.DebugService)game.Services.GetService(typeof(core.DebugService));
            //debugSvc.AddDebugText("Mouse: " + mouseState.X.ToString() + ", " + mouseState.Y.ToString(), 0.1f);
            if (mouseState.Y >= _height - (_height * 0.01))
            {
                // Scroll down
                Vector3 lookLevel = new Vector3(gameObject.LookVector.X, 0.0f, gameObject.LookVector.Z);
                Vector3 newLook = gameObject.LookAt - (lookLevel * _mouseMoveScalar);

                if (lookAtBounds.Contains(newLook) == ContainmentType.Contains)
                {
                    gameObject.Position = gameObject.Position - (lookLevel * _mouseMoveScalar);
                    gameObject.LookAt = newLook;

                    cameraBounds.Center = newLook;
                }
            }
            else if (mouseState.Y <= 0 + (_height * 0.01))
            {
                // Scroll up
                Vector3 lookLevel = new Vector3(gameObject.LookVector.X, 0.0f, gameObject.LookVector.Z);
                Vector3 newLook = gameObject.LookAt + (lookLevel * _mouseMoveScalar);

                if (lookAtBounds.Contains(newLook) == ContainmentType.Contains)
                {
                    gameObject.Position = gameObject.Position + (lookLevel * _mouseMoveScalar);
                    gameObject.LookAt = newLook;

                    cameraBounds.Center = newLook;
                }
            }

            if (_prevMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                if (_clickMouseState.X - mouseState.X != 0 && _clickMouseState.Y - mouseState.Y != 0)
                {
                    if (!keyState.IsKeyDown(Keys.LeftShift) ||
                        (keyState.IsKeyDown(Keys.LeftShift) && _multiSelectedUnits.Count() < _maxSelectedUnits))
                    {
                        if (desk != null)
                        {
                            
                            Vector3 upLeft, downLeft, upRight, downRight, center;
                            Ray upLeftRay, downLeftRay, upRightRay, downRightRay, centerRay;

                            if (_clickMouseState.X < mouseState.X && _clickMouseState.Y < mouseState.Y)
                            {
//                                upLeft.X = _clickMouseState.X;
//                                upLeft.Y = _clickMouseState.Y;
                                upLeftRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, _clickMouseState.Y);

//                                downRight.X = mouseState.X;
//                                downRight.Y = mouseState.Y;
                                downRightRay = svc.ComputeRayFromScreenXY(mouseState.X, mouseState.Y);

//                                downLeft.X = _clickMouseState.X;
//                                downLeft.Y = mouseState.Y;
                                downLeftRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, mouseState.Y);

//                                upRight.X = mouseState.X;
//                                upRight.Y = _clickMouseState.Y;
                                upRightRay = svc.ComputeRayFromScreenXY(mouseState.X, _clickMouseState.Y);

                                int centX = _clickMouseState.X + (int)(0.5f * (mouseState.X - _clickMouseState.X));
                                int centY = _clickMouseState.Y + (int)(0.5f * (mouseState.Y - _clickMouseState.Y));
                                centerRay = svc.ComputeRayFromScreenXY(centX, centY);
                            }
                            else if (_clickMouseState.X < mouseState.X && _clickMouseState.Y > mouseState.Y)
                            {
//                                downLeft.X = _clickMouseState.X;
//                                downLeft.Y = _clickMouseState.Y;
                                downLeftRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, _clickMouseState.Y);

//                                upRight.X = mouseState.X;
//                                upRight.Y = mouseState.Y;
                                upRightRay = svc.ComputeRayFromScreenXY(mouseState.X, mouseState.Y);

//                                upLeft.X = _clickMouseState.X;
//                                upLeft.Y = mouseState.Y;
                                upLeftRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, mouseState.Y);

//                                downRight.X = mouseState.X;
//                                downRight.Y = _clickMouseState.Y;
                                downRightRay = svc.ComputeRayFromScreenXY(mouseState.X, _clickMouseState.Y);

                                int centX = _clickMouseState.X + (int)(0.5f * (mouseState.X - _clickMouseState.X));
                                int centY = mouseState.Y + (int)(0.5f * (_clickMouseState.Y - mouseState.Y));
                                centerRay = svc.ComputeRayFromScreenXY(centX, centY);
                            }
                            else if (_clickMouseState.X > mouseState.X && _clickMouseState.Y < mouseState.Y)
                            {
//                                upRight.X = _clickMouseState.X;
//                                upRight.Y = _clickMouseState.Y;
                                upRightRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, _clickMouseState.Y);

//                                downLeft.X = mouseState.X;
//                                downLeft.Y = mouseState.Y;
                                downLeftRay = svc.ComputeRayFromScreenXY(mouseState.X, mouseState.Y);

//                                upLeft.X = mouseState.X;
//                                upLeft.Y = _clickMouseState.Y;
                                upLeftRay = svc.ComputeRayFromScreenXY(mouseState.X, _clickMouseState.Y);

//                                downRight.X = _clickMouseState.X;
//                                downRight.Y = mouseState.Y;
                                downRightRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, mouseState.Y);

                                int centX = mouseState.X + (int)(0.5f * (_clickMouseState.X - mouseState.X));
                                int centY = _clickMouseState.Y + (int)(0.5f * (mouseState.Y - _clickMouseState.Y));
                                centerRay = svc.ComputeRayFromScreenXY(centX, centY);
                            }
                            else
                            {
//                                downRight.X = _clickMouseState.X;
//                                downRight.Y = _clickMouseState.Y;
                                downRightRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, _clickMouseState.Y);

//                                upLeft.X = mouseState.X;
//                                upLeft.Y = mouseState.Y;
                                upLeftRay = svc.ComputeRayFromScreenXY(mouseState.X, mouseState.Y);

//                                downLeft.X = mouseState.X;
//                                downLeft.Y = _clickMouseState.Y;
                                downLeftRay = svc.ComputeRayFromScreenXY(mouseState.X, _clickMouseState.Y);

//                                upRight.X = _clickMouseState.X;
//                                upRight.Y = mouseState.Y;
                                upRightRay = svc.ComputeRayFromScreenXY(_clickMouseState.X, mouseState.Y);

                                int centX = mouseState.X + (int)(0.5f * (_clickMouseState.X - mouseState.X));
                                int centY = mouseState.Y + (int)(0.5f * (_clickMouseState.Y - mouseState.Y));
                                centerRay = svc.ComputeRayFromScreenXY(centX, centY);
                            }

                            gfx.RayTriangleIntersector isectorUL = new gfx.RayTriangleIntersector(upLeftRay);
                            desk.VisitComponents(isectorUL);
                            upLeft = isectorUL.GetIntersection();

                            gfx.RayTriangleIntersector isectorUR = new gfx.RayTriangleIntersector(upRightRay);
                            desk.VisitComponents(isectorUR);
                            upRight = isectorUR.GetIntersection();

                            gfx.RayTriangleIntersector isectorDL = new gfx.RayTriangleIntersector(downLeftRay);
                            desk.VisitComponents(isectorDL);
                            downLeft = isectorDL.GetIntersection();

                            gfx.RayTriangleIntersector isectorDR = new gfx.RayTriangleIntersector(downRightRay);
                            desk.VisitComponents(isectorDR);
                            downRight = isectorDR.GetIntersection();

                            gfx.RayTriangleIntersector isectorC = new gfx.RayTriangleIntersector(centerRay);
                            desk.VisitComponents(isectorC);
                            center = isectorC.GetIntersection();

                            _selectObjectsBox.SetScreenCoords(0, 0, 0, 0);
                            _selectObjectsBox.SetIsActive(false, true);

                            // Ensures that all points of the box are on the table.
                            if (upLeft != Vector3.Zero && downLeft != Vector3.Zero && upRight != Vector3.Zero && downRight != Vector3.Zero)
                            {
                                if (keyState.IsKeyDown(Keys.LeftShift))
                                {
                                    List<core.GameObject> newlist = new List<core.GameObject>();
                                    gameObjDB.GetGameObjectsIntersectBox(upLeft, upRight, downLeft, downRight, center, _playerTeamGameObjFilter, newlist);

                                    foreach (core.GameObject newUnit in newlist)
                                    {
                                        if (_multiSelectedUnits.Count() < _maxSelectedUnits)
                                        {
                                            if (newUnit.Class != "Commander")
                                            {
                                                if (!_multiSelectedUnits.Contains(newUnit))
                                                {
                                                    _multiSelectedUnits.Add(newUnit);
                                                    _selectedObjectUnit = newSelObjUnitComp;
                                                    _msgObjSelected.SelectedObject = newUnit;

                                                    bool haloSet = false;
                                                    foreach (core.GameObject halo in _multiHaloUnits)
                                                    {
                                                        if (!haloSet)
                                                        {
                                                            gameplay.HaloComponent newHaloUnitComp = null;

                                                            _findHaloComponent.Reset();
                                                            halo.VisitComponents(_findHaloComponent);
                                                            newHaloUnitComp = _findHaloComponent.HaloComp;


                                                            if (newHaloUnitComp.SelectedObject == null)
                                                            {
                                                                halo.Send(_msgObjSelected);
                                                                haloSet = true;
                                                            }
                                                        }
                                                    }

                                                    newUnit.Send(_msgObjSelected);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                                    {
                                        _msgObjSelected.SelectedObject = null;
                                        selectedUnit.Send(_msgObjSelected);
                                    }
                                    foreach (core.GameObject halo in _multiHaloUnits)
                                    {
                                        halo.Send(_msgObjSelected);
                                    }

                                    _multiSelectedUnits.Clear();

                                    List<core.GameObject> newlist = new List<core.GameObject>();
                                    gameObjDB.GetGameObjectsIntersectBox(upLeft, upRight, downLeft, downRight, center, _playerTeamGameObjFilter, newlist);

                                    foreach (core.GameObject newUnit in newlist)
                                    {
                                        if (_multiSelectedUnits.Count() < _maxSelectedUnits)
                                        {
                                            if (newUnit.Class != "Commander")
                                            {
                                                if (!_multiSelectedUnits.Contains(newUnit))
                                                {
                                                    _multiSelectedUnits.Add(newUnit);
                                                    _selectedObjectUnit = newSelObjUnitComp;
                                                    _msgObjSelected.SelectedObject = newUnit;

                                                    bool haloSet = false;
                                                    foreach (core.GameObject halo in _multiHaloUnits)
                                                    {
                                                        if (!haloSet)
                                                        {
                                                            gameplay.HaloComponent newHaloUnitComp = null;

                                                            _findHaloComponent.Reset();
                                                            halo.VisitComponents(_findHaloComponent);
                                                            newHaloUnitComp = _findHaloComponent.HaloComp;


                                                            if (newHaloUnitComp.SelectedObject == null)
                                                            {
                                                                halo.Send(_msgObjSelected);
                                                                haloSet = true;
                                                            }
                                                        }
                                                    }

                                                    newUnit.Send(_msgObjSelected);
                                                }
                                            }
                                        }
                                    }
                                }

                                sound.com.MsgSoundEffectRequest se = new sound.com.MsgSoundEffectRequest();
                                se.Request = new DeskWars.sound.SoundComponent.Request();
                                se.Request.SoundName = "SelectUnit";
                                se.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                                gameObject.PostOffice.Send(se);
                            }

                            /*
                            gfx.RayTriangleIntersector isector = new gfx.RayTriangleIntersector(ray);
                            desk.VisitComponents(isector);
                            if (isector.FoundIntersection())
                            {
                                Vector3 _clickMousePos2 = isector.GetIntersection();

                                BoundingBox selectionBox;

                                if (_clickMousePos.X > _clickMousePos2.X)
                                {
                                    selectionBox.Max.X = _clickMousePos.X;
                                    selectionBox.Min.X = _clickMousePos2.X;
                                }
                                else
                                {
                                    selectionBox.Max.X = _clickMousePos2.X;
                                    selectionBox.Min.X = _clickMousePos.X;
                                }

                                if (_clickMousePos.Z > _clickMousePos2.Z)
                                {
                                    selectionBox.Max.Z = _clickMousePos.Z;
                                    selectionBox.Min.Z = _clickMousePos2.Z;
                                }
                                else
                                {
                                    selectionBox.Max.Z = _clickMousePos2.Z;
                                    selectionBox.Min.Z = _clickMousePos.Z;
                                }

                                selectionBox.Max.Y = 1000;
                                selectionBox.Min.Y = -1000;
                                _selectObjectsBox.SetScreenCoords(0, 0, 0, 0);
                                _selectObjectsBox.SetIsActive(false, true);

                                List<core.GameObject> newlist = new List<core.GameObject>();
                                gameObjDB.GetGameObjectsIntersectBox(selectionBox, _playerTeamGameObjFilter, newlist);

                                if (keyState.IsKeyDown(Keys.LeftShift))
                                {
                                    foreach (core.GameObject newUnit in newlist)
                                    {
                                        if (_multiSelectedUnits.Count() < maxSelectedUnits)
                                        {
                                            if (newUnit.Class != "Commander")
                                            {
                                                if (!_multiSelectedUnits.Contains(newUnit))
                                                {
                                                    _multiSelectedUnits.Add(newUnit);
                                                    _selectedObjectUnit = newSelObjUnitComp;
                                                    _msgObjSelected.SelectedObject = newUnit;

                                                    bool haloSet = false;
                                                    foreach (core.GameObject halo in _multiHaloUnits)
                                                    {
                                                        if (!haloSet)
                                                        {
                                                            gameplay.HaloComponent newHaloUnitComp = null;

                                                            _findHaloComponent.Reset();
                                                            halo.VisitComponents(_findHaloComponent);
                                                            newHaloUnitComp = _findHaloComponent.HaloComp;


                                                            if (newHaloUnitComp.SelectedObject == null)
                                                            {
                                                                halo.Send(_msgObjSelected);
                                                                haloSet = true;
                                                            }
                                                        }
                                                    }

                                                    newUnit.Send(_msgObjSelected);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                                    {
                                        _msgObjSelected.SelectedObject = null;
                                        selectedUnit.Send(_msgObjSelected);
                                    }
                                    foreach (core.GameObject halo in _multiHaloUnits)
                                    {
                                        halo.Send(_msgObjSelected);
                                    }

                                    _multiSelectedUnits.Clear();

                                    foreach (core.GameObject newUnit in newlist)
                                    {
                                        if (_multiSelectedUnits.Count() < maxSelectedUnits)
                                        {
                                            if (newUnit.Class != "Commander")
                                            {
                                                if (!_multiSelectedUnits.Contains(newUnit))
                                                {
                                                    _multiSelectedUnits.Add(newUnit);
                                                    _selectedObjectUnit = newSelObjUnitComp;
                                                    _msgObjSelected.SelectedObject = newUnit;

                                                    bool haloSet = false;
                                                    foreach (core.GameObject halo in _multiHaloUnits)
                                                    {
                                                        if (!haloSet)
                                                        {
                                                            gameplay.HaloComponent newHaloUnitComp = null;

                                                            _findHaloComponent.Reset();
                                                            halo.VisitComponents(_findHaloComponent);
                                                            newHaloUnitComp = _findHaloComponent.HaloComp;


                                                            if (newHaloUnitComp.SelectedObject == null)
                                                            {
                                                                halo.Send(_msgObjSelected);
                                                                haloSet = true;
                                                            }
                                                        }
                                                    }

                                                    newUnit.Send(_msgObjSelected);
                                                }
                                            }
                                        }
                                    }
                                }

                                sound.com.MsgSoundEffectRequest se = new sound.com.MsgSoundEffectRequest();
                                se.Request = new DeskWars.sound.SoundComponent.Request();
                                se.Request.SoundName = "SelectUnit";
                                se.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                                gameObject.PostOffice.Send(se);
                            }
                            */
                        }
                    }
                }
                else if (_multiSelectedUnits.Count != 0)
//                if (_selectedObject != null)
                {
                    if (newSelObject != null)
                    {
                        // Check if selecting a new unit
                        
                        if (!keyState.IsKeyDown(Keys.LeftShift))
                        {
                            foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                            {
                                _msgObjSelected.SelectedObject = null;
                                selectedUnit.Send(_msgObjSelected);
                            }
                            foreach (core.GameObject halo in _multiHaloUnits)
                            {
                                halo.Send(_msgObjSelected);
                            }

                            _multiSelectedUnits.Clear();

                            _multiSelectedUnits.Add(newSelObject);
                            _selectedObjectUnit = newSelObjUnitComp;

                            _msgObjSelected.SelectedObject = newSelObject;
                            _multiHaloUnits.First().Send(_msgObjSelected);
                            newSelObject.Send(_msgObjSelected);

                            sound.com.MsgSoundEffectRequest se = new sound.com.MsgSoundEffectRequest();
                            se.Request = new DeskWars.sound.SoundComponent.Request();
                            se.Request.SoundName = "SelectUnit";
                            se.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                            gameObject.PostOffice.Send(se);
                        } 
                        else if (keyState.IsKeyDown(Keys.LeftShift))
                        {
                            // This is the case where a player would "Add" or "Remove" an additional unit to their selected units.
                            if (_multiSelectedUnits.Contains(newSelObject))
                            {
                                _multiSelectedUnits.Remove(newSelObject);
                                _msgObjSelected.SelectedObject = null;
                                newSelObject.Send(_msgObjSelected);

                                _msgObjSelected.SelectedObject = newSelObject;

                                bool objFound = false;
                                foreach (core.GameObject halo in _multiHaloUnits)
                                {
                                    if (!objFound)
                                    {
                                        gameplay.HaloComponent newHaloUnitComp = null;

                                        _findHaloComponent.Reset();
                                        halo.VisitComponents(_findHaloComponent);
                                        newHaloUnitComp = _findHaloComponent.HaloComp;

                                        if (newHaloUnitComp.SelectedObject == newSelObject)
                                        {
                                            _msgObjSelected.SelectedObject = null;
                                            halo.Send(_msgObjSelected);
                                            objFound = true;
                                        }
                                    }
                                }

                                if (_multiSelectedUnits.Count() != 0)
                                {
                                    _findUnitComponent.Reset();
                                    _multiSelectedUnits.First().VisitComponents(_findUnitComponent);
                                    newSelObjUnitComp = _findUnitComponent.UnitComp;
                                    _msgMouseOver.UnitComponent = newSelObjUnitComp;

                                    _selectedObjectUnit = newSelObjUnitComp;
                                }
                            }
                            else
                            {
                                if (_multiSelectedUnits.Count() < _maxSelectedUnits)
                                {
                                    _multiSelectedUnits.Add(newSelObject);
                                    _selectedObjectUnit = newSelObjUnitComp;
                                    _msgObjSelected.SelectedObject = newSelObject;

                                    bool haloSet = false;
                                    foreach (core.GameObject halo in _multiHaloUnits)
                                    {
                                        if (!haloSet)
                                        {
                                            gameplay.HaloComponent newHaloUnitComp = null;

                                            _findHaloComponent.Reset();
                                            halo.VisitComponents(_findHaloComponent);
                                            newHaloUnitComp = _findHaloComponent.HaloComp;


                                            if (newHaloUnitComp.SelectedObject == null)
                                            {
                                                halo.Send(_msgObjSelected);
                                                haloSet = true;
                                            }
                                        }
                                    }

                                    newSelObject.Send(_msgObjSelected);

                                    sound.com.MsgSoundEffectRequest se = new sound.com.MsgSoundEffectRequest();
                                    se.Request = new DeskWars.sound.SoundComponent.Request();
                                    se.Request.SoundName = "SelectUnit";
                                    se.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                                    gameObject.PostOffice.Send(se);
                                }
                                else
                                {
                                    // play error sound.
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (newSelObject != null)
                    {
                        
                        _multiSelectedUnits.Add(newSelObject);
                        _msgObjSelected.SelectedObject = newSelObject;
                        _multiHaloUnits.First().Send(_msgObjSelected);
                        newSelObject.Send(_msgObjSelected);
                        _selectedObjectUnit = newSelObjUnitComp;

                        sound.com.MsgSoundEffectRequest se = new sound.com.MsgSoundEffectRequest();
                        se.Request = new DeskWars.sound.SoundComponent.Request();
                        se.Request.SoundName = "SelectUnit";
                        se.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                        gameObject.PostOffice.Send(se);
                    }
                }
            }

            if (_prevMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
            {
                if(_multiSelectedUnits.Count != 0)
                {
                    if (unit != null)
                    {
                        // Check if attacking an enemy
                        com.MsgChase msg = new com.MsgChase();
                        msg.AttackObject = unit;

                        foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                        {
                            selectedUnit.Send(msg);
                        }

                        //only play the class specific attack sound if we haven't played it
                        //for 1 second or the last attacker was someone other than the
                        //current attacker
                        bool playSound = false;
                        if (_updateMouseIconMsg.Texture == _attackMouseFail)
                        {
                            _soundEffectRequest.Request.SoundName = "AttackFail";
                            playSound = true;
                        }
                        else
                        {
                            //only play the attack sound if the attacker has changed or the
                            //attacked object has changed
                            if (_lastAttacker != _multiSelectedUnits.First() 
                                || _lastAttackedObject != unit)
                            {
                                playSound = true;
                            }

                            _soundEffectRequest.Request.SoundName = _multiSelectedUnits.First().Class + "Attack" + _attackSound;
                            if (_attackSound == 1)
                                _attackSound = 2;
                            else
                                _attackSound = 1;
                        }
                        _lastAttacker = _multiSelectedUnits.First();
                        _lastAttackedObject = unit;
                        if (playSound)
                        {
                            _soundEffectRequest.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                            gameObject.PostOffice.Send(_soundEffectRequest);
                        }

                    }
                    else if (newSelObject != null)
                    {
                        // Check if healing a friendly unit

                        if (_multiSelectedUnits.Count != 0)
                        {
                            // Don't allow healer to heal itself?
                        }
                    }
                    else if (desk != null)
                    {
                        // Check if moving a unit
                        gfx.RayTriangleIntersector isector = new gfx.RayTriangleIntersector(ray);
                        desk.VisitComponents(isector);
                        if (isector.FoundIntersection())
                        {
                            com.MsgMoveTo msg = new com.MsgMoveTo();
                            msg.GoTo = isector.GetIntersection();

                            foreach (core.GameObject selectedUnit in _multiSelectedUnits)
                            {
                                selectedUnit.Send(msg);
                            }

                            _soundEffectRequest.Request.SoundName = "MoveUnit";
                            _soundEffectRequest.Request.RequestedAction = DeskWars.sound.SoundComponent.Request.Action.START;
                            gameObject.PostOffice.Send(_soundEffectRequest);
                            //core.DebugService debugSvc = (core.DebugService)game.Services.GetService(typeof(core.DebugService));
                            //debugSvc.AddDebugText("Ground Intersection: " + msg.GoTo.ToString(),
                            //                      5.0f);
                        }
                    }
                }
            }

            if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                Vector3 position = gameObject.Position + -(gameObject.RightVector * ((_prevMouseState.X - mouseState.X) * _mouseRotateScalar)) +
                                       -(gameObject.UpVector * ((_prevMouseState.Y - mouseState.Y) * _mouseRotateScalar));

                if (position.Y >= 10.0f)
                    gameObject.SetPositionAndLookAt(position, gameObject.LookAt);
            }

            if (mouseState.ScrollWheelValue != prevScrollWheelValue)
            {
                int diff = mouseState.ScrollWheelValue - prevScrollWheelValue;
                prevScrollWheelValue = mouseState.ScrollWheelValue;
                Vector3 moveVector = (gameObject.LookVector * diff);
                Vector3 nextPos = gameObject.Position + moveVector;

                if (cameraBounds.Contains(nextPos) == ContainmentType.Contains && nextPos.Y >= 10.0f)
                {
                    // Zoom around selected object
                    Vector3 lookAt = gameObject.LookAt + moveVector;

                    gameObject.SetPositionAndLookAt(nextPos, lookAt);
                }
            }

            // Implement camera bounding box

            _prevMouseState = mouseState;
        }

        public override void Update(core.GameObject gameObject,
                                    GameTime gameTime)
        {
            //this message is sent by the minimap
            if (_updateCameraMsg != null)
            {
                Vector3 diff = _updateCameraMsg.LookAt - gameObject.LookAt;
                gameObject.Position += diff;
                gameObject.LookAt = _updateCameraMsg.LookAt;
                _updateCameraMsg = null;
                return;
            }

            core.Game.KeyboardMouseInput input = core.Game.instance().Input;

            if (!input.KeyboardInputHandled)
            {
                HandleKeyboardInput(input, gameObject);
            }

            if (!input.MouseInputHandled)
            {
                HandleMouseInput(input, gameObject, gameTime);
            }
            else
            {
                if (_updateMouseIconMsg.Texture != _defaultMouse)
                {
                    _updateMouseIconMsg.Texture = _defaultMouse;
                    core.Game.instance().PostOffice.Send(_updateMouseIconMsg);
                }
            }
        }

        #region MessageHandler Members

        public void receive(DeskWars.com.Message msg)
        {
            _updateCameraMsg = (com.MsgUpdateCamera)msg;
        }

        #endregion



        public override void OnAddToGameObject(DeskWars.core.GameObject gameObject)
        {
            core.GameObjectFactory factory =
                            (core.GameObjectFactory)core.Game.instance().Services.GetService(typeof(core.GameObjectFactory));

            for (int i = 0; i < _maxSelectedUnits; i++)
            {
                string name = "SelectedObject " + i.ToString();
                core.GameObject halo = factory.CreateGameObject("SelectedObject", name);
                _multiHaloUnits.Add(halo);
            }

            _mouseHoverObject = factory.CreateGameObject("MouseHoverObject", "MouseHoverObject");
            core.GameObject selectObjectsGameObj = factory.CreateGameObject("SelectObjectsBox", "SelectObjectsBox");
            _selectObjectsBox = (gui.TextureWidget)selectObjectsGameObj.Components[0];
            _selectObjectsBox.SetScreenCoords(0, 0, 0, 0);
            _selectObjectsBox.SetIsActive(false, true);
            _selectObjectsBox.ReceiveEvents = false;

            core.Game.instance().PostOffice.RegisterForMessages(typeof(com.MsgUpdateCamera), this);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            foreach (core.GameObject halo in _multiHaloUnits)
            {
                core.Game.instance().DeleteGameObject(halo);
            }
            core.Game.instance().PostOffice.UnRegisterForMessages(typeof(com.MsgUpdateCamera), this);
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
