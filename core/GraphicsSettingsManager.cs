#region File Description
/*****************************************************************************
 *
 * 5th Gear FIRST Robotics Simulation
 * Copyright (c) 2008 Lockheed Martin Corporation.  All Rights Reserved.
 *
 *****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    /// <summary>
    /// This class is used for managing the video settings
    /// </summary>
    /// <remarks>
    /// GraphicsSettingsManager uses a GraphicsSettingsConfigFile to set graphics settings
    ///  - Fullscreen - yes/no
    ///  - Backbuffer width & height - integer value greater than zero
    ///  - Synchronize draw with vertical retrace = yes/no
    ///  - Use Antialiasing & number of samples - yes/no and samples value between 1 and 16
    ///  - Gamma Ramp Offset - use to modify the rgb levels (brightness) of the screen, only has
    ///     an effect if in full screen mode
    /// </remarks>
    public class GraphicsSettingsManager : Microsoft.Xna.Framework.GraphicsDeviceManager
    {   
        #region Constructors
        
        /// <summary>
        /// Constructor for GraphicsSettingsManager
        /// </summary>
        /// <param name="configFilePath">
        /// path to config file
        /// </param>
        public GraphicsSettingsManager(Microsoft.Xna.Framework.Game game) :
            base(game)
        {
            this.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(GraphicsSettingsManager_PreparingDeviceSettings);
            this.DeviceCreated += new EventHandler(GraphicsSettingsManager_DeviceUpdated);
            this.DeviceReset += new EventHandler(GraphicsSettingsManager_DeviceUpdated);
        }

        #endregion

        //----------------------------------------------------------------------------

        //----------------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Configures the Graphics Settings using the GraphicsSettingsConfigFile
        /// </summary>
        /// <returns>true on success, false otherwise</returns>
        public bool Configure(bool isFullScreen, int backbufferWidth, int backbufferHeight, bool aa)
        {

            this.IsFullScreen = isFullScreen;

            this.PreferredBackBufferWidth = backbufferWidth;

            this.PreferredBackBufferHeight = backbufferHeight;

            this.PreferMultiSampling = aa;
            
            return true;
        }

        /// <summary>
        /// Configures the graphics settings using default, low detail settings
        /// </summary>
        /// <returns>return true</returns>
        public bool UseLowestDetailSettings()
        {
            this.PreferredBackBufferWidth = 800;
            this.PreferredBackBufferHeight = 600;

            this.PreferMultiSampling = true;
           
            return true;
        }

        #endregion
            
        //----------------------------------------------------------------------------
        
        #region Internal Properties

        #endregion
        
        //----------------------------------------------------------------------------
        
        #region Internal Methods

        #endregion
        
        //----------------------------------------------------------------------------
        
        #region Protected Methods
            
        #endregion

        //----------------------------------------------------------------------------
        
        #region Protected Members


        #endregion

        //----------------------------------------------------------------------------
        
        #region Private Methods

        /// <summary>
        /// This function is called by the parent class (GraphicsDeviceManager) right before
        /// a new graphics device is created.
        /// If the graphics are configured to use anitaliasing then this function will set the
        /// number of samples that are used. It will start with the sample value from the config
        /// file and iteratively drop to lower sample values until a value that is supported by 
        /// the default adapter is found.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphicsSettingsManager_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            if (this.PreferMultiSampling)
            {
                MultiSampleType type = (MultiSampleType)4;

                int quality = 0;
                GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
                SurfaceFormat format = adapter.CurrentDisplayMode.Format;

                for ( ; type >= MultiSampleType.None; --type)
                {
                    if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                       this.IsFullScreen, type, out quality))
                    {
                        if (quality > 1)
                            quality = 2;

                        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = quality - 1;
                        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = type;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This function is called by the parent class (GraphicsDeviceManager) right after a new 
        /// graphics device has been created.
        /// If the graphics are configured to use full screen then this will apply any gamma ramp
        /// offset specified in the config file to the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphicsSettingsManager_DeviceUpdated(object sender, EventArgs e)
        {
            /*if (this.GraphicsDevice.PresentationParameters.IsFullScreen)
            {
                short[] rgb = m_configFile.GammaRampOffset;

                short redOffset = rgb[0];
                short blueOffset = rgb[1];
                short greenOffset = rgb[2];

                short[] redGammaRamp = new short[256];
                short[] greenGammaRamp = new short[256];
                short[] blueGammaRamp = new short[256];

                GenerateGammaRamp(redOffset,
                                  greenOffset,
                                  blueOffset,
                                  redGammaRamp,
                                  greenGammaRamp,
                                  blueGammaRamp);

                GammaRamp gamma = this.GraphicsDevice.GetGammaRamp();

                gamma.SetRed(redGammaRamp);
                gamma.SetGreen(greenGammaRamp);
                gamma.SetBlue(blueGammaRamp);

                this.GraphicsDevice.SetGammaRamp(true, gamma);
            }*/
        }

        /// <summary>
        /// Generates three 256 element arrays that are used to set the gamma ramp parameter
        /// of the graphics device
        /// </summary>
        /// <param name="redOffset">the red offset</param>
        /// <param name="greenOffset">the green offset</param>
        /// <param name="blueOffset">the blue offset</param>
        /// <param name="newRedRamp">returns the red offset ramp</param>
        /// <param name="newGreenRamp">returns the green offset ramp</param>
        /// <param name="newBlueRamp">returns the blue offset ramp</param>
        private void GenerateGammaRamp(short redOffset, short greenOffset, short blueOffset,
                                       short[] newRedRamp, short[] newGreenRamp, short[] newBlueRamp)
        {
            for (short index = 0; index < 256; ++index)
            {
                int rampValue = index * (redOffset + 256);
                if (rampValue > ushort.MaxValue)
                    rampValue = ushort.MaxValue;

                newRedRamp[index] = (short)rampValue;

                rampValue = index * (greenOffset + 256);
                if (rampValue > ushort.MaxValue)
                    rampValue = ushort.MaxValue;

                newGreenRamp[index] = (short)rampValue;

                rampValue = index * (blueOffset + 256);
                if (rampValue > ushort.MaxValue)
                    rampValue = ushort.MaxValue;

                newBlueRamp[index] = (short)rampValue;
            }
        }

        #endregion

        //----------------------------------------------------------------------------

    }
}