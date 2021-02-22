# DeferredRendererGameProject
This is the code that I wrote for 3 person game project for my Digipen Masters program. This code doesn't compile or run anymore, I just have it as an example of my work (albeit from 2010).

The game was called DeskWars. The idea was that you had to protect your cell phone from evil gremlins using animated desk objects, such as pencils, push pins, staplers, etc. It featured a deferred renderer supporting multiple shadow casting lights, which we used to give the game a dark feel, where the only light came from the head lamps on the push pin foot soldiers, the glow of the cell phone, and solitary desk lamp.

My responsibilities on the team were technical director, core game engine, graphics, GUI, gameplay, and the sound engine.

I designed and implemented the core game engine framework. I also designed and 
implemented the graphics engine and all of the graphics components, the gui
engine and all of the gui components, the sound engine and the sound components,
and the gameplay engine. 

I was fairly happy with the design and implementation 
of my game engine framework. The design made it possible for us to turn on and
off the different components such as AI, Sound, etc in order to test
or profile the code for bottlenecks.  The messaging system prevented any single
sub-system from having to be dependent on a separate sub-system, which helped
prevent any hard to track bugs. Features included:

1. Data driven configuration system
2. Game world query functionality
3. Interface for services provided by sub-systems
4. Messaging system for triggering other sub-systems or components
5. Game Object pools for things like weapons that are created and deleted 
many times over (so you just create a big pool of them at the start of the 
level and then reuse them repeatedly - more efficient than allocating memory in the middle of a level)

I also enjoyed getting the chance to bone up on my graphics programming skills
especially working on the vertex and pixel shaders for doing hardware skeletal
animation, soft shadow maps, and a deferred lighting shader all of which were
essential for the look and feel of our game. Features included:

1. Dynamic lighting and soft shadows - multiple lights (30+) and multiple shadow map lights (6)
2. Hardware skeletal animation
3. Dynamically configurable particle systems
4. Pre-loading of art assets and effects and sharing between game objects (prevent duplicate data)

As far as the GUI, I can't say I realy enjoyed working on the gui code, but once we finally 
got the gui art from the artists I was glad that I had done a good job designing it because 
my design made it easy to switch out my programmer art with nice looking art and move some gui 
widgets around without needing to write any new code. All that was required was modification 
of the gui configuration xml file. Feature include

1. GUI layout driven by xml config file
2. Specifying event handlers for GUI widgets also driven by xml configuration file
3. Supports multiple layers of gui widgets and alpha blending of transparent gui widgets

The sound engine and it's sound components were fairly simple, I think I spent a couple of hours implementing them, but they 
added almost as much to the game as the graphics engine. So it was definitely worth it.

The main thing I did in gameplay logic was implement the gameplay engine, which
controlled loading the game objects and game object components that were needed
for each level (via an xml configuration file). I also implemented the 
triggering and control of the particle system based "win" and "lose" 
scenes that happen at the end of levels.

# Code Descriptions

