using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that displays Game Over and Game Won when a game is won or lost
    /// </summary>
    class GameOverScreen : Screen
    {
        //variables for storing textures and strings
        string backgroundImage;
        Texture2D backgroundTexture;
        const string enterFont = "Narkisim Italic";
        const string loseFont = "Chiller";
        const string normalFont = "Narkisim";
        const string winFont = "Freestyle Script";
        Texture2D trophyTexture;

        //The text to display on game completion
        string gameWonText = "Congratulations!\n\n![F:" + normalFont + "]![#:FFFFFF]You have completed the final level of Trash";
        
        //how long to display the game over font
        int milisecondsToDisplayGameOver = 4000;

        //The winner
        PlayerDetails winner;

        /// <summary>
        /// The constructor for the game over screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public GameOverScreen(Game game, string backgroundImage, MusicType backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Draws the game over screen for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            Debug.Assert(winner != null);

            base.Draw(gameTime);

            TextDrawer.spriteBatch = SpriteBatch;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
            {
                if (player.gameState == GameState.GameOver || player.gameState == GameState.GameComplete)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                    Debug.Assert(player.gameType != GameType.None);
                    if (player.gameState == GameState.GameOver)
                    {
                        if (player.gameType == GameType.Solo)
                            TextDrawer.DrawTextCentered(loseFont, "Game Over", PlayingAreaConstants.BoardCentre, Color.Crimson);
                        else
                            if (player == winner)
                            {
                                TextDrawer.DrawTextCentered(winFont, "Winner", PlayingAreaConstants.BoardCentre, Color.Azure);
                                DrawTrophy();
                            }
                            else
                                TextDrawer.DrawTextCentered(loseFont, "Loser", PlayingAreaConstants.BoardCentre, Color.Crimson);
                    }
                    else
                    {
                        Debug.Assert(player.gameType == GameType.Solo);
                        TextDrawer.DrawText(winFont, gameWonText, PlayingAreaConstants.BoardCentre, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Middle, PlayingAreaConstants.Width, true,gameTime);
                    }
                    TextDrawer.DrawText(enterFont, "![Flash:ON]Press [" + player.inputHelper.enterKey.ToString() + "] to continue", PlayingAreaConstants.ExitLocation, Color.White, TextAlignment.Center, TextVerticalAlignment.Top,true,gameTime);
                    SpriteBatch.End();
                }

            }
        }

        /// <summary>
        /// Draws a trophy on the screen of the winner
        /// </summary>
        private void DrawTrophy()
        {
            Vector2 playingAreaCenter = new Vector2(
                PlayingAreaConstants.LeftEdge + PlayingAreaConstants.Width / 2,
                PlayingAreaConstants.TopEdge + PlayingAreaConstants.Height / 2);

            //Draw Medals just below center
            Vector2 position = new Vector2(playingAreaCenter.X - (trophyTexture.Width / 2), playingAreaCenter.Y + 30);
            SpriteBatch.Draw(trophyTexture, position, Color.White);
        }

        /// <summary>
        /// Load any background textures and fonts
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            trophyTexture = Game.Content.Load<Texture2D>(@"Textures/Trophy");
            backgroundTexture = Game.Content.Load<Texture2D>(backgroundImage);

        }

        /// <summary>
        /// Called to see if we should exit the gameover screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            //check to see if any of the players in game over have hit enter to continue 
            //or if their gameover timer has expired
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
            {
                if (player.gameState == GameState.GameOver || player.gameState == GameState.GameComplete)
                {
                    player.GameOverDisplayTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (player.inputHelper.IsEnterPressed() || player.GameOverDisplayTime > milisecondsToDisplayGameOver)
                        ((TrashGame)Game).ResetPlayer(player);
                }
            }

        }

        /// <summary>
        /// Set the current winner
        /// </summary>
        /// <param name="winner">The player who won</param>
        public void UpdateWinner(PlayerDetails winner)
        {
            this.winner = winner;
        }
    }
}
