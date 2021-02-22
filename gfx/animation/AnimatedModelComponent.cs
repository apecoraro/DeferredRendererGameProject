/*
 * ModelAnimator.cs
 * Copyright (c) 2007 David Astle, Michael Nikonov
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#define GI
#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using DeskWars.gfx;
using Xclna.Xna.Animation;
#endregion

namespace DeskWars.gfx.Animation
{

    /// <summary>
    /// Animates and draws a model that was processed with AnimatedModelProcessor
    /// </summary>
    public class AnimatedModelComponent : DeskWars.gfx.ModelComponent
    {
        #region Member Variables
        class MessageHandler : DeskWars.com.MessageHandler
        {
            AnimatedModelComponent _comp;
            public MessageHandler(AnimatedModelComponent comp) { _comp = comp; }

            #region MessageHandler Members

            void DeskWars.com.MessageHandler.receive(DeskWars.com.Message msg)
            {
                _comp.OnAnimateMessage(msg as gfx.com.MsgAnimate);
            }

            #endregion
        }

        MessageHandler _messageHandler = null;
        // Stores the world transform for the animation controller.
        //private Matrix _world = Matrix.Identity;

        // Model to be animated
        //private readonly Model _model;

        // This stores all of the "World" matrix parameters for an unskinned model
        private readonly EffectParameter[] _worldParams, _matrixPaletteParams;

        // A flattened array of effects, one for each ModelMeshPart
        private Effect[] _modelEffects;
        private ReadOnlyCollection<Effect> _effectCollection;

        // Skeletal structure containg transforms
        private BonePoseCollection _bonePoses;

        private AnimationInfoCollection _animations;
        private List<String> _animationNames = new List<String>();

        public int GetNumAnimations()
        {
            return _animationNames.Count;
        }

        public String GetAnimationName(int index)
        {
            return _animationNames[index];
        }

        // List of attached objects
        private IList<IAttachable> _attachedObjects = new List<IAttachable>();

        // Store the number of meshes in the model
        private readonly int _numMeshes;
        
        // Stores the number of effects/ModelMeshParts
        private readonly int _numEffects;

        // Used to avoid reallocation
        private static Matrix _skinTransform;
        // Buffer for storing absolute bone transforms
        private Matrix[] _pose;
        // Array used for the matrix palette
        private Matrix[][] _palette;
        // Inverse reference pose transforms
        private SkinInfoCollection[] _skinInfo;

        private Dictionary<string, AnimationController> _animationControllers;
        AnimationController _activeController = null;

        #endregion

        #region General Properties

        /// <summary>
        /// Gets or sets the world matrix for the animation scene.
        /// </summary>
        //public Matrix World
        //{
        //    get
        //    {
        //        return _world;
        //    }
        //    set
        //    {
        //        _world = value;
        //    }
        //}

        /// <summary>
        /// Returns the number of effects used by the model, one for each ModelMeshPart
        /// </summary>
        protected int EffectCount
        {
            get { return _numEffects; }
        }

        /// <summary>
        /// Gets the model associated with this controller.
        /// </summary>
        //public Model Model
        //{ get { return _model; } }

        /// <summary>
        /// Gets the animations that were loaded in from the content pipeline
        /// for this model.
        /// </summary>
        public AnimationInfoCollection Animations
        { get { return _animations; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of ModelAnimator.
        /// </summary>
        /// <param name="game">The game to which this component will belong.</param>
        /// <param name="model">The model to be animated.</param>
        public AnimatedModelComponent(Model model)
        {
            _messageHandler = new MessageHandler(this);

            this.Model = model;

            _animations = AnimationInfoCollection.FromModel(model);
            IEnumerator<KeyValuePair<string, AnimationInfo>> itr = _animations.GetEnumerator();
            _animationControllers = new Dictionary<string, AnimationController>(_animations.Count);
            while (itr.MoveNext())
            {
                if(itr.Current.Value != null)
                {
                    AnimationController animCtrl = new AnimationController(itr.Current.Value);

                    _animationControllers.Add(itr.Current.Key, animCtrl);

                    _animationNames.Add(itr.Current.Key);
                }
            }

            _bonePoses = BonePoseCollection.FromModelBoneCollection(
                model.Bones);

            //if(_animationControllers.ContainsKey("idle"))
            //    SetAnimation(_animationControllers["idle"]);

            _numMeshes = model.Meshes.Count;
            // Find total number of effects used by the model
            _numEffects = 0;
            foreach (ModelMesh mesh in model.Meshes)
                foreach (Effect effect in mesh.Effects)
                    _numEffects++;

            // Initialize the arrays that store effect parameters
            _modelEffects = new Effect[_numEffects];
            _worldParams = new EffectParameter[_numEffects];
            _matrixPaletteParams = new EffectParameter[_numEffects];
            InitializeEffectParams();

            _pose = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(_pose);
            // Get all the skinning info for the model
            Dictionary<string, object> modelTagInfo = (Dictionary<string, object>)model.Tag;
            if (modelTagInfo == null)
                throw new Exception("Model Processor must subclass AnimatedModelProcessor.");
            _skinInfo = (SkinInfoCollection[])modelTagInfo["SkinInfo"];
            if (_skinInfo == null)
                throw new Exception("Model processor must pass skinning info through the tag.");

            _palette = new Matrix[model.Meshes.Count][];
            for (int i = 0; i < _skinInfo.Length; i++)
            {
                if (Util.IsSkinned(model.Meshes[i]))
                    _palette[i] = new Matrix[_skinInfo[i].Count];
                else
                    _palette[i] = null;
            }
   
            // Test to see if model has too many bones
            for (int i = 0; i < model.Meshes.Count; i++ )
            {
                if (_palette[i] != null && _matrixPaletteParams[i] != null)
                {
                    Matrix[] meshPalette = _palette[i];
                    try
                    {
                        _matrixPaletteParams[i].SetValue(meshPalette);
                    }
                    catch
                    {
                        throw new Exception("Model has too many skinned bones for the matrix palette.");
                    }
                }
            }
        }

        public void OnAnimateMessage(gfx.com.MsgAnimate msg)
        {
            StartAnimation(msg.Animation, msg.SpeedFactor, msg.Loop);
        }

        public void StopAnimation()
        {
            SetAnimation(null);
        }

        public void StartAnimation(string animation, float speedFactor, bool animLoop)
        {
            if (_animationControllers.ContainsKey(animation))
            {
                AnimationController controller = _animationControllers[animation];
                controller.SpeedFactor = speedFactor;
                controller.IsLooping = animLoop;
                
                SetAnimation(controller);
            }
            else
                SetAnimation(null);
        }

        public override void OnAddToGameObject(core.GameObject gameObject)
        {
            gameObject.PostOffice.RegisterForMessages(typeof(gfx.com.MsgAnimate), _messageHandler);
            base.OnAddToGameObject(gameObject);
        }

        public override void OnRemoveFromGameObject(DeskWars.core.GameObject gameObject)
        {
            gameObject.PostOffice.UnRegisterForMessages(typeof(gfx.com.MsgAnimate), _messageHandler);
            base.OnRemoveFromGameObject(gameObject);
        }
        #endregion
        /// <summary>
        /// Returns skinning information for a mesh.
        /// </summary>
        /// <param name="index">The index of the mesh.</param>
        /// <returns>Skinning information for the mesh.</returns>
        public SkinInfoCollection GetMeshSkinInfo(int index)
        {
            return _skinInfo[index];
        }

        /// <summary>
        /// Called during creation and calls to InitializeEffectParams.  Returns the list of
        /// effects used during rendering.
        /// </summary>
        /// <returns>A flattened list of effects used during rendering, one for each ModelMeshPart</returns>
        protected virtual IList<Effect> CreateEffectList()
        {
            List<Effect> effects = new List<Effect>();
            foreach (ModelMesh mesh in this.Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    effects.Add(part.Effect);
                }
            }
            return effects;
        }

        public override void InitializeDrawables()
        {
            InitializeEffectParams();

            this.Drawables = new Drawable[this.Model.Meshes.Count];
            for(int meshIndex = 0; meshIndex < this.Model.Meshes.Count; ++meshIndex)
            {
                ModelMesh mesh = this.Model.Meshes[meshIndex];
                this.Drawables[meshIndex] = new Drawable(this);
                this.Drawables[meshIndex].IndexBuffer = mesh.IndexBuffer;
                this.Drawables[meshIndex].VertexBuffer = mesh.VertexBuffer;
                this.Drawables[meshIndex].DrawableParts = new Drawable.DrawablePart[mesh.MeshParts.Count];
                for (int i = 0; i < mesh.MeshParts.Count; ++i)
                {
                    ModelMeshPart part = mesh.MeshParts[i];
                    this.Drawables[meshIndex].DrawableParts[i] = new Drawable.DrawablePart(this.Drawables[meshIndex]);
                    Drawable.DrawablePart drawPart = this.Drawables[meshIndex].DrawableParts[i];
                    drawPart.BaseVertex = part.BaseVertex;
                    drawPart.Effect = (ShadowMapEffect)part.Effect;
                    drawPart.MinVertexIndex = 0;
                    drawPart.NumVertices = part.NumVertices;
                    drawPart.PrimitiveCount = part.PrimitiveCount;
                    drawPart.PrimitiveType = PrimitiveType.TriangleList;
                    drawPart.StartIndex = part.StartIndex;
                    drawPart.StreamOffset = part.StreamOffset;
                    drawPart.VertexDeclaration = part.VertexDeclaration;
                    drawPart.VertexStride = part.VertexStride;
                    int effectIndex = 0;
                    for (; effectIndex < _modelEffects.Length; ++effectIndex)
                    {
                        if (part.Effect == _modelEffects[effectIndex])
                            break;
                    }
                    drawPart.ComponentData = new DrawableComponentData(meshIndex, effectIndex);
                }
            }
        }

        /// <summary>
        /// Initializes the effect parameters.  Should be called after the effects
        /// on the model are changed.
        /// </summary>
        protected void InitializeEffectParams()
        {
            IList<Effect> effects = CreateEffectList();
            if (effects.Count != _numEffects)
                throw new Exception("The number of effects in the list returned by CreateEffectList "
                    + "must be equal to the number of ModelMeshParts.");
            effects.CopyTo(_modelEffects, 0);
            _effectCollection = new ReadOnlyCollection<Effect>(_modelEffects);
            // store the parameters in the arrays so the values they refer to can quickly be set
            for (int i = 0; i < _numEffects; i++)
            {
                _worldParams[i] = _modelEffects[i].Parameters["World"];
                _matrixPaletteParams[i] = _modelEffects[i].Parameters["MatrixPalette"];
            }
        }

        /// <summary>
        /// Gets a collection of effects, one per ModelMeshPart, that are used by 
        /// the ModelAnimator. The first index of the collection corresponds to the
        /// effect used to draw the first ModelMeshPart of the first Mesh, and the 
        /// last index corresponds to the effect used to drwa the last ModelMeshPart
        /// of the last Mesh.
        /// </summary>
        public ReadOnlyCollection<Effect> Effects
        {
            get { return _effectCollection; }
        }

        #region Animation and Update Routines

        public override void Update(DeskWars.core.GameObject gameObject, GameTime gameTime)
        {
            if (_activeController != null)
                _activeController.Update(gameTime);

            base.Update(gameObject, gameTime);
        }

        private void SetAnimation(AnimationController controller)
        {
            _activeController = controller;
            foreach (BonePose p in this.BonePoses)
            {
                if (p.CurrentController != controller)
                {
                    if(controller != null)
                        controller.ElapsedTime = 0;
                    p.CurrentController = controller;
                    p.CurrentBlendController = null;
                }
            }
        }

        /// <summary>
        /// Updates the animator by finding the current absolute transforms.
        /// </summary>
        /// <param name="gameTime">The GameTime.</param>
        public void UpdateAnimation(Matrix worldMat, GameTime gameTime)
        {
            _bonePoses.CopyAbsoluteTransformsTo(_pose);
            for (int i = 0; i < _skinInfo.Length; i ++) 
            {
                if (_palette[i] == null)
                    continue;
                SkinInfoCollection infoCollection = _skinInfo[i];
                foreach (SkinInfo info in infoCollection)
                {
                    _skinTransform = info.InverseBindPoseTransform;
                    Matrix.Multiply(ref _skinTransform, ref _pose[info.BoneIndex],
                       out _palette[i][info.PaletteIndex]);
                }
            }

            foreach (IAttachable attached in _attachedObjects)
            {
                attached.CombinedTransform = attached.LocalTransform *
                    Matrix.Invert(_pose[this.Model.Meshes[0].ParentBone.Index]) *
                    _pose[attached.AttachedBone.Index] * worldMat;
            }
        }


        /// <summary>
        /// Copies the current absolute transforms to the specified array.
        /// </summary>
        /// <param name="transforms">The array to which the transforms will be copied.</param>
        public void CopyAbsoluteTransformsTo(Matrix[] transforms)
        {
            _pose.CopyTo(transforms, 0);
        }

        /// <summary>
        /// Gets the current absolute transform for the given bone index.
        /// </summary>
        /// <param name="boneIndex"></param>
        /// <returns>The current absolute transform for the bone index.</returns>
        public Matrix GetAbsoluteTransform(int boneIndex)
        {
            return _pose[boneIndex];
        }


        /// <summary>
        /// Gets a list of objects that are attached to a bone in the model.
        /// </summary>
        public IList<IAttachable> AttachedObjects
        {
            get { return _attachedObjects; }
        }

        /// <summary>
        /// Gets the BonePoses associated with this ModelAnimator.
        /// </summary>
        public BonePoseCollection BonePoses
        {
            get { return _bonePoses; }
        }

        class DrawableComponentData : ComponentData
        {
            public DrawableComponentData(int meshIndex, int effectIndex)
            {
                MeshIndex = meshIndex;
                EffectIndex = effectIndex;
            }

            public int MeshIndex;
            public int EffectIndex;
        }
        /// <summary>
        /// Draws the current frame
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void PreDraw(Matrix worldMat,
                                     GameTime gameTime)
        {
            UpdateAnimation(worldMat, gameTime);
            SetDrawablesWorldTransform(worldMat);
        }

        public override void ConfigureEffect(ShadowMapEffect baseEffect,
                                             Matrix worldMat,
                                             ComponentData data)
        {
            DrawableComponentData compData = (DrawableComponentData)data;
            int effectIndex = compData.EffectIndex;
            // Update all the effects with the palette and world and draw the meshes
            int meshIndex = compData.MeshIndex;

            ModelMesh mesh = this.Model.Meshes[meshIndex];
                // The starting index for the modelEffects array
            if (_matrixPaletteParams[effectIndex] != null)
            {
                _worldParams[effectIndex].SetValue(worldMat);
                if(_palette[meshIndex] != null)
                    _matrixPaletteParams[effectIndex].SetValue(_palette[meshIndex]);
            }
            else
            {
                _worldParams[effectIndex].SetValue(_pose[mesh.ParentBone.Index] * worldMat);
            }
        }
        #endregion
        //Matrix world = this.Model.Root.Transform * this.ScaleTransform * worldMat;
        //    int index = 0;
        //    // Update all the effects with the palette and world and draw the meshes
        //    for (int i = 0; i < _numMeshes; i++)
        //    {
        //        ModelMesh mesh = this.Model.Meshes[i];
        //        // The starting index for the modelEffects array
        //        int effectStartIndex = index;
        //        if (_matrixPaletteParams[index] != null)
        //        {
        //            //foreach (ShadowMapEffect effect in mesh.Effects)
        //            for(int mIndex = 0; mIndex < mesh.Effects.Count; ++mIndex)
        //            {
        //                ShadowMapEffect effect = (ShadowMapEffect)mesh.Effects[mIndex];

        //                if (baseEffect.UseDefaultTechnique())
        //                    effect.EnableDefaultTechnique();
        //                else
        //                    effect.EnableShadowMapTechnique();

        //                effect.View = viewMat;
        //                effect.Projection = projMat;
        //                //this.State.Apply(effect);
        //                effect.DiffuseColor *= this.State.DiffuseColor;
        //                effect.Alpha *= this.State.Alpha;
        //                effect.WorldViewProjection = world * viewMat * projMat;
        //                //effect.World = worldMat;
        //                _worldParams[index].SetValue(worldMat);
        //                if(_palette[i] != null)
        //                    _matrixPaletteParams[index].SetValue(_palette[i]);
        //                index++;
        //            }
        //        }
        //        else
        //        {
        //            foreach (ShadowMapEffect effect in mesh.Effects)
        //            {
        //                effect.CurrentTechnique = baseEffect.CurrentTechnique;
        //                _worldParams[index].SetValue(_pose[mesh.ParentBone.Index] * worldMat);
        //                index++;
        //            }
        //        }
        //        int numParts = mesh.MeshParts.Count;
        //        GraphicsDevice device = mesh.VertexBuffer.GraphicsDevice;
        //        device.Indices = mesh.IndexBuffer;
        //        for (int j = 0; j < numParts; j++ )
        //        {
        //            ModelMeshPart currentPart = mesh.MeshParts[j];
        //            if (currentPart.NumVertices == 0 || currentPart.PrimitiveCount == 0)
        //                continue;
        //            Effect currentEffect = _modelEffects[effectStartIndex+j];

        //            device.VertexDeclaration = currentPart.VertexDeclaration;
        //            device.Vertices[0].SetSource(mesh.VertexBuffer, currentPart.StreamOffset,
        //                currentPart.VertexStride);

        //            currentEffect.Begin();
        //            EffectPassCollection passes = currentEffect.CurrentTechnique.Passes;
        //            int numPasses = passes.Count;
        //            for (int k = 0; k < numPasses; k++)
        //            {
        //                EffectPass pass = passes[k];
        //                pass.Begin();
        //                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, currentPart.BaseVertex,
        //                    0, currentPart.NumVertices, currentPart.StartIndex, currentPart.PrimitiveCount);
        //                pass.End();
        //            }

        //            currentEffect.End();
        //        }
        //    }
    }
}
