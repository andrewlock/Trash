using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that displays scrolling credits and highscores
    /// </summary>
    class CreditsScreen : Screen
    {
        //variables for storing textures and strings
        string backgroundImage;
        Texture2D backgroundTexture;
        Color color = Color.White;
        const string font = "Narkisim";
        const string italicFont = "Narkisim Italic";
        const string smallerFont = "Narkisim_16";

        //the text to use for the credits
        string text1 = "![F:" + italicFont + "]![#:DAA520]Welcome to...\n\n![#:FF0000]![F:TrashFont]Trash \n\n![F:" + italicFont + "]![#:FFFFFF]![Flash:ON]Push ";
        string text2 ="To Begin!\n\n\n![Flash:OFF]![F:" + smallerFont + "]Eradicate all the germs in the bottle using the pills. "
            + "Form lines of 4 or more pills and germs to remove them. Unsupported pills will drop down. "
            + "To get the biggest scores, use these to clear multiple lines using a single pill. In multiplayer mode "
            + "this will also drop trash on your opponent!\n\n\n\n";
        string aboutText = "![F:" + italicFont + "]![#:6495ED]Developed by Andrew Lock using XNA Game Studio 3.1 and Visual Studio 2008 Pro.\n\n"
            +"Music: Joe Satriani - One Big Rush, transcribed by Michael Corbel\n";
        string highScoresText = "";
        
        //A dictionary of players and Scrolling fonts to allow different 
        //text scroll positions for each player
        Dictionary<PlayerDetails, ScrollingFont> creditsDictionary = new Dictionary<PlayerDetails, ScrollingFont>();

        /// <summary>
        /// The constructor for the credits screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public CreditsScreen(Game game, string backgroundImage, MusicType backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Create a new ScrollingFont for the given player using the current highscores
        /// </summary>
        /// <param name="player">The player to create the credits for</param>
        public void CreateCreditsScrollingFont(PlayerDetails player)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(text1);
            sb.Append("[" + player.inputHelper.enterKey.ToString() + "]");
            sb.Append(text2);
            sb.Append(highScoresText);
            sb.Append("![F:" + font + "]![#:DAA520]CONTROLS\n\n![F:" + smallerFont +"]![#:FFFFFF]");
            sb.Append("[" + player.inputHelper.leftKey.ToString() + "] - Left\n");
            sb.Append("[" + player.inputHelper.rightKey.ToString() + "] - Right\n");
            sb.Append("[" + player.inputHelper.upKey.ToString() + "] - Rotate AntiClockwise\n");
            sb.Append("[" + player.inputHelper.downKey.ToString() + "] - Rotate Clockwise\n");
            sb.Append("[" + player.inputHelper.enterKey.ToString() + "] - Drop\n");
            sb.Append("[" + player.inputHelper.pauseKey.ToString() + "] - Pause Game\n");
            sb.Append("[" + player.inputHelper.backKey.ToString() + "] - Quit Game\n\n\n");

            sb.Append(aboutText);

            //Create a scrolling text that smoothly exists the board area but requires seperate
            //Spritebatch Begin- end pair
            creditsDictionary.Add(player,
                new ScrollingFont(font, sb.ToString(), color, PlayingAreaConstants.BoardArea,
                    ScrollDirection.Up, true, PlayingAreaConstants.Width, true, 0, PlayingAreaConstants.Height/2, 
                    TextAlignment.Center, TextVerticalAlignment.Top,true,true));
        }

        /// <summary>
        /// Draws the credits screen for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            TextDrawer.spriteBatch = SpriteBatch;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.Credits)
                {
                    if (creditsDictionary.ContainsKey(player))
                    {
                        SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                        SpriteBatch.Begin();
                        SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                        SpriteBatch.End();
                        creditsDictionary[player].Draw(gameTime);
                    }
                }

        }

        /// <summary>
        /// Initializes the game screen and the game board for each player
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            backgroundTexture = Game.Content.Load<Texture2D>(backgroundImage);
        }

        /// <summary>
        /// Called to update the screen when the game is in credits
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //if the high scores need updating then do so
            if (TrashGame.highScoresChanged)
                UpdateHighScores();

            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.Credits)

                    // check to see if the player has pressed enter to begin
                    if (player.inputHelper.IsEnterPressed())
                        ((TrashGame)Game).PlayerSelectSettings(player);
                    else
                        if (creditsDictionary.ContainsKey(player))
                            creditsDictionary[player].Update(gameTime);
                        else
                            CreateCreditsScrollingFont(player);
        }

        /// <summary>
        /// Clears the creditsdictionary entries to allow new ones with update high scores to be created
        /// </summary>
        private void UpdateHighScores()
        {
            //clear the credits entries for each player so will be recreated with scrolling font
            creditsDictionary.Clear();
            UpdateHighScoresText();
            TrashGame.highScoresChanged = false;

        }

        /// <summary>
        /// Modifies the Highscores text to represent the new high scores
        /// </summary>
        private void UpdateHighScoresText()
        {
            //add the current high scores to the credits text
            StringBuilder sb = new StringBuilder();
            sb.Append("![F:" + font + "]![#:DAA520]HIGHSCORES\n\n![F:" + smallerFont + "]![#:FFFFFF]");
            for (int i = 0; i < TrashGame.HighScores.Count; i++)
            {
                sb.Append((i + 1).ToString() + ": " + TrashGame.HighScores[i].Name + " - " + TrashGame.HighScores[i].Score + "\n");
            }
            sb.Append("\n\n\n");
            highScoresText = sb.ToString();
        }
    }

}