gfx/animation/AnimatedModelComponent.cs - draws animated models
gfx/animation/AnimationController.cs - controls animation of animated models
gfx/BaseEffect.cs - interface for configuring the shaders
gfx/BaseEffectConfig.cs - configures lighting parameters on shader effects
gfx/BoundingBoxLines.cs - debug component for visualizing bounding boxes on game objects
gfx/BoxComponent.cs - draws a box of configurable size (used this for units before we got unit models)
gfx/CameraComponent.cs - the camera
gfx/Component.cs - base graphics component for drawing things on the screen
gfx/ComponentState.cs - not used after switch to deferred shader
gfx/ComponentTypeFilter.cs - used to search the gameobjectdb for all game objects that contain a gfx component
gfx/DebugService.cs - interface that allows other systems to print messages to the screen for debugging
gfx/DeferredShaderEffect.cs - the interface to the deferred shader program
gfx/DirectionalLightComponent.cs - configures the shader to do a directional light
gfx/Drawable.cs - a component that contain drawable data
gfx/DrawableStateSetter.cs - a component that custom configures the shader to draw a specific Drawable component
gfx/Engine.cs - the main driver of the graphics
gfx/FramesPerSecondText.cs - prints the frames per second on the screen
gfx/GfxAssetManager.cs - manages graphical assets such as models, textures, effects, etc
gfx/GridLines.cs - debug object for visualizing the ai grid
gfx/HaloLightComponent.cs - creates the glow effect on the halo that appears around selected objects
gfx/LightComponent.cs - base component for lights
gfx/LinesComponent.cs - debug component for drawing lines 
gfx/LookLines.cs - utilizes LinesComponent to draw the lines that represent where a game object is looking/facing for debugging use
gfx/ModelComponent.cs - draws non-animated models
gfx/PickService.cs - used to compute a 3d ray from the mouse coordinates in order to query for intersections of rays with the desk and units
gfx/PlaneComponent.cs - debug shape for drawing a plane (used this for the desk before we got a desk model)
gfx/PointLightComponent.cs - configures defferred shader effect to draw a point light
gfx/ps/Billboard.cs - this is the particle system that draws the health bar that pops up over the moused over unit or gremlin
gfx/ps/BillboardComponent.cs - this class controls enabling and disabling of the Billboard.cs
gfx/ps/CircleParticleSystem.cs - draws a circular particle system (didn't end up using this though)
gfx/ps/CircleParticleSystemComponent.cs - controls the circular particle system
gfx/ps/FireworksPS.cs - draws a fireworks particle system (wanted to use this at the end of the game, but ran out of time)
gfx/ps/ImageParticleSystemComponent.cs - controls the display of the "You Win" and "You Lose" screen
gfx/ps/ImagePS.cs - controls the particles in the "You Win" and "You Lose" screen
gfx/ps/ParticleSystemManager.cs - manages, draws, etc all particle systems
gfx/ps/PointSpriteParticleSystem.cs - used to draw the explosion and damage effects
gfx/ps/PointSpriteParticleSystemComponent.cs - controls the enabling and disabling of the PointSpriteParticleSystem.cs
gfx/Quad.cs - geometry for screen sized quad (necessary for deferred shader)
gfx/QuadRenderer.cs - draws the screen sized quad
gfx/RayTriangleIntersector.cs - computes intersections given a ray and set of triangles
gfx/RenderBin.cs - sorts Drawables by Effect and draws them in efficient manner
gfx/ShadowCaster.cs - used to indicate that a game object should cast a shadow
gfx/ShadowMapEffect.cs - interface into shadow map shader that was used prior to switching to the deferred shader
gfx/ShadowMapLightComponent.cs - old shadow map light component for the pre-deferred shader engine
gfx/SpotLightComponent.cs - new shadow map light component and spot light (shadow map is optional in this component)
gfx/Text2DComponent.cs - used by DebugComponent to draw text on the screen
gfx/Triangle.cs - used by RayTriangleIntersector for computing intersections
gfx/TriangleIterator.cs - used by RayTriangleIntersector for computing intersections
gfx/com/MsgAnimate.cs - message used to trigger the playing of an animation
gfx/com/MsgBillboard.cs - messaged used to display of the popup health bar
gfx/com/MsgEffect.cs - message used to trigger a particle effect
gfx/com/MsgChangeState.cs - not used after switch to deferred shader
gfx/com/MsgDraw.cs - not used after implementation of particle systems

sound/com/MsgSoundEffectRequest.cs - used to trigger the playing of a sound
sound/Engine.cs - creates and loads sound components and sound effects
sound/SoundComponent.cs - plays sounds associated with 3d game objects
sound/WidgetSoundEffectComponent.cs  - plays sounds for 2d gui objects (clicking sound, etc).

core/Camera.cs - abstract class for providing camera services to sub-systems 
core/Component.cs - base component class (game objects are made up of these)
core/ComponentConfig.cs - holds the parameters specified in the GameObjectConfig.xml, used to create custom configured components
core/ComponentExactMultiTypeFilter.cs - used with the GameObjectDB to find game objects that contain multiple different component types
core/ComponentExactTypeFilter.cs - used with the GameObjectDB to find game objects that contain a specific component type
core/ComponentTypeFilter.cs - abstract base class used with the GameObjectDB to create custom querys for game objects
core/ComponentVisitor.cs - abstract base class used to perform custom operations on the components of a game object
core/DebugService.cs - abstract class that allows sub-systems to print messages to screen (implemented in the gfx engine)
core/Engine.cs - base class that defines an interface that sub-systems can implement to plugin to the Game's Initialize(), LoadLevel(), Update(), and Draw() calls
core/Game.cs - this is where the main Initialize(), LoadLevel(), Update(), and Draw() calls are implemented
core/GameObject.cs - a collection of components (each component provides custom functionality)
core/GameObjectConfig.cs - contains ComponentConfig instances - used for data driven creation of game objects from custom components
core/GameObjectDB.cs - contains all the active game objects in the game, provides query functionality 
core/GameObjectFactory.cs - service that sub-systems can use to dynamically create a game object
core/GameObjectTypeFilter.cs - used with the GameObjectDB to do queries
core/GraphicsSettingsManager.cs - controls cofiguring of the graphics settings
core/PickService.cs - service interface for doing picking
core/Renderer.cs - abstract class for drawing to the screen (implemented by gfx/Engine.cs and gui/Engine.cs)
core/Timer.cs - used for doing timing of frames (performance timing)

gameplay/AutoRotateComponent.cs - rotates the web camera/lights in the game
gameplay/CommanderComponent.cs - handles logic specific to Commander Good
gameplay/CommanderHealthBar.cs - handles display and update of the commander's health bar gui widget (red battery looking thing on the right side of screen)
gameplay/CustomBoundingBox.cs - allows us to specify a custom bounding box for a game object
gameplay/DeskComponent.cs - component used for identifying the desk game object
gameplay/Engine.cs - controls loading levels and determining win vs loss of levels
gameplay/FullscreenMenuItemSelection.cs - displays and configures the fullscreen yes/no options menu item
gameplay/HaloComponent.cs - handles activating and deactivating the selection halo (circle that appears around the units that you have selected)
gameplay/HelpButtonMessageHandler.cs - handles interaction with the help button
gameplay/MainMenuButtonMessageHandler.cs - handles interaction with the main menu button
gameplay/MiniMapWidget.cs - handles interaction with the mini-map
gameplay/MouseHoverComponent.cs - handles triggering of the billboard component to show the health bar for each unit
gameplay/QuitButtonMessageHandler.cs - handles interaction with the quit button
gameplay/RepeatLevelClicker.cs - used in conjuction with the lose screen to cause the level to be repeated when the user clicks on the screen
gameplay/ResolutionMenuItemSelection.cs - handles display and configuration of the resolution option menu item
gameplay/ResumeButtonMessageHandler.cs - handles interaction with the resume button
gameplay/SelectedUnitWidget.cs - handles displaying of the selected units stats on the gui
gameplay/SettingsButtonEventHandlers.cs - handles interaction with the options menu 
gameplay/SettingsButtonMessageHandler.cs - handles interaction with the options button (which shows the options menu)
gameplay/StartGameClicker.cs - used in multiple points in the game to advance to the next level when the user clicks
gameplay/StartGameGUI.cs - handles interaction with the start game gui
gameplay/SuperGremlinComponent.cs - didn't end up using this, but it is what controls playing of the gremlin scream sound

gui/Engine.cs - controls display and handling of mouse and keyboard events for the gui
gui/MenuItemSelection.cs - used in the options menu for doing selection of options
gui/MouseIcon.cs - used to draw the mouse icon
gui/MsgMouseEvent.cs - used to trigger the mouse icon to change
gui/TextureWidget.cs - used to draw gui widgets that have textures
gui/TextureWidgetColorSwitcher.cs - switches the color on TextureWidgets when you mouse over, out, click, etc on them
gui/TextWidget.cs - used to draw text for the gui
gui/Widget.cs - base class of gui widgets
gui/WidgetDepthAndEventToggler.cs - used to toggle the gui widgets from the top layer to lower layers in the gui tree
gui/WidgetMessageHandler.cs - base class that sub-systems can use to implement gui event interaction handlers

--- All the messages below trigger some functionality that is implemented in the main code components ---
com/Message.cs
com/MessageHandler.cs
com/MsgCommanderAttacked.cs
com/MsgGameSettings.cs
com/MsgGameState.cs
com/MsgLoadLevel.cs
com/MsgLook.cs
com/MsgMouseOver.cs
com/MsgMove.cs
com/MsgObjectSelected.cs
com/MsgUnitSelected.cs
com/MsgUpdateCamera.cs
com/PostOffice.cs
