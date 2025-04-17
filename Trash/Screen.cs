
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Trash
{
    /// <summary>
    /// Screen represents a unit of rendering for the game, generally transitional point
    /// such as splash screens, selection screens and the actual game levels.
    /// </summary>
    public class Screen : DrawableGameComponent
    {

        private SoundEntry backgroundMusic;
        private Texture2D backgroundTexture;
        private ResizedSpriteBatch batch;
        private Cue cue;
        private bool isMusicPlaying;

        /// <summary>
        /// Gets the sprite batch used for this screen
        /// </summary>
        /// <value>The sprite batch for this screen</value>
        public ResizedSpriteBatch SpriteBatch
        {
            get
            {
                return batch;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game object</param>
        /// 
        /// <param name="backgroundMusic">The background music to play when this is 
        /// visible</param>
        public Screen(Game game, SoundEntry backgroundMusic)
            : base(game)
        {
            this.backgroundMusic = backgroundMusic;
        }

        /// <summary>
        /// Initializes the component.  Override to load any non-graphics resources and
        /// query for any required services.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            StartMusic();
        }

        /// <summary>
        /// Load any graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            //Re-Create the Sprite Batch!
            IGraphicsDeviceService graphicsService =
                Game.Services.GetService(typeof(IGraphicsDeviceService))
                as IGraphicsDeviceService;

            batch = new ResizedSpriteBatch(graphicsService.GraphicsDevice);

            base.LoadContent();
        }

        /// <summary>
        /// Called when the DrawableGameComponent.Visible property changes.  Raises the
        /// DrawableGameComponent.VisibleChanged event.
        /// </summary>
        /// <param name="sender">The DrawableGameComponent.</param>
        /// <param name="args">Arguments to the DrawableGameComponent.VisibleChanged 
        /// event.</param>
        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            base.OnVisibleChanged(sender, args);
            if (!Visible)
            {
                ShutdownMusic();
            }
            else
            {
                StartMusic();
            }

        }

        /// <summary>
        /// Tidies up the scene.
        /// </summary>
        public virtual void Shutdown()
        {
            ShutdownMusic();

            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }

        /// <summary>
        /// If music is playing, shut it down
        /// </summary>
        private void ShutdownMusic()
        {
            if (isMusicPlaying)
            {
                Sound.Stop(cue);
                isMusicPlaying = false;
            }
        }

        /// <summary>
        /// Start the background music
        /// </summary>
        private void StartMusic()
        {
            cue = Sound.Play(backgroundMusic);
            isMusicPlaying = true;
        }
    }
}
