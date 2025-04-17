using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that displays Level complete or round won depending if it is a solo or link game
    /// </summary>
    class LevelChangeScreen : Screen
    {
        //variables for storing textures and strings
        string backgroundImage;
        Texture2D backgroundTexture;
        string enterFont = "Narkisim Italic";
        string loseFont = "Chiller";
        Texture2D medalTexture;
        string winFont = "Freestyle Script";

        //the player who won
        PlayerDetails winner = null;

        /// <summary>
        /// The constructor for the level change screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public LevelChangeScreen(Game game, string backgroundImage, SoundEntry backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Draws the level change complete screen for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Debug.Assert(winner != null);
            Debug.Assert(winner.gameType != GameType.None);

            //set sprite batch to this screen's spritebatch
            TextDrawer.spriteBatch = SpriteBatch;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
            {
                if (player.gameState == GameState.LevelChange|| player.gameState == GameState.ReadyToContinue)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                    if (player.gameState == GameState.LevelChange)
                    {
                        if (player.gameType == GameType.Solo)
                            TextDrawer.DrawTextCentered(winFont, "Level Complete", PlayingAreaConstants.BoardCentre,
                                Color.CadetBlue);
                        else
                            if (player == winner)
                                TextDrawer.DrawTextCentered(winFont, "Round Won", PlayingAreaConstants.BoardCentre, Color.Azure);
                            else
                                TextDrawer.DrawTextCentered(loseFont, "Round Lost", PlayingAreaConstants.BoardCentre, Color.Crimson);
                        DrawMedals(player.wins);
                        TextDrawer.DrawText(enterFont, "![Flash:ON]Press [" + player.inputHelper.enterKey.ToString() + "] to continue", PlayingAreaConstants.ExitLocation, Color.White,TextAlignment.Center,TextVerticalAlignment.Top,true,gameTime);
                    }
                    SpriteBatch.End();
                }
            }
        }

        /// <summary>
        /// Draws a number of medals equal to the number of wins in the middle of the screen
        /// </summary>
        /// <param name="numMedals">The number of medals to draw</param>
        private void DrawMedals(int numMedals)
        {

            Vector2 playingAreaCenter = new Vector2(
                PlayingAreaConstants.LeftEdge + PlayingAreaConstants.Width / 2,
                PlayingAreaConstants.TopEdge + PlayingAreaConstants.Height / 2);

            //Draw Medals just below center
            Vector2 position = new Vector2(playingAreaCenter.X - (medalTexture.Width + 4) * numMedals /2f, playingAreaCenter.Y + 30);
            Debug.Assert(position.X > PlayingAreaConstants.LeftEdge);
            for (int i = 0; i < numMedals; i++)
            {
                SpriteBatch.Draw(medalTexture, position, Color.White);
                position += new Vector2(medalTexture.Width + 4, 0);
            }
        }

        /// <summary>
        /// Load any background textures and fonts
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            medalTexture = Game.Content.Load<Texture2D>(@"Textures/Medal");
            backgroundTexture = Game.Content.Load<Texture2D>(backgroundImage);
        }

        /// <summary>
        /// Called to check if a player has pressed continue
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.LevelChange)
                    if (player.inputHelper.IsEnterPressed())
                        ((TrashGame)Game).DoLevelChange(player);

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
