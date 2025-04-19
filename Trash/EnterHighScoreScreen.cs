using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that allows players to enter their name when they achieve a new high score
    /// </summary>
    class EnterHighScoreScreen : Screen
    {
        //variables for storing textures and strings
        string backgroundImage;
        Texture2D backgroundTexture;
        Texture2D cursorTexture;
        string mainFont = "Narkisim";
        string subFont = "Narkisim_16";

        /// <summary>
        /// The constructor for the Enter high score screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public EnterHighScoreScreen(Game game, string backgroundImage, MusicType backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Change a letter of the name
        /// </summary>
        /// <param name="player">The player who's name to change</param>
        /// <param name="letterNum">The index of the letter to change (0-2)</param>
        /// <param name="direction">The direction to to move in the array of letters</param>
        private void ChangeLetter(PlayerDetails player, int letterNum, int direction)
        {
            //change the specified letter of the name
            player.Name[letterNum] = (player.Name[letterNum] + direction + TrashGame.HighScoreLetters.Length) % TrashGame.HighScoreLetters.Length;
        }

        /// <summary>
        /// Draws the enter high score for each required player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //Set the font spritebatch to use this screen's spritebatch
            TextDrawer.spriteBatch = SpriteBatch;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.EnterHighScore)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                    DrawText(player);
                    DrawCursor(player);
                    SpriteBatch.End();
                }
        }

        /// <summary>
        /// Draw the cursor on the screen
        /// </summary>
        /// <param name="player">The player who's screen to draw</param>
        private void DrawCursor(PlayerDetails player)
        {
            BoardSetupDetails board = player.boardSetupDetails;

            SpriteFont mainSpriteFont = TextDrawer.GetSpriteFont(mainFont);
            SpriteFont subSpriteFont = TextDrawer.GetSpriteFont(subFont);

            StringBuilder sb = new StringBuilder();
            sb.Append(TrashGame.HighScoreLetters[player.Name[0]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[1]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[2]]);
            sb.Append("ok");

            Vector2 wholeSize = mainSpriteFont.MeasureString(sb.ToString());
            Vector2 letter1Size = mainSpriteFont.MeasureString(TrashGame.HighScoreLetters[player.Name[0]]);
            Vector2 letter2Size = mainSpriteFont.MeasureString(TrashGame.HighScoreLetters[player.Name[1]]);
            Vector2 letter3Size = mainSpriteFont.MeasureString(TrashGame.HighScoreLetters[player.Name[2]]);
            Vector2 okSize = mainSpriteFont.MeasureString("ok");

            Rectangle areaToDraw = Rectangle.Empty;
            switch (board.cursorLocation)
            {
                case CursorLocation.OnLetter1:
                    areaToDraw = new Rectangle((int)(PlayingAreaConstants.HighScoreNameLocation.X - wholeSize.X/ 2),
                        (int)(PlayingAreaConstants.HighScoreNameLocation.Y),
                        (int)(letter1Size.X),
                        (int)(letter1Size.Y));
                    break;

                case CursorLocation.OnLetter2:
                    areaToDraw = new Rectangle((int)(PlayingAreaConstants.HighScoreNameLocation.X - wholeSize.X / 2 +letter1Size.X),
                        (int)(PlayingAreaConstants.HighScoreNameLocation.Y),
                        (int)(letter2Size.X),
                        (int)(letter2Size.Y));
                    break;
                case CursorLocation.OnLetter3:
                    areaToDraw = new Rectangle((int)(PlayingAreaConstants.HighScoreNameLocation.X - wholeSize.X / 2 + letter1Size.X+letter2Size.X),
                        (int)(PlayingAreaConstants.HighScoreNameLocation.Y),
                        (int)(letter3Size.X),
                        (int)(letter3Size.Y));
                    break;
                case CursorLocation.OnLettersOk:
                    areaToDraw = new Rectangle((int)(PlayingAreaConstants.HighScoreNameLocation.X - wholeSize.X / 2 + letter1Size.X+letter2Size.X+letter3Size.X),
                        (int)(PlayingAreaConstants.HighScoreNameLocation.Y),
                        (int)(okSize.X),
                        (int)(okSize.Y));
                    break;
            }



            Debug.Assert(!areaToDraw.Equals(Rectangle.Empty));
            areaToDraw.Inflate(2, 2);
            SpriteBatch.Draw(cursorTexture, areaToDraw, Color.White);
        }

        /// <summary>
        /// Draw the static text on the screen
        /// </summary>
        /// <param name="player">the player on who's screen to draw</param>
        private void DrawText(PlayerDetails player)
        {
            BoardSetupDetails board = player.boardSetupDetails;

            StringBuilder sb = new StringBuilder();
            sb.Append(TrashGame.HighScoreLetters[player.Name[0]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[1]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[2]]);
            sb.Append("ok");
            TextDrawer.DrawText(mainFont, "HIGH SCORE", PlayingAreaConstants.HighScoreLabelLocation, Color.White,
                TextAlignment.Center, TextVerticalAlignment.Top);
            TextDrawer.DrawText(subFont, player.score.ToString(), PlayingAreaConstants.HighScoreLocation, Color.Goldenrod,
                TextAlignment.Center, TextVerticalAlignment.Top);
            TextDrawer.DrawText(mainFont, sb.ToString(), PlayingAreaConstants.HighScoreNameLocation, Color.White,
               TextAlignment.Center, TextVerticalAlignment.Top);
        }

        /// <summary>
        /// called after player presses ok. Updates high scores and attempts to save them to the xml file
        /// </summary>
        /// <param name="player"></param>
        private void EnterNameComplete(PlayerDetails player)
        {
            //add the highscore to the list 
            StringBuilder sb = new StringBuilder();
            sb.Append(TrashGame.HighScoreLetters[player.Name[0]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[1]]);
            sb.Append(TrashGame.HighScoreLetters[player.Name[2]]);

            TrashGame.HighScores.Add(new HighScoreEntry(sb.ToString(), player.score));

            //sort the list in descending order
            TrashGame.HighScores.Sort(new DescendingComparer<HighScoreEntry>());

            //remove the last entry in the high score table that has been pushed off the end
            TrashGame.HighScores.RemoveAt(TrashGame.HighScores.Count - 1);

            //save the highscores to file
            ((TrashGame)Game).SaveHighScores();

            //mark the highscores as having been changed
            TrashGame.highScoresChanged = true;

            //change the state of the player to scroll the credits
            player.gameState = GameState.Credits;
        }

        /// <summary>
        /// Load any background textures and fonts
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            cursorTexture = Game.Content.Load<Texture2D>(@"Textures/cursor");
            backgroundTexture = Game.Content.Load<Texture2D>(backgroundImage);
        }

        /// <summary>
        /// Called to update the screen when the game is in enter highscores
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //move cursor if inputHelper indicates it is required for each player.
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.EnterHighScore)
                    UpdateCursor(player);
        }

        /// <summary>
        /// Upate the position of the cursor if the player has moved it
        /// </summary>
        /// <param name="player">The player who's cursor to move</param>
        private void UpdateCursor(PlayerDetails player)
        {
            BoardSetupDetails board = player.boardSetupDetails;
            InputHelper helper = player.inputHelper;

            Debug.Assert(board.selectionScreen == SelectionScreen.EnterHighScore);

            switch (board.cursorLocation)
            {
                case CursorLocation.OnLetter1:
                    if (helper.IsUpPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 0, 1);
                    }
                    else if (helper.IsDownPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 0, -1);
                    }
                    else if (helper.IsRightPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLetter2;
                    }
                    break;
                case CursorLocation.OnLetter2:
                    if (helper.IsUpPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 1, 1);
                    }
                    else if (helper.IsDownPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 1, -1);
                    }
                    else if (helper.IsRightPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLetter3;
                    }
                    else if (helper.IsLeftPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLetter1;
                    }
                    break;
                case CursorLocation.OnLetter3:
                    if (helper.IsUpPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 2, 1);
                    }
                    else if (helper.IsDownPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        ChangeLetter(player, 2, -1);
                    }
                    else if (helper.IsRightPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLettersOk;
                    }
                    else if (helper.IsLeftPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLetter2;
                    }
                    break;
                case CursorLocation.OnLettersOk:
                    if (helper.IsEnterPressed())
                    {
                        Sound.Play(SoundEffectType.StartGame);
                        EnterNameComplete(player);
                    }
                    else if (helper.IsLeftPressed())
                    {
                        Sound.Play(SoundEffectType.Navigate);
                        board.cursorLocation = CursorLocation.OnLetter3;
                    }
                    break;
            }

        }
    }

}
