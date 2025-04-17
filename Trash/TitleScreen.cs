using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Trash
{
    /// <summary>
    /// A simple splash screen that simply displays a texture
    /// </summary>
    class TitleScreen : Screen
    {
        //The texture to use for the title overlay
        Texture2D titleTexture;
        string titleTextureString;


        /// <summary>
        /// The constructor for the title screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public TitleScreen(Game game, string backgroundImage, SoundEntry backgroundMusic)
            : base(game, backgroundMusic)
        {
            titleTextureString = backgroundImage;
        }

        /// <summary>
        /// Load the background texture
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            titleTexture = Game.Content.Load<Texture2D>(titleTextureString);
        }

        /// <summary>
        /// The call to draw the title screen
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            
            //Add 'Trash' Title overlay
            SpriteBatch.GraphicsDevice.Viewport = TrashGame.DefaultViewport;
            SpriteBatch.Begin();
            SpriteBatch.Draw(titleTexture, Vector2.Zero, Color.White);
            SpriteBatch.End();
        }

        /// <summary>
        /// Update the title screen - check for a key press and enter main game if so
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            bool inMainGame = false;
            //check to see if any has been pressed, clear any enters
            //and if any have, change the state of all users
            if (Keyboard.GetState().GetPressedKeys().Length > 0)
                inMainGame = true;

            if (inMainGame)
            {
                foreach (PlayerDetails player in ((TrashGame)Game).Players)
                    player.inputHelper.IgnoreSingleEnterPress();
                ((TrashGame)Game).EnterMainGame();
            }
        }
    }
}
