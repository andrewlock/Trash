using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that overlays score, level, difficulty and wins for each player
    /// </summary>
    class PlayerScoreScreen : Screen
    {
        //variables for storing textures and strings
        Texture2D medalTexture;
        string scoreFont = "Narkisim";

        /// <summary>
        /// The constructor for the player score screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public PlayerScoreScreen(Game game, string backgroundImage, SoundEntry backgroundMusic)
            : base(game, backgroundMusic)
        {
        }

        /// <summary>
        /// Draws the scoreboards for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //set the spritefont used by the screen 
            TextDrawer.spriteBatch = SpriteBatch;
            //draw each board if required
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState != GameState.None)
                {
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Begin();
                    DrawLevel(player.boardSetupDetails.currentLevel + 1);
                    DrawDifficulty(TrashGame.difficultyInfoList.levels[player.boardSetupDetails.currentDifficulty].name);
                    DrawScore(player.score);
                    DrawMedals(player.wins);
                    SpriteBatch.End();
                }

        }

        /// <summary>
        /// Draw the difficulty leve
        /// </summary>
        /// <param name="difficulty">the level to draw</param>
        private void DrawDifficulty(string difficulty)
        {
            TextDrawer.DrawText(scoreFont, difficulty, PlayingAreaConstants.DifficultyLocation, Color.GhostWhite,
                TextAlignment.Center,TextVerticalAlignment.Top);
        }

        /// <summary>
        /// Draw the level
        /// </summary>
        /// <param name="level">the level to use</param>
        private void DrawLevel(int level)
        {
            TextDrawer.DrawText(scoreFont, level.ToString(), PlayingAreaConstants.LevelLocation, Color.GhostWhite,
                TextAlignment.Center,TextVerticalAlignment.Top);
        }

        /// <summary>
        /// Draw the current number of wins
        /// </summary>
        /// <param name="numMedals">the number of wins to draw</param>
        private void DrawMedals(int numMedals)
        {
            //Draw Medals just below center, shrunken slightly
            Vector2 position = PlayingAreaConstants.WinsLocation -
                new Vector2((medalTexture.Width*0.8f + 4) * (numMedals) / 2f, 0);
            for (int i = 0; i < numMedals; i++)
            {
                SpriteBatch.Draw(medalTexture, new Rectangle((int)position.X, (int)position.Y, (int)(medalTexture.Width*0.8), (int)(medalTexture.Height*0.8)), Color.White);
                position += new Vector2(medalTexture.Width*0.8f + 4, 0);
            }
        }

        /// <summary>
        /// Draw the current score
        /// </summary>
        /// <param name="score">the score to draw</param>
        private void DrawScore(int score)
        {
            TextDrawer.DrawText(scoreFont, score.ToString(), PlayingAreaConstants.ScoreLocation, Color.GhostWhite,
                TextAlignment.Center,TextVerticalAlignment.Top);
        }

        /// <summary>
        /// Load any background textures and fonts
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            medalTexture = Game.Content.Load<Texture2D>(@"Textures/Medal");
        }
    }
}
