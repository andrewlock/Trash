using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Trash
{
    /// <summary>
    /// Enum for horizontal alignments
    /// </summary>
    public enum TextAlignment
    {
        Left,
        Right,
        Center
    }

    /// <summary>
    /// Enum for Vertical Alignments
    /// </summary>
    public enum TextVerticalAlignment
    {
        Top,
        Bottom,
        Middle
    }

    /// <summary>
    /// Enum for valid formatting types
    /// </summary>
    public enum FontFormattingType
    {
        None,
        Font,
        Color,
        Flashing
    }

    /// <summary>
    /// Class that encapsulates a formatted string
    /// </summary>
    [DebuggerDisplay("{Text}, Tag={FormattingElementString}")]
    public class FontFormattingElement
    {

        /// <summary>
        /// The full formatting string specifed
        /// </summary>
        public string FormattingElementString;

        /// <summary>
        /// The type of formatting applied
        /// </summary>
        public FontFormattingType FormattingType;

        /// <summary>
        /// The value of the tag with tag identifier removed
        /// </summary>
        public string TagValue;

        /// <summary>
        /// The text of the string with formatting tags removed
        /// </summary>
        public string Text;

        /// <summary>
        /// Constructor for FontFormattingElement
        /// </summary>
        /// <param name="formattingType">The type of formatting</param>
        /// <param name="text">The text of the string with formatting removed</param>
        /// <param name="tagValue">The value of the tag with tag identifier removed</param>
        /// <param name="formattingElement">The full text of the formatting element</param>
        public FontFormattingElement(FontFormattingType formattingType,
            string text, string tagValue, string formattingElement)
        {
            FormattingType = formattingType;
            Text = text;
            TagValue = tagValue;
            FormattingElementString = formattingElement;
        }

        /// <summary>
        /// Default constructor, specifes a null string with no formatting
        /// </summary>
        public FontFormattingElement()
        {
            FormattingType = FontFormattingType.None;
            Text = "";
            TagValue = "";
            FormattingElementString = "";
        }
    }

    /// <summary>
    /// A line of text with stored dimensions to avoid repeated re-evaluation
    /// </summary>
    [DebuggerDisplay("{Line}, Dimensions ={LineDimensions.X},{LineDimensions.Y}")]
    public class TextLine
    {

        /// <summary>
        /// The text
        /// </summary>
        public string Line { get; private set; }

        /// <summary>
        /// The dimensions of the line
        /// </summary>
        public Vector2 LineDimensions { get; private set; }


        /// <summary>
        /// Constructor that creates the textline
        /// </summary>
        /// <param name="text">The text of the line</param>
        /// <param name="font">The font to use to measure the line</param>
        public TextLine(string text, SpriteFont font)
        {
            this.Line = text;
            LineDimensions = font.MeasureString(Line);
        }

        /// <summary>
        /// Constructor that creates the textline
        /// </summary>
        /// <param name="text">The text of the line</param>
        /// <param name="font">The font to use to measure the line</param>
        public TextLine(string text, string font)
        {
            this.Line = text;
            LineDimensions = TextDrawer.GetSpriteFont(font).MeasureString(Line);
        }

        /// <summary>
        /// Constructor that allows the user to set the recorded dimensions of the specified string
        /// Useful when the string contains formatting with different font sizes
        /// </summary>
        /// <param name="text">The text of the line</param>
        /// <param name="dimensions">the recorded dimensions of the line</param>
        public TextLine(string text, Vector2 dimensions)
        {
            Line = text;
            LineDimensions = dimensions;
        }


        /// <summary>
        /// Used to convert a TextLine to a string
        /// </summary>
        /// <param name="line">The TextLine to convert</param>
        /// <returns>The string equivalent</returns>
        public static string GetString(TextLine line)
        {
            return line.Line;
        }
    }

    [DebuggerDisplay("Font = {currentFont}, Color ={courrentColor}")]
    public static class TextDrawer
    {
        static ContentManager contentManager;
        
        //keeps track of whether we should be drawing flashing fonts or not
        static bool flashingActive = false;
        static int millisecondsFlashingTextShown = 1000;
        
        //List of all the fonts loaded by the class
        static Dictionary<string, SpriteFont> fontDictionary = new Dictionary<string, SpriteFont>();


        static Color currentColor = Color.White;
        /// <summary>
        /// The current color in which to draw strings
        /// </summary>
        public static Color CurrentColor { get; set; }

        static string currentFont = "";
        /// <summary>
        /// The name of the current font to use to draw string
        /// </summary>
        public static string CurrentFont
        {
            get { return currentFont; }
            set
            {
                if (fontDictionary.ContainsKey(value))
                    currentFont = value;
            }
        }

        /// <summary>
        /// The SpriteFont corresponding to CurrentFont name
        /// </summary>
        public static SpriteFont CurrentSpriteFont
        {
            get
            {
                if (fontDictionary.ContainsKey(currentFont))
                    return fontDictionary[currentFont];
                else
                    return null;

            }
        }

        static int FlashTime { get { return millisecondsFlashingTextShown; } set { millisecondsFlashingTextShown = value; } }

        /// <summary>
        /// The SpriteBatch to use to draw the strings
        /// </summary>
        public static ResizedSpriteBatch spriteBatch { get; set; }


        /// <summary>
        /// Change the current drawing parameters based on a formatting tag, ignoring flashing text
        /// </summary>
        /// <param name="element">The font formatting element whose formatting to apply</param>
        /// <returns>Whether the text should be displayed or not(for flashing text)</returns>
        public static void ApplyFormatting(FontFormattingElement element)
        {
            ApplyFormatting(element, new GameTime());
        }

        /// <summary>
        /// Change the current drawing parameters based on a formatting tag
        /// </summary>
        /// <param name="element">The font formatting element whose formatting to apply</param>
        /// <returns>Whether the text should be displayed or not(for flashing text)</returns>
        public static bool ApplyFormatting(FontFormattingElement element, GameTime gameTime)
        {
            switch (element.FormattingType)
            {
                case FontFormattingType.None:
                    break;
                case FontFormattingType.Font:
                    CurrentFont = element.TagValue;
                    break;
                case FontFormattingType.Color:
                    CurrentColor = ConvertHexStringToColor(element.TagValue);
                    break;
                case FontFormattingType.Flashing:
                    //Flash the text depending on the gameTime and if flashing was just turned on or off
                    //Turn on or off flashing as required
                    if (element.TagValue == "ON")
                        flashingActive = true;
                    else
                        flashingActive = false;
                    break;
            }
            if (flashingActive)
                if (gameTime.TotalGameTime.Milliseconds % millisecondsFlashingTextShown < millisecondsFlashingTextShown / 2)
                    return false;
                else
                    return true;
            return true;
        }

        /// <summary>
        /// Load all the fonts in the given Content sub folder
        /// </summary>
        /// <param name="contentFontsFolder">The name of the folder</param>
        static public void AutoLoadFonts(string contentFontsFolder)
        {
            // Throw an exception if we don't have a content manager.
            if (contentManager == null)
                throw new Exception("TextHandler ContentManager Not Initialized");

            // Get all of the files in the passed folder (they will be .XNB files)
            // Note that it will try to load any .XNB files in this folde as SpriteFonts, so that
            // should be the only type of content located there.
            DirectoryInfo info = new DirectoryInfo(contentManager.RootDirectory + @"\" + contentFontsFolder);
            foreach (FileInfo file in info.GetFiles("*.XNB"))
            {
                // Trim off the .XNB extension
                string sFontName = file.Name.Substring(0, file.Name.Length - 4);

                // Load the font, giving it a dictionary name equal to the .spritefont asset name.
                LoadFont(sFontName, contentFontsFolder + @"\" + sFontName);
            }
        }

        /// <summary>
        /// Used by formatting functions to convert hex string into Color i.e FFFFFF = Color.White
        /// </summary>
        /// <param name="hexString">Hexstring to convert</param>
        /// <returns>The resulting Color</returns>
        private static Color ConvertHexStringToColor(string hexString)
        {
            // Pull out the characters
            byte r = byte.Parse(hexString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // Set current color to the desired color.
            return new Color(r, g, b);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        static public void DrawText(string text, Vector2 position)
        {
            DrawText(currentFont, text, position, 0, currentColor, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        static public void DrawText(string font, string text, Vector2 position)
        {
            DrawText(font, text, position, 0, currentColor, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        static public void DrawText(string text, Vector2 position, Color color)
        {
            DrawText(currentFont, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string text, Vector2 position, bool checkFormatting, GameTime gameTime)
        {
            DrawText(currentFont, text, position, 0, currentColor, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        static public void DrawText(string font, string text, Vector2 position, Color color)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string font, string text, Vector2 position, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, currentColor, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string text, Vector2 position, Color color, bool checkFormatting, GameTime gameTime)
        {
            DrawText(currentFont, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, float maxLineWidth)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, maxLineWidth);

        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string font, string text, Vector2 position, Color color, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, TextAlignment alignment, TextVerticalAlignment vAlignment)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, alignment, vAlignment, 0);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, float maxLineWidth, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Left, TextVerticalAlignment.Top, maxLineWidth, checkFormatting, gameTime);

        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, TextAlignment alignment, TextVerticalAlignment vAlignment,
            float maxLineWidth)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, alignment, vAlignment, maxLineWidth);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, TextAlignment alignment, TextVerticalAlignment vAlignment, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, alignment, vAlignment, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="color">Color to use</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        /// <param name="checkFormatting">Whether to use custom formatting of the string</param>
        static public void DrawText(string font, string text, Vector2 position,
            Color color, TextAlignment alignment, TextVerticalAlignment vAlignment,
            float maxLineWidth, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, alignment, vAlignment, maxLineWidth, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function for drawing text, does not use formatting
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="angle">Rotation angle to draw the text</param>
        /// <param name="color">Color to use</param>
        /// <param name="origin">The origin of the string, typically top left corner</param>
        /// <param name="scale">The scale of the string</param>
        /// <param name="spriteEffects">SpriteEffects to use</param>
        /// <param name="layerDepth">Layer depth of the text</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        static public void DrawText(string font, string text, Vector2 position, float angle,
            Color color, Vector2 origin, float scale,
            SpriteEffects spriteEffects, float layerDepth, TextAlignment alignment,
            TextVerticalAlignment vAlignment, float maxLineWidth)
        {

            DrawText(font, text, position, angle, color, origin, scale, spriteEffects, layerDepth, alignment,
                vAlignment, maxLineWidth, false, new GameTime());
        }

        /// <summary>
        /// Function for drawing text
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text, typically top left corner</param>
        /// <param name="angle">Rotation angle to draw the text</param>
        /// <param name="color">Color to use</param>
        /// <param name="origin">The origin of the string, typically top left corner</param>
        /// <param name="scale">The scale of the string</param>
        /// <param name="spriteEffects">SpriteEffects to use</param>
        /// <param name="layerDepth">Layer depth of the text</param>
        /// <param name="alignment">Horizontal alignment of the string</param>
        /// <param name="vAlignment">Vertical alignment of the string</param>
        /// <param name="wrapText">Whether the text should be wrapped</param>
        /// <param name="maxLineWidth">The maximum line width for text wrapping</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawText(string font, string text, Vector2 position, float angle,
            Color color, Vector2 origin, float scale,
            SpriteEffects spriteEffects, float layerDepth, TextAlignment alignment,
            TextVerticalAlignment vAlignment, float maxLineWidth, bool checkFormatting, GameTime gameTime)
        {
            // Throw an exception if we don't have a spritebatch.
            if (spriteBatch == null)
                throw new Exception("TextDrawer SpriteBatch Not Initialized");

            // Make sure the font exists in our font dictionary.  Otherwise, default to the CurrentFont
            CurrentFont = font;
            CurrentColor = color;

            //if text wrapping is required, split the string if required, 
            //converting to textLines to measure the strings and using formatting if required
            List<TextLine> textToDraw;
            if (maxLineWidth > 0)
                WrapText(text, font, maxLineWidth, out textToDraw, checkFormatting);
            else
            {
                textToDraw = new List<TextLine>(1);
                if (checkFormatting)
                    textToDraw.Add(MeasureFormattedString(text, font));
                else
                    textToDraw.Add(new TextLine(text, font));
                //To do alignments correctly we need to remove cariage returns by splitting strings as necessary
                //This is handled automatically in wrapText
                RemoveCarriageReturns(ref textToDraw);
            }

            //need to adjust the position for every line depending on the vertical alignment specified
            float textHeight = 0;
            foreach (TextLine line in textToDraw)
                textHeight += line.LineDimensions.Y;

            //The current curosr position
            Vector2 cursorPosition = position;
            switch (vAlignment)
            {
                //do not need to adjust for top alignment
                case TextVerticalAlignment.Top:
                    break;
                //subtract the total width to ensure end of text aligns with specified position
                case TextVerticalAlignment.Bottom:
                    cursorPosition.Y -= textHeight;
                    break;
                //subtract half the text width to ensure middle of string aligns with specified position
                case TextVerticalAlignment.Middle:
                    cursorPosition.Y -= textHeight / 2;
                    break;
            }

            //keep track of horizontal and vertical offsets
            float horizontalOffset = 0;
            float verticalOffset = 0;

            //loop through each line, updating the horizontal offset if required
            foreach (TextLine line in textToDraw)
            {
                switch (alignment)
                {
                    //do not need to adjust for left alignment
                    case TextAlignment.Left:
                        break;
                    //subtract the string width to ensure end of string aligns with specified position
                    case TextAlignment.Right:
                        horizontalOffset = -line.LineDimensions.X;
                        break;
                    //subtract half the string width to ensure middle of string aligns with specified position
                    case TextAlignment.Center:
                        horizontalOffset = -line.LineDimensions.X / 2;
                        break;
                }

                //loop through every formatting element, adjust parameters as necessary and draw
                foreach (FontFormattingElement element in GetFormattingElements(line.Line))
                {
                    //adjust any features specified by the formatting tag
                    bool showText = ApplyFormatting(element, gameTime);
                    if (showText)
                    {
                        // Use SpriteBatch to draw the text string.
                        spriteBatch.DrawString(fontDictionary[CurrentFont],
                            element.Text,
                            cursorPosition + new Vector2(horizontalOffset, verticalOffset),
                            CurrentColor,
                            angle,
                            origin,
                            scale,
                            spriteEffects,
                            layerDepth);
                    }
                }
                //move the cursor position for the next line
                verticalOffset += line.LineDimensions.Y;
            }
        }

        /// <summary>
        /// Function for drawing text in center. Identical to public void DrawText(string font, string text, Vector2 position,
        /// Color color, TextAlignment alignment, TextVerticalAlignment vAlignment)with alignment set to center
        /// and vAlignment set to middle
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text (where to place center of string)</param>
        /// <param name="color">Color to use</param>
        static public void DrawTextCentered(string font, string text, Vector2 position, Color color)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Center, TextVerticalAlignment.Middle, 0);
        }

        /// <summary>
        /// Function for drawing text in center. Identical to public void DrawText(string font, string text, Vector2 position,
        /// Color color, TextAlignment alignment, TextVerticalAlignment vAlignment)with alignment set to center
        /// and vAlignment set to middle
        /// </summary>
        /// <param name="font">Name of the font to use</param>
        /// <param name="text">Text to right</param>
        /// <param name="position">Position reference point for the text (where to place center of string)</param>
        /// <param name="color">Color to use</param>
        /// <param name="checkFormatting">Whether to use custom formatting for the string</param>
        static public void DrawTextCentered(string font, string text, Vector2 position, Color color, bool checkFormatting, GameTime gameTime)
        {
            DrawText(font, text, position, 0, color, Vector2.Zero, 1f,
                SpriteEffects.None, 0, TextAlignment.Center, TextVerticalAlignment.Middle, 0, checkFormatting, gameTime);
        }

        /// <summary>
        /// Function used to create a list of tag - text pairs for use when calaculating wrapping 
        /// and when drawing text
        /// </summary>
        /// <param name="line">The string to parse</param>
        /// <returns>the resulting list of FontFormattingElements</returns>
        static public List<FontFormattingElement> GetFormattingElements(string line)
        {
            List<FontFormattingElement> tagList = new List<FontFormattingElement>();
            FontFormattingElement element = null;
            string preText, midText;
            //keep checking the string until we have removed all the tags
            while (line.Contains("!["))
            {
                preText = line.Substring(0, line.IndexOf("!["));
                midText = line.Substring(line.IndexOf("![") + 2);
                line = midText.Substring(midText.IndexOf("]") + 1);
                midText = midText.Substring(0, (midText.Length - line.Length - 1));

                //if we have a formatting code
                if (midText.Length > 0)
                {
                    //if we have a previous fontFormattingElement, assign this pretext to 
                    //it's posttext, add to the list
                    if (element != null)
                    {
                        element.Text = preText;
                        tagList.Add(element);
                    }
                    //Other wise add a new non-formatting element to hold the pretext and create a
                    //new tag for the post text tag and assign preText 
                    else
                        tagList.Add(new FontFormattingElement(FontFormattingType.None, preText, "", ""));

                    //create a new element for our text
                    element = new FontFormattingElement();

                    //set the element's formatting string value
                    //This makes it easy to reinsert into string as required
                    element.FormattingElementString = "![" + midText + "]";
                    //check if font change specified
                    if (midText.StartsWith("F:"))
                    {
                        //pull out string and set element fields
                        element.FormattingType = FontFormattingType.Font;
                        element.TagValue = midText.Substring(2);
                    }
                    // # sign indicates an HTML-like color code (ie, #FFFFFF = White)
                    else if (midText.StartsWith("#:"))
                    {
                        //pull out string and set element fields
                        element.FormattingType = FontFormattingType.Color;
                        element.TagValue = midText.Substring(2);
                    }
                    else if (midText.StartsWith("Flash:"))
                    {
                        //pull out string and set element fields
                        element.FormattingType = FontFormattingType.Flashing;
                        element.TagValue = midText.Substring(6);
                    }
                    else
                    {
                        Debugger.Break();
                    }
                }
            }
            //if we have an element and any remaining text 
            //then we need to set it's post text value and add to the list
            if (element != null)
            {
                element.Text = line;
                tagList.Add(element);
            }
            //if we do not have an element then we need to create one with the entire string
            else
                tagList.Add(new FontFormattingElement(FontFormattingType.None, line, "", ""));

            return tagList;
        }

        /// <summary>
        /// Get the sprite font with the given name
        /// </summary>
        /// <param name="font">The name of the font to get</param>
        /// <returns></returns>
        public static SpriteFont GetSpriteFont(string font)
        {
            if (fontDictionary.ContainsKey(font))
                return fontDictionary[font];
            else
                return CurrentSpriteFont;
        }

        /// <summary>
        /// Inititalises the fonts class by passing in the sprite batch and content manager
        /// If a fonts folder is provided then autoloads all the fonts in this folder
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to use</param>
        /// <param name="contentManager">The ContentManager to use</param>
        /// <param name="fontsFolderPrefix">The sub folder of Content to search. </param>
        public static void Initialize(ResizedSpriteBatch spriteBatch, ContentManager contentManager, string fontsFolderPrefix)
        {
            TextDrawer.spriteBatch = spriteBatch;
            TextDrawer.contentManager = contentManager;
            if (fontsFolderPrefix != "")
                AutoLoadFonts(fontsFolderPrefix);
        }

        /// <summary>
        /// Load a font into the font dictionary
        /// </summary>
        /// <param name="fontName">The name of the font to use for reference later</param>
        /// <param name="resourceName">The name of the font as shown in the Content Manager</param>
        static public void LoadFont(string fontName, string resourceName)
        {
            // Throw an exception if we don't have a content manager.
            if (contentManager == null)
                throw new Exception("TextHandler ContentManager Not Initialized");

            fontDictionary.Add(fontName, contentManager.Load<SpriteFont>(resourceName));
            if (currentFont == "")
                currentFont = fontName;
        }

        /// <summary>
        /// Measures a formatted string acounting for any formatting tags by wrapping it with a TextLine
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="startFont">The font to start measuring in, if it changes mid string it will be changed</param>
        /// <returns>The resulting TextLine containg size information</returns>
        static public TextLine MeasureFormattedString(string text, ref string startFont)
        {
            return MeasureFormattedString(text, startFont, out startFont);
        }

        /// <summary>
        /// Measures a formatted string acounting for any formatting tags by wrapping it with a TextLine
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="startFont">The font to start measuring in, if it changes mid string it will be changed</param>
        /// <returns>The resulting TextLine containg size information</returns>
        static public TextLine MeasureFormattedString(string text, string startFont)
        {
            string outFont;
            return MeasureFormattedString(text, startFont, out outFont);
        }

        /// <summary>
        /// Measures a formatted string acounting for any formatting tags by wrapping it with a TextLine
        /// </summary>
        /// <param name="text">The text to measure</param>
        /// <param name="font">The font to start measuring in</param>
        /// <param name="endFont">The final font used after formatting chages</param>
        /// <returns>The resulting TextLine containg size information</returns>
        static public TextLine MeasureFormattedString(string text, string startFont, out string endFont)
        {
            //the starting spritefont
            SpriteFont spriteFont = GetSpriteFont(startFont);

            //a list of all the formatting elements in the string
            List<FontFormattingElement> elements = TextDrawer.GetFormattingElements(text);
            float maxHeight = 0;
            float lineWidth = 0;
            foreach (FontFormattingElement element in elements)
            {
                //adjust the font if required, we do not actually want to apply the formatting to the 
                //Font class by changing current font etc so do not call ApplyFormatting()
                if (element.FormattingType == FontFormattingType.Font)
                {
                    startFont = element.TagValue;
                    spriteFont = GetSpriteFont(startFont);
                }
                float height = spriteFont.MeasureString(element.Text).Y;
                if (height > maxHeight)
                    maxHeight = height;
                lineWidth += spriteFont.MeasureString(element.Text).X;
            }
            endFont = startFont;
            return new TextLine(text, new Vector2(lineWidth, maxHeight));

        }

        /// <summary>
        /// Remove carriage returns denoted by \n by slitting the string into several entries
        /// and replacing in the list
        /// </summary>
        /// <param name="textToDraw">The list with strings to be searched</param>
        /// <param name="font">The font to use to measure the string for the TextLines created</param>
        public static void RemoveCarriageReturns(ref List<TextLine> text)
        {
            RemoveCarriageReturns(ref text, currentFont);
        }

        /// <summary>
        /// Remove carriage returns denoted by \n by slitting the string into several entries
        /// and replacing in the list
        /// </summary>
        /// <param name="textToDraw">The list with strings to be searched</param>
        public static void RemoveCarriageReturns(ref List<string> text)
        {
            List<string> subLines;
            for (int i = 0; i < text.Count; i++)
            {
                subLines = RemoveCarriageReturns(text[i]);
                if (subLines.Count > 1)
                {
                    //Add the Array of strings
                    text.RemoveAt(i);
                    text.InsertRange(i, subLines);
                    i += subLines.Count - 1;
                }
            }
        }

        /// <summary>
        /// Remove carriage returns denoted by \n by splitting the string into several entries
        /// and replacing into a list
        /// </summary>
        /// <param name="textToDraw">The string to be searched</param>
        public static List<string> RemoveCarriageReturns(string text)
        {
            //The List into which to place the split lines
            List<string> brokenText = new List<string>();
            //Break the lines and add
            brokenText.AddRange(text.Split(new string[] { "\n" }, StringSplitOptions.None));
            return brokenText;
        }

        /// <summary>
        /// Remove carriage returns denoted by \n by splitting the string into several entries
        /// and replacing in the list
        /// </summary>
        /// <param name="textToDraw">The list with strings to be searched</param>
        /// <param name="font">The font to use to measure the string for the TextLines created</param>
        public static void RemoveCarriageReturns(ref List<TextLine> text, string font)
        {
            if (!fontDictionary.ContainsKey(font))
                font = currentFont;

            List<TextLine> subLines = new List<TextLine>();
            for (int i = 0; i < text.Count; i++)
            {
                subLines = RemoveCarriageReturns(text[i], fontDictionary[font]);

                //if the line was split
                if (subLines.Count > 1)
                {
                    //Remove the original line and replace with array of TextLines
                    text.RemoveAt(i);
                    text.InsertRange(i, subLines);
                    i += subLines.Count - 1;

                }
            }
        }

        /// <summary>
        /// Remove carriage returns denoted by \n by slitting the string into several entries
        /// and replacing into a list
        /// </summary>
        /// <param name="text">The string to be searched</param>
        /// <param name="font">The font to use to create the new textLines</param>
        public static List<TextLine> RemoveCarriageReturns(TextLine text, SpriteFont font)
        {
            //The List into which to place the split lines
            List<TextLine> brokenText = new List<TextLine>();
            //Break the lines and add
            string[] line = text.Line.Split(new string[] { "\n" }, StringSplitOptions.None);

            foreach (string subLine in line)
                brokenText.Add(new TextLine(subLine, font));
            return brokenText;
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks. Uses currentFont and 
        /// does not use text formatting
        /// </summary>
        /// <param name="Text">Text to break</param>
        /// <param name="MaxLineWidth">The Maximum line width</param>
        /// <returns>The resulting list of strings</returns>
        static public List<string> WrapText(string text, float maxLineWidth)
        {
            return WrapText(text, currentFont, maxLineWidth, false);
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks
        /// Does not use text formatting
        /// </summary>
        /// <param name="text">Text to break</param>
        /// <param name="fontName">Font to use</param>
        /// <param name="maxLineWidth">The Maximum line width</param>
        /// <returns>The Resulting list of strings</returns>
        static public List<string> WrapText(string text, string fontName, float maxLineWidth)
        {
            return WrapText(text, fontName, maxLineWidth, false);
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks. Uses currentFont
        /// </summary>
        /// <param name="Text">Text to break</param>
        /// <param name="MaxLineWidth">The Maximum line width</param>
        /// <returns></returns>
        static public List<string> WrapText(string text, float maxLineWidth, bool checkFormatting)
        {
            return WrapText(text, currentFont, maxLineWidth, checkFormatting);
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks
        /// </summary>
        /// <param name="text">Text to break</param>
        /// <param name="fontName">Font to use</param>
        /// <param name="maxLineWidth">The Maximum line width</param>
        /// <param name="maxLineWidth">Whether to use text formatting tags</param>
        /// <returns></returns>
        static public List<string> WrapText(string text, string fontName, float maxLineWidth, bool checkFormatting)
        {
            List<TextLine> wrappedTextLines;
            WrapText(text, fontName, maxLineWidth, out wrappedTextLines, checkFormatting);

            //convert list of text lines to list of strings
            return wrappedTextLines.ConvertAll<string>(new Converter<TextLine, string>(TextLine.GetString));
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks, without checking for 
        /// formatting markup
        /// </summary>
        /// <param name="text">Text to break</param>
        /// <param name="fontName">Font to use</param>
        /// <param name="maxLineWidth">The Maximum line width</param>
        /// <returns></returns>
        static public void WrapText(string text, string fontName, float maxLineWidth, out List<TextLine> outputText)
        {
            WrapText(text, fontName, maxLineWidth, out outputText, false);
        }

        /// <summary>
        /// Break a line of text into a series of lines joined by line breaks
        /// </summary>
        /// <param name="text">Text to break</param>
        /// <param name="fontName">Font to use</param>
        /// <param name="maxLineWidth">The Maximum line width</param>
        /// <returns></returns>
        static public void WrapText(string text, string fontName, float maxLineWidth, out List<TextLine> outputText, bool checkFormatting)
        {
            //Remove all the carriage returns from the given string by splitting and forming list
            List<string> fullText = RemoveCarriageReturns(text);
            outputText = new List<TextLine>();

            //cycle through each of our new lines and calculate if wrapping is required
            for (int i = 0; i < fullText.Count; i++)
            {

                //if we are checking formatting then get the codes from the string 
                //and process each section seperately
                List<FontFormattingElement> formattingElements;
                if (checkFormatting)
                    formattingElements = GetFormattingElements(fullText[i]);
                //if not then just add the current line to the list with
                else
                {
                    formattingElements = new List<FontFormattingElement>();
                    formattingElements.Add(new FontFormattingElement(FontFormattingType.None, fullText[i], "", ""));
                }

                //alist of strings (lines) for storing the temporary split result
                List<TextLine> lines = new List<TextLine>();

                // A StringBuilder lets us add to a string and finally return the result
                StringBuilder sb = new StringBuilder();

                // How long is the line we are currently working on so far
                float lineWidth = 0.0f;

                //need to keept track if we have added a word to the new line
                //cannot simply use sb.length as we may add formatting tags which shouldn;t be counted
                bool wordAddedToLine = false;

                //used to record the maximum height of a string when we are creating a Text line
                //updated to match the largest font used in the line
                float maxHeight = 0;

                //loop through all of the elements in our formattingElements list,
                foreach (FontFormattingElement section in formattingElements)
                {

                    // Create an array (words) with one entry for each word in the parsed text string
                    string[] words = section.Text.Split(new char[] { ' ' });

                    if (checkFormatting)
                    {
                        //if the current formatting section has a font tag then change the font
                        if (section.FormattingType == FontFormattingType.Font)
                            fontName = section.TagValue;

                        //write any tag to the string builder so the formatting is not removed by the process
                        sb.Append(section.FormattingElementString);
                    }
                    // Store a measurement of the size of a space in the font we are using.
                    float spaceWidth = fontDictionary[fontName].MeasureString(" ").X;

                    // Loop through each word in the string
                    foreach (string word in words)
                    {

                        Vector2 size = fontDictionary[fontName].MeasureString(word);
                        if (size.Y > maxHeight)
                            maxHeight = size.Y;

                        // If this word will fit on the current line, add it and keep track
                        // of how long the line has gotten.
                        if (lineWidth + spaceWidth + size.X < maxLineWidth)
                        {
                            //if this is not the first word on the line then add a space
                            if (wordAddedToLine)
                            {
                                lineWidth += spaceWidth;
                                sb.Append(" ");
                            }
                            sb.Append(word);
                            lineWidth += size.X;
                            wordAddedToLine = true;
                        }
                        else
                        // otherwise, add the new line to the list, create a new sb for the next line
                        // and calaculate the word width
                        {
                            lines.Add(new TextLine(sb.ToString(), new Vector2(lineWidth, maxHeight)));
                            sb = new StringBuilder();
                            sb.Append(word);
                            lineWidth = size.X;
                            maxHeight = size.Y;
                        }
                    }
                }
                //store the final line. If it is empty add a space to ensure correct spacing when drawing blank lines
                if (sb.Length == 0)
                {
                    sb.Append(" ");
                    Vector2 size = fontDictionary[fontName].MeasureString(" ");
                    maxHeight = size.Y;
                    lineWidth = size.X;
                }
                lines.Add(new TextLine(sb.ToString(), new Vector2(lineWidth, maxHeight)));

                //add lines to ouput List
                outputText.AddRange(lines);

            }
        }
    }

}
