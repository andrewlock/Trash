using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Trash
{
    /// <summary>
    /// This class provides helper functions for making sprite textures and fonts, which are pixel based
    /// get drawn in correct positions and sizes when a window is resized.
    /// For the purposes of this code fullWidthxfullHeight resolution is considered the base 
    /// size. Everything is sized based on its relative value to that.
    /// </summary>
    public class ResizedSpriteBatch : SpriteBatch
    {
        /// <summary>
        /// The full size 
        /// </summary>
        private static int fullHeight;
        private static int fullWidth;

        /// <summary>
        /// The required offset to position the resized sprites correctly
        /// </summary>
        public static Vector2 offset { get; private set; }

        /// <summary>
        /// The required scale to resize the sprites correctly
        /// </summary>
        public static float scale { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device where sprites will be 
        /// drawn.</param>
        public ResizedSpriteBatch(GraphicsDevice graphicsDevice, int GameFullHeight, int GameFullWidth)
            : base(graphicsDevice)
        {
            fullHeight = GameFullHeight;
            fullWidth = GameFullWidth;
            Resize();
        }

        /// <summary>
        /// Default constructor, requires that static fullHeight and fullwidth values are set
        /// </summary>
        /// <param name="graphicsDevice">The graphics device where sprites will be drawn</param>
        public ResizedSpriteBatch(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            Resize();
        }

        /// <summary>
        /// Draws a sprite at a particular position
        /// </summary>
        /// <param name="texture">The texture to take the sprite from. For this overload
        /// it will draw the entire texture.</param>
        /// <param name="position">The position in fullwidthxfullHeight screen space. It will 
        /// actually be drawn in the correct relative position and size</param>
        /// <param name="color">The color to tint the sprite</param>
        public new void Draw(Texture2D texture, Vector2 position, Color color)
        {
            this.Draw(texture, position, null, color);
        }

        /// <summary>
        /// Draws a sprite at a particular position
        /// </summary>
        /// <param name="texture">The texture to take the sprite from. For this overload
        /// it will draw the entire texture.</param>
        /// <param name="destinationRectangle">The destination rectangle in fullwidthxfullHeight screen space. It will 
        /// actually be drawn in the correct relative position and size</param>
        /// <param name="color">The color to tint the sprite</param>
        public new void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            destinationRectangle.X = (int)(destinationRectangle.X * scale + offset.X);
            destinationRectangle.Y = (int)(destinationRectangle.Y * scale + offset.Y);
            destinationRectangle.Width = (int)(destinationRectangle.Width*scale);
            destinationRectangle.Height = (int)(destinationRectangle.Height*scale);

            base.Draw(texture, destinationRectangle, null, color,0, Vector2.Zero, SpriteEffects.None, 0);        
        }

        /// <summary>
        /// Draws a sprite at a particular position
        /// </summary>
        /// <param name="texture">The texture to take the sprite from. For this overload
        /// it will draw the entire texture.</param>
        /// <param name="position">The position in 1280x720 screen space. It will 
        /// actually be drawn in the correct relative position and size</param>
        /// <param name="source">The part of the texture to draw</param>
        /// <param name="color">The color to tint the sprite</param>
        public new void Draw(Texture2D texture, Vector2 position, Rectangle? source, Color color)
        {
            //Scale and move the sprite based on current screen size 
            this.Draw(texture, position, source, color, 0,
                Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draws a sprite at a particular position
        /// </summary>
        /// <param name="texture">The texture to take the sprite from</param>
        /// <param name="position">The position in fullwidthxfullHeight screen space. It will 
        /// actually be drawn in the correct relative position and size</param>
        /// <param name="sourceRectangle">The region of the texture to draw</param>
        /// <param name="color">The color to tint the sprite</param>
        /// <param name="rotation">The rotation to apply</param>
        /// <param name="origin">The origin of the texture</param>
        /// <param name="scale">The scaling to apply. this is in addition to scaling applied due to screen size</param>
        /// <param name="effects">The SpriteEffect to apply to the texture</param>
        /// <param name="layerDepth">the layer depth at which to draw the sprite</param>
        public new void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            //Scale and move the sprite based on current screen size 
            base.Draw(texture, (position * ResizedSpriteBatch.scale) + offset, sourceRectangle, color, rotation, origin, ResizedSpriteBatch.scale*scale, effects, layerDepth);
        }

        /// <summary>
        /// Draws a sprite string at a particular position
        /// </summary>
        /// <param name="spriteFont">The SpriteFont to use to draw the texture</param>
        /// <param name="text">The text to draw</param>
        /// <param name="position">The position in fullwidthxfullHeight screen space. It will 
        /// actually be drawn in the correct relative position and size</param>
        /// <param name="color">The color to tint the sprite</param>
        /// <param name="rotation">The rotation to apply</param>
        /// <param name="origin">The origin of the texture</param>
        /// <param name="scale">The scaling to apply. this is in addition to scaling applied due to screen size</param>
        /// <param name="effects">The SpriteEffect to apply to the texture</param>
        /// <param name="layerDepth">the layer depth at which to draw the sprite</param>
        public new void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            //scale and move the string based on the current screen size
            base.DrawString(spriteFont, text, (position * ResizedSpriteBatch.scale) + ResizedSpriteBatch.offset, color, rotation, origin, ResizedSpriteBatch.scale * scale, effects, layerDepth);
        }

        /// <summary>
        /// Adjust the scale and location of the drawn graphics so that everything 
        /// fits on the screen and is centered.
        /// </summary>
        public void Resize()
        {
            // Scale is used to stretch or shrink the drawn images so that everything
            // is visible on screen.
            scale =
                Math.Min((float)GraphicsDevice.Viewport.Height / (float)fullHeight,
                (float)GraphicsDevice.Viewport.Width / (float)fullWidth);
            // The offset used to center the drawn images on the screen
            offset =
                new Vector2((GraphicsDevice.Viewport.Width - fullWidth * scale) / 2,
                (GraphicsDevice.Viewport.Height - fullHeight * scale) / 2);
        }
    }
}
