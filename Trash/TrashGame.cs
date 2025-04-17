using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Trash
{
    /// <summary>
    /// The main type for the Game
    /// </summary>
    public class TrashGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        //variables for managing highscores
        public static List<HighScoreEntry> HighScores;
        public static bool highScoresChanged = true;
        string highscoresPath = "highscores.xml";
        public static string[] HighScoreLetters = new string[]{
        "_","A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
        " ","0","1","2","3","4","5","6","7","8","9","-","+","=","!","$","%","^","&","*","(",")","@"};

        //Screens representing various game states
        GameScreen mainGameScreen;
        TitleScreen titleScreen;
        CreditsScreen creditsScreen;
        SettingsScreen settingsScreen;
        LevelChangeScreen levelChangeScreen;
        GameOverScreen gameOverScreen;
        PlayerScoreScreen playerScoreScreen;
        EnterHighScoreScreen enterHighScoreScreen;

        //The possible colours for pills
        public static Color[] pillColours = new Color[] { Color.Red, Color.Blue, Color.Yellow };

        //The level and difficulty settings 
        public static LevelSettings levelInfoList = new LevelSettings();
        public static DifficultySettings difficultyInfoList = new DifficultySettings();

        //Random number generator used by all componenets to ensure thread safe
        public Random rnd = new Random();

        //the default viewport for the screen, as oposed to the viewports used by each player
        public static Viewport DefaultViewport { get; private set; }
        Cue musicCue;

        /// <summary>
        /// List of all the PlayerDetails objects in the game, representing discrete players
        /// </summary>
        public List<PlayerDetails> Players { get; private set; }

        //The screen size upon which all coordinates are based
        const int GameFullHeight = 720;
        const int GameFullWidth = 1280;

        /// <summary>
        /// The SpriteBatch which handles resizing the screen for resolutions as required
        /// </summary>
        public ResizedSpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public TrashGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            bool useFullScreen = false;
#if !DEBUG
            useFullScreen = true;
#endif

            //initialise the graphics mode depending on available screen resolutions
            if (!InitGraphicsMode(1280, 720, useFullScreen))
                if (!InitGraphicsMode(1024, 768, useFullScreen))
                    if (!InitGraphicsMode(800, 600, useFullScreen))
                        if (!InitGraphicsMode(640, 480, useFullScreen))
                            //no resolutions supported
                            this.Exit();
        }

        /// <summary>
        /// Called by the level change screen after continue is pressed to start a new level or to end the game
        /// </summary>
        /// <param name="player">The player who pressed continue</param>
        public void DoLevelChange(PlayerDetails player)
        {
            mainGameScreen.Continue(player);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //components will handle all of the drawing
            base.Draw(gameTime);
        }

        /// <summary>
        /// Called when the title screen exists to start the main game
        /// </summary>
        public void EnterMainGame()
        {
            //start game music
            musicCue = Sound.Play(SoundEntry.MusicGame);
            //disable title screen
            titleScreen.Enabled = false;
            titleScreen.Visible = false;

            //enable all other screens - only when a player is in an approriate
            //game state will the screens update or draw anything
            creditsScreen.Enabled = true;
            creditsScreen.Visible = true;
            enterHighScoreScreen.Enabled = true;
            enterHighScoreScreen.Visible = true;
            playerScoreScreen.Enabled = true;
            playerScoreScreen.Visible = true;
            mainGameScreen.Enabled=true;
            mainGameScreen.Visible = true;
            settingsScreen.Enabled = true;
            settingsScreen.Visible = true;
            //Do not enable as they require a winner, will get activated when required
            //levelChangeScreen.Enabled = true;
            //levelChangeScreen.Visible = true;
            //gameOverScreen.Enabled = true;
            //gameOverScreen.Visible = true;

            foreach (PlayerDetails player in Players)
                player.gameState = GameState.Credits;
        }

        /// <summary>
        /// Called when a game is over for some reason
        /// </summary>
        /// <param name="player">The player who won in link play, or lost in solo play</param>
        public void GameOver(PlayerDetails player)
        {
            foreach (PlayerDetails _player in Players)
            {
                if (_player.gameState == GameState.GameOver)
                {
                    player.inputHelper.allowMultipleEnterPresses = false;
                    //clear any previous enter presses                    
                    player.inputHelper.IgnoreSingleEnterPress();
                    //reset the players game over screen timer
                    player.GameOverDisplayTime = 0;
                    player.isExiting = false;
                    player.isPaused = false;
                    player.gameBoard.trashRequired = false;
                }
            }
            gameOverScreen.UpdateWinner(player);
            gameOverScreen.Visible = true;
            gameOverScreen.Enabled = true;
        }

        /// <summary>
        /// Called to try and set the prefferedBackBuffer size of the graphics device
        /// </summary>
        /// <param name="iWidth">The width to try</param>
        /// <param name="iHeight">the height to try</param>
        /// <param name="bFullScreen">Whether to use fullscreen</param>
        /// <returns>True if the size changes were applied ok, false if they could not be</returns>
        private bool InitGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphics.PreferredBackBufferWidth = iWidth;
                    graphics.PreferredBackBufferHeight = iHeight;
                    graphics.IsFullScreen = bFullScreen;
                    graphics.ApplyChanges();
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set. To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphics.PreferredBackBufferWidth = iWidth;
                        graphics.PreferredBackBufferHeight = iHeight;
                        graphics.IsFullScreen = bFullScreen;
                        graphics.ApplyChanges();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            creditsScreen = new CreditsScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(creditsScreen);
            creditsScreen.Visible = false;
            creditsScreen.Enabled = false;

            settingsScreen = new SettingsScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(settingsScreen);
            settingsScreen.Visible = false;
            settingsScreen.Enabled = false;

            mainGameScreen = new GameScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(mainGameScreen);
            mainGameScreen.Visible = false;
            mainGameScreen.Enabled = false;

            levelChangeScreen = new LevelChangeScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(levelChangeScreen);
            levelChangeScreen.Visible = false;
            levelChangeScreen.Enabled = false;

            gameOverScreen = new GameOverScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(gameOverScreen);
            gameOverScreen.Visible = false;
            gameOverScreen.Enabled = false;

            enterHighScoreScreen = new EnterHighScoreScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(enterHighScoreScreen);
            enterHighScoreScreen.Visible = false;
            enterHighScoreScreen.Enabled = false;

            playerScoreScreen = new PlayerScoreScreen(this, @"Textures/play_screen", SoundEntry.NoSound);
            Components.Add(playerScoreScreen);
            gameOverScreen.Visible = false;
            gameOverScreen.Enabled = false;

            titleScreen = new TitleScreen(this, @"Textures/title_screen", SoundEntry.MusicTitle);
            Components.Add(titleScreen);
            titleScreen.Visible = true;
            titleScreen.Enabled = true;

            //create the player list - must add the viewport in LoadContent after Graphics context exists
            Players = new List<PlayerDetails>();
            InputHelper helper = new InputHelper(this, Keys.W, Keys.S, Keys.A, Keys.D, Keys.Space, Keys.Escape, Keys.F1);
            Components.Add(helper);

            Players.Add(new PlayerDetails(helper, new GameBoard(this, mainGameScreen), new BoardSetupDetails()));

            helper = new InputHelper(this, Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Enter, Keys.Back, Keys.P);
            Components.Add(helper);
            Players.Add(new PlayerDetails(helper, new GameBoard(this, mainGameScreen), new BoardSetupDetails()));

            Sound.Initialize();
            base.Initialize();

            LoadHighScores();
        }

        /// <summary>
        /// Called when a player completes a level
        /// </summary>
        /// <param name="player">The player who completed the level</param>
        public void LevelComplete(PlayerDetails player)
        {
            foreach (PlayerDetails _player in Players)
            {
                if (_player.gameState == GameState.LevelChange)
                {
                    player.inputHelper.allowMultipleEnterPresses = false;
                    //clear any previous enter presses                    
                    player.inputHelper.IgnoreSingleEnterPress();
                    player.isExiting = false;
                    player.isPaused = false;
                    player.gameBoard.trashRequired = false;

                }
            }
            Sound.Play(SoundEntry.MusicBoardCleared);
            levelChangeScreen.UpdateWinner(player);
            levelChangeScreen.Visible = true;
            levelChangeScreen.Enabled = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures, with the required screen size set
            SpriteBatch = new ResizedSpriteBatch(GraphicsDevice, GameFullHeight, GameFullWidth);

            //reset the playing board position values
            PlayingAreaConstants.Resize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
                GameFullWidth, GameFullHeight);

            //initialise the static font class to handle drawing sttrings on screen
            TextDrawer.Initialize(SpriteBatch, Content, "Fonts");

            //need to have a graphics context to set the viewports so cannot do this in initialize
            //calculate required viewports
            DefaultViewport = SpriteBatch.GraphicsDevice.Viewport;
            Viewport leftViewport, rightViewport;
            leftViewport = DefaultViewport;
            rightViewport = DefaultViewport;
            leftViewport.Width /= 2;
            rightViewport.Width /= 2;
            rightViewport.X = leftViewport.Width + 1;

            Players[0].viewport = leftViewport;
            Players[1].viewport = rightViewport;
        }

        /// <summary>
        /// Attempt to load the high scores from the highScoresFile, if  can't then load defaults
        /// </summary>
        private void LoadHighScores()
        {
            highScoresChanged = false;
            try
            {
                using (FileStream file =
                    File.Open(highscoresPath, FileMode.Open))
                {
                    try
                    {
                        XmlSerializer serializer =
                            new XmlSerializer(typeof(List<HighScoreEntry>));
                        HighScores = (List<HighScoreEntry>)serializer.Deserialize(file);
                        highScoresChanged = true;
                    }
                    catch (Exception)
                    { }
                    finally
                    {
                        file.Close();
                    }
                }
            }
            //if can't load highscores for whatever reason, create defaults
            catch (Exception) { }
            finally
            {
                if (!highScoresChanged)
                {
                    HighScores = new List<HighScoreEntry>();
                    for (int i = 10; i > 0; i--)
                        HighScores.Add(new HighScoreEntry("AAA", i * 100));
                    highScoresChanged = true;
                }
            }

        }

        /// <summary>
        /// Called after a player has pressed enter from the credits screen to start a new game
        /// </summary>
        /// <param name="player">The player to modify</param>
        public void PlayerSelectSettings(PlayerDetails player)
        {
            player.gameState = GameState.SelectSettings;
            player.boardSetupDetails.cursorLocation = CursorLocation.OnSolo;
            player.boardSetupDetails.selectionScreen = SelectionScreen.PlayerNumbers;
            //enable the select settings screen
            settingsScreen.Enabled = true;
            settingsScreen.Visible = true;
        }

        /// <summary>
        /// Called after a player has finished the game or reached game over
        /// Checks to see if the player got a highscore and if not 
        /// Resets the player back to the credits screen
        /// </summary>
        /// <param name="player">The Player to reset</param>
        public void ResetPlayer(PlayerDetails player)
        {

            //test to see if we got a high score
            bool highScoreAchieved = false;

            for (int i = HighScores.Count - 1; i >= 0; i--)
                if (player.score > HighScores[i].Score)
                {
                    highScoreAchieved = true;
                    break;
                }

            if (highScoreAchieved)
            {
                player.boardSetupDetails.cursorLocation = CursorLocation.OnLetter1;
                player.boardSetupDetails.selectionScreen = SelectionScreen.EnterHighScore;
                player.gameState = GameState.EnterHighScore;
            }
            else
                player.gameState = GameState.Credits;
        }

        /// <summary>
        /// Saves the current high scores to disk
        /// </summary>
        public void SaveHighScores()
        {
            bool serialisedOk = false;
            try
            {
                //if a file exists create a copy to restore on failure
                if (File.Exists(highscoresPath))
                {
                    if (File.Exists(Path.ChangeExtension(highscoresPath, ".bak")))
                        File.Delete(Path.ChangeExtension(highscoresPath, ".bak"));
                    File.Move(highscoresPath, Path.ChangeExtension(highscoresPath, ".bak"));
                }
                try
                {
                    using (FileStream file = File.Create(highscoresPath))
                    {
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<HighScoreEntry>));
                            serializer.Serialize(file, HighScores);
                            file.Close();
                            serialisedOk = true;
                        }
                        catch (Exception)
                        {
                            file.Close();
                            File.Delete(highscoresPath);

                        }
                    }
                }
                catch (Exception) { Debugger.Break(); }
                finally
                {
                    if (!serialisedOk)
                    {
                        //if we didn't create the file ok then restore the original
                        if (File.Exists(highscoresPath))
                            File.Delete(highscoresPath);
                        File.Move(Path.ChangeExtension(highscoresPath, ".bak"), highscoresPath);
                    }
                }
            }
            catch (Exception)
            {
                //we can't move files or delete our newly creted file
                Debugger.Break();
            }
        }

        /// <summary>
        /// Called to begin a game
        /// </summary>
        public void StartGame()
        {
            //still may have players who are selecting their settings so don't disable started screen(it will
            //only draw those players necessary anyway)            
            mainGameScreen.Enabled = true;
            mainGameScreen.Visible = true;

            //set the helper to record multiple enter presses
            foreach (PlayerDetails player in Players)
                if (player.gameState == GameState.Started)
                    player.inputHelper.allowMultipleEnterPresses = true;

            Sound.Play(SoundEntry.StartGame);
            mainGameScreen.NewGame();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Sound.Shutdown();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //The screen components handle updating themselves
            base.Update(gameTime);
        }
    }
}
