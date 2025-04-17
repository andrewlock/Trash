using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// Enum for the scroll direction, currently only vertical scrolling supported
    /// </summary>
    public enum ScrollDirection
    {
        Up,
        Down
    }

    /// <summary>
    /// An abstract class for a scrolling object, contains variables for general scrolling
    /// </summary>
    public abstract class ScrollingObject
    {
        protected Vector2 currentPosition;
        protected float frameScrollDistance = 1;

        //control whether to display the text and whether to scroll it
        protected bool isScrolling = true;
        public bool IsScrolling { get { return isScrolling; } set { isScrolling = value; } }
        protected bool isVisible = true;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; } }
        
        //whether to loop the scrolling object, the gap between each loop and the start offset
        protected bool loop = true;
        public bool Loop { get { return loop; } set { loop = value; } }
        protected float minimumGapSize = 0;
        protected float startOffset = 0;

        //The scroll speed
        protected int millisecondsPerScrollFrame = 16;
        protected int millisecondsSinceLastScroll = 0;
        protected Rectangle scrollArea = Rectangle.Empty;
        protected ScrollDirection scrollDirection;
        protected Vector2 size = Vector2.Zero;

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime);

    }

    public class ScrollingFont : ScrollingObject
    {
        //variables for specifying the drawing style
        string font = "";
        Color color = Color.White;
        bool wrapText;
        bool checkFormatting = false;
        TextVerticalAlignment vAlignment = TextVerticalAlignment.Top;
        TextAlignment alignment = TextAlignment.Left;
        float maxLineWidth;
        
        //The text to draw and the size of the passage
        List<TextLine> text = new List<TextLine>();
        Vector2 textSize = Vector2.Zero;
        
        //Position parameters and whether to use scissor rectangle for smoother area exit
        //at the expense of an additional spriteBatch call
        int currentLine = 0;
        int numGapLinesAdded = 0;
        int distanceSinceLastLine = 0;
        bool smoothBorderExit = false;

        /// <summary>
        /// Constructor for scrolling font object
        /// </summary>
        /// <param name="font">The default font to use</param>
        /// <param name="text">The text to scroll</param>
        /// <param name="color">the default color to draw the text</param>
        /// <param name="scrollArea">The area within which the text must scroll</param>
        /// <param name="scrollDirection">The direction for the text to move on the screen</param>
        /// <param name="wrapText">Whether or not to wrap the text. If false, then the width of the text 
        /// must be less than the scroll area (unless using smooth border exit)</param>
        /// <param name="maxLineWidth">The maximum line width to use when calculating text wrapping</param>
        /// <param name="loop">Whether the text should scroll once past the scrollarea or loop continuously </param>
        /// <param name="minimumGapSize">The minimum gap to display between each loop of the text</param>
        /// <param name="startOffset">The vertical distance to offset the text when drawing</param>
        /// <param name="textAlignment">The horizontal allignment to apply to the text</param>
        /// <param name="textVerticalAlignment">The vertical alginment to apply to the text. 
        /// For vertical scrolling, only top alignment is valid</param>
        /// <param name="checkFormatting">Whether to apply formatting markup tags <see cref="Font"/> 
        /// specified within the text</param>
        /// <param name="smoothBorderExit">True to enable smooth exiting of text outside scroll area borders.
        /// If true then a call to Font.SpriteBatch.Begin is made to set a scissorRectangle clipping area.
        /// If false then the draw call must be made between existing Font.SpriteBatch.Begin and End calls</param>
        public ScrollingFont(string font, string text, Color color, Rectangle scrollArea,
            ScrollDirection scrollDirection, bool wrapText, float maxLineWidth, bool loop, float minimumGapSize,
            float startOffset, TextAlignment textAlignment, TextVerticalAlignment textVerticalAlignment, bool checkFormatting,
            bool smoothBorderExit)
        {

            if (scrollDirection == ScrollDirection.Up || scrollDirection == ScrollDirection.Down)
                textVerticalAlignment = TextVerticalAlignment.Top;
            this.font = font;
            this.color = color;
            this.scrollArea = scrollArea;
            this.scrollDirection = scrollDirection;
            this.wrapText = wrapText;
            this.maxLineWidth = maxLineWidth;
            this.loop = loop;
            this.startOffset = startOffset;
            this.minimumGapSize = minimumGapSize;
            this.alignment = textAlignment;
            this.checkFormatting = checkFormatting;
            this.vAlignment = textVerticalAlignment;
            this.smoothBorderExit = smoothBorderExit;
            if (wrapText)
                TextDrawer.WrapText(text, font, maxLineWidth, out this.text, checkFormatting);
            else
            {
                //TODO:Need to account for formatting here
                this.text.Add(new TextLine(text, font));
                TextDrawer.RemoveCarriageReturns(ref this.text, font);
            }


            AddGap();
            CalculateSize();

            if (!wrapText && textSize.X > scrollArea.Width)
                throw new ArgumentException("When wrapText = false, text width must not be greater than scroll area width");
            CalculateStartPosition();
        }

        /// <summary>
        /// Adds a gap at the start of the text and update currentDrawingLine
        /// (Added to start not end to ensure we use the default font both during size calculation and drawing)
        /// </summary>
        private void AddGap()
        {
            if (minimumGapSize > 0)
            {
                //find the vertical height of a space character in the starting font
                Vector2 charSize = TextDrawer.GetSpriteFont(font).MeasureString(" ");

                //calculate number of lines to add
                numGapLinesAdded = (int)(minimumGapSize / charSize.Y) + 1;

                //add the required number of textlines to the start of the text
                for (int i = 0; i < numGapLinesAdded; i++)
                    text.Insert(0, (new TextLine(" ", charSize)));

                //adjust the current line to account for the added blank lines
                currentLine += numGapLinesAdded;
            }
        }

        /// <summary>
        /// Calculate the size of the given text passage after wrapping
        /// </summary>
        private void CalculateSize()
        {
            //calculate the size of the text area, the y is the sum of all lines, x is the max
            float maxLineWidth = 0;
            float totalLineHeight = 0;

            foreach (TextLine line in this.text)
            {
                if (line.LineDimensions.X > maxLineWidth)
                    maxLineWidth = line.LineDimensions.X;
                totalLineHeight += line.LineDimensions.Y;
            }

            size = new Vector2(maxLineWidth, totalLineHeight);

        }

        /// <summary>
        /// Calculate the required start position, copmensating for any gap added at start of text
        /// </summary>
        private void CalculateStartPosition()
        {
            //set the current position based on the scrollArea defined, and the start offset
            //also need to compensate for the lines added at the front of the text in AddGap
            //switch (scrollDirection)
            //{
            //    case ScrollDirection.Up:
            //        break;
            //    case ScrollDirection.Down:
            //        break;
            //}

            float requiredOffset = startOffset;
            for (int i = 0; i < numGapLinesAdded; i++)
                requiredOffset -= text[i].LineDimensions.Y;

            distanceSinceLastLine = (int)requiredOffset;

            switch (alignment)
            {
                case TextAlignment.Left:
                    currentPosition = new Vector2((float)scrollArea.Left, scrollArea.Top + requiredOffset);
                    break;
                case TextAlignment.Right:
                    currentPosition = new Vector2((float)scrollArea.Right, scrollArea.Top + requiredOffset);
                    break;
                case TextAlignment.Center:
                    currentPosition = new Vector2((float)scrollArea.Center.X, scrollArea.Top + requiredOffset);
                    break;
            }
        }

        /// <summary>
        /// Draw the scrolling font
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            if (isVisible)
            {
                if (smoothBorderExit)
                    DrawScissored(gameTime);
                else
                    DrawUnScissored(gameTime);
            }

        }

        /// <summary>
        /// Draws the scrolling font using the Scissor rectangle technique 
        /// to give smooth exits from the scroll area at expense of addition spritebatch.begin - end pair
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void DrawScissored(GameTime gameTime)
        {
            //uses the spritebatch specified for font to calculate scissor areas
            //and starts a new Begin section as only want to scissor the scrolling text
            ResizedSpriteBatch spriteBatch = TextDrawer.spriteBatch;
            spriteBatch.Begin();

            //starts a new sprite batch, sets the scissor rectangle property 
            Rectangle defaultScissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
            bool defaultUseScissorTest = spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable;

            //when setting the scissor area we have to account for the scaling of the screen
            Rectangle scissorArea = new Rectangle((int)(spriteBatch.GraphicsDevice.Viewport.X + scrollArea.X * ResizedSpriteBatch.scale + ResizedSpriteBatch.offset.X), (int)(spriteBatch.GraphicsDevice.Viewport.Y + scrollArea.Y * ResizedSpriteBatch.scale + ResizedSpriteBatch.offset.Y),
                (int)(scrollArea.Width * ResizedSpriteBatch.scale), (int)(scrollArea.Height * ResizedSpriteBatch.scale));

            //Want to do the scissor rectangle with regard to the current viewport
            spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = true;
            spriteBatch.GraphicsDevice.ScissorRectangle = scissorArea;

            Vector2 position = currentPosition;

            TextDrawer.CurrentFont = font;
            TextDrawer.CurrentColor = color;

            //starting at current line and position, draw each line in the text
            for (int i = 0; i < text.Count; i++)
            {
                //draw the line in the given position, 
                //persist formatting color and fonts by using font properties
                TextDrawer.DrawText(TextDrawer.CurrentFont, text[i].Line, position, TextDrawer.CurrentColor, alignment, TextVerticalAlignment.Top, checkFormatting,gameTime);
                // move the draw position by the size of the last line
                position += new Vector2(0, text[i].LineDimensions.Y);
            }

            //if we have specified to loop the text then keep drawing until we have filled the scroll area
            if (loop)
            {
                while (position.Y < scrollArea.Bottom)
                {
                    TextDrawer.CurrentFont = font;
                    TextDrawer.CurrentColor = color;

                    for (int i = 0; i < text.Count; i++)
                    {
                        //draw the line in the given position
                        TextDrawer.DrawText(TextDrawer.CurrentFont, text[i].Line, position, TextDrawer.CurrentColor, alignment, TextVerticalAlignment.Top, checkFormatting,gameTime);
                        // move the draw position by the size of the last line
                        position += new Vector2(0, text[i].LineDimensions.Y);
                    }
                }
            }
            spriteBatch.End();

            //reset the previous scissorRectangle parameters
            spriteBatch.GraphicsDevice.ScissorRectangle = defaultScissorRectangle;
            spriteBatch.GraphicsDevice.RenderState.ScissorTestEnable = defaultUseScissorTest;

        }

        /// <summary>
        /// Draws the scrolling font without using the scissor rectangle technique
        /// to draw within current begin-end pair at expense of less smooth exit of text from scroll area
        /// </summary>
        /// <param name="gameTime">Provides a sanpshot of timing values</param>
        private void DrawUnScissored(GameTime gameTime)
        {
            Vector2 position = currentPosition;

            TextDrawer.CurrentFont = font;
            TextDrawer.CurrentColor = color;

            if (checkFormatting)
            {
                //first we get all the formatting elements up to the current line 
                //to ensure that we have the correct formatting when we begin drawing strings
                List<FontFormattingElement> previousElements = new List<FontFormattingElement>();
                for (int i = 0; i < currentLine; i++)
                {
                    previousElements = TextDrawer.GetFormattingElements(text[i].Line);
                    //apply all the formatting changes without drawing any text
                    foreach (FontFormattingElement element in previousElements)
                        TextDrawer.ApplyFormatting(element);
                }
            }

            bool insideArea = true;
            //starting at current line and position, draw each line in the text
            for (int i = currentLine; i < text.Count; i++)
            {
                if (position.Y + text[i].LineDimensions.Y >= scrollArea.Bottom)
                {
                    insideArea = false;
                    break;
                }                    //if we are out of the draw area then break out of the loop
                //draw the line in the given position, 
                //persist formatting color and fonts by using font properties
                TextDrawer.DrawText(TextDrawer.CurrentFont, text[i].Line, position, TextDrawer.CurrentColor, alignment, TextVerticalAlignment.Top, checkFormatting,gameTime);
                // move the draw position by the size of the last line
                position += new Vector2(0, text[i].LineDimensions.Y);
            }

            //if we have specified to loop the text then keep drawing until we have filled the scroll area
            //stop just before we exceed it
            if (loop)
            {

                //continually loop, we will check for the break condition inside;
                while (insideArea)
                {
                    //reset the font to the  start font and color
                    TextDrawer.CurrentFont = font;
                    TextDrawer.CurrentColor = color;

                    for (int i = 0; i < text.Count; i++)
                    {
                        if (position.Y >= scrollArea.Bottom)
                        {
                            insideArea = false;
                            break;
                        }
                        //draw the line in the given position
                        TextDrawer.DrawText(TextDrawer.CurrentFont, text[i].Line, position, TextDrawer.CurrentColor, alignment, TextVerticalAlignment.Top, checkFormatting,gameTime);
                        // move the draw position by the size of the last line
                        position += new Vector2(0, text[i].LineDimensions.Y);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the scrolling text to it's start position
        /// </summary>
        public void Reset()
        {
            currentLine = numGapLinesAdded;
            millisecondsSinceLastScroll = 0;
            CalculateStartPosition();
        }

        /// <summary>
        /// Update the position and current line of the scrolling text
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            //if not scrolling we don't need to recalculate
            if (isScrolling)
            {
                //check if it is time to move
                millisecondsSinceLastScroll += gameTime.ElapsedGameTime.Milliseconds;
                if (millisecondsSinceLastScroll > millisecondsPerScrollFrame)
                {
                    millisecondsSinceLastScroll = 0;

                    Vector2 distanceToMove = Vector2.Zero;
                    //Recalculate the drawing location
                    switch (scrollDirection)
                    {
                        //scrolling up
                        case ScrollDirection.Up:
                            distanceToMove = new Vector2(0, -frameScrollDistance);
                            //check to see if we need to update the current line 
                            distanceSinceLastLine -= (int)frameScrollDistance;

                            //if we are not using scissor rectangle then need to keep track of which line to use
                            if (!smoothBorderExit && distanceSinceLastLine <= 0)
                            {
                                distanceToMove.Y += text[currentLine].LineDimensions.Y;
                                distanceSinceLastLine = (int)text[currentLine].LineDimensions.Y;
                                currentLine = (currentLine + 1) % text.Count;
                            }

                            break;
                        //scrolling down
                        case ScrollDirection.Down:
                            distanceToMove = new Vector2(0, frameScrollDistance);
                            distanceSinceLastLine -= (int)frameScrollDistance;
                            if (!smoothBorderExit && distanceSinceLastLine <= 0)
                            {
                                distanceSinceLastLine = (int)text[currentLine].LineDimensions.Y;
                                currentLine = (currentLine + text.Count - 1) % text.Count;
                                distanceToMove.Y -= text[currentLine].LineDimensions.Y;
                            }
                            break;
                    }
                    currentPosition += distanceToMove;
                }
            }
        }
    }
}
