using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen that allows players to select the level and difficulty and start a new game
    /// </summary>
    class SettingsScreen : Screen
    {
        //variables for storing textures and strings
        bool allowLinkPlayStart = false;
        string backgroundImage;
        Texture2D backgroundTexture;
        Texture2D cursorTexture;
        string mainFont = "Narkisim";
        string subFont = "Narkisim Italic";

        /// <summary>
        /// The constructor for the Settings screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public SettingsScreen(Game game, string backgroundImage, MusicType backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Check to see if we are allowed to start a link game 
        /// There must be more than one player on the multiplayer game screen
        /// </summary>
        private void CheckAllowedLinkPlayStart()
        {
            //check if we are allowed to start a link game,
            int numLinkSelected = 0;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameType == GameType.Link)
                    //not allowed to start another link play game when one is already in progress
                    if (player.gameState == GameState.InPlay)
                    {
                        allowLinkPlayStart = false;
                        return;
                    }
                    else
                        numLinkSelected++;

            //if more than one player selected for link play then allowed to start a link game
            if (numLinkSelected > 1)
                allowLinkPlayStart = true;
            else
                allowLinkPlayStart = false;
        }

        /// <summary>
        /// Draws the settings scree for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //Set the font spritebatch to use this screen's spritebatch
            TextDrawer.spriteBatch = SpriteBatch;
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.SelectSettings)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                    DrawCursor(player, allowLinkPlayStart);
                    DrawText(player, allowLinkPlayStart);
                    SpriteBatch.End();
                }

        }

        /// <summary>
        /// Draw the cursor
        /// </summary>
        /// <param name="player">The player to draw</param>
        /// <param name="allowLinkPlayStart">Whether the player is allowed to start a lnk game</param>
        private void DrawCursor(PlayerDetails player, bool allowLinkPlayStart)
        {
            BoardSetupDetails board = player.boardSetupDetails;

            if (!player.isExiting)
            {
                Color colorToDraw = Color.White;
                SpriteFont mainSpriteFont = TextDrawer.GetSpriteFont(mainFont);
                SpriteFont subSpriteFont = TextDrawer.GetSpriteFont(subFont);

                Rectangle areaToDraw = Rectangle.Empty;
                switch (board.selectionScreen)
                {
                    case SelectionScreen.PlayerNumbers:
                        switch (board.cursorLocation)
                        {
                            case CursorLocation.OnSolo:

                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.P1Location.X - mainSpriteFont.MeasureString("1 Player Game").X / 2),
                                    (int)(PlayingAreaConstants.P1Location.Y),
                                    (int)(mainSpriteFont.MeasureString("1 Player Game").X),
                                    (int)(mainSpriteFont.MeasureString("1 Player Game").Y));
                                break;

                            case CursorLocation.OnLink:
                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.P2Location.X - mainSpriteFont.MeasureString("Multiplayer Game").X / 2),
                                    (int)(PlayingAreaConstants.P2Location.Y),
                                    (int)(mainSpriteFont.MeasureString("Multiplayer Game").X),
                                    (int)(mainSpriteFont.MeasureString("Multiplayer Game").Y));
                                break;

                            case CursorLocation.OnExit:
                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.ExitLocation.X - mainSpriteFont.MeasureString("Quit Game").X / 2),
                                    (int)(PlayingAreaConstants.ExitLocation.Y),
                                    (int)(mainSpriteFont.MeasureString("Quit Game").X),
                                    (int)(mainSpriteFont.MeasureString("Quit Game").Y));
                                break;
                        }
                        break;
                    case SelectionScreen.LevelAndDifficulty:
                        switch (board.cursorLocation)
                        {
                            case CursorLocation.OnLevel:
                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.LevelChangeLocation.X - subSpriteFont.MeasureString((board.currentLevel+1).ToString()).X / 2),
                                    (int)(PlayingAreaConstants.LevelChangeLocation.Y),
                                    (int)(subSpriteFont.MeasureString((board.currentLevel+1).ToString()).X),
                                    (int)(subSpriteFont.MeasureString((board.currentLevel+1).ToString()).Y));
                                break;

                            case CursorLocation.OnDifficulty:
                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.DifficultyChangeLocation.X - subSpriteFont.MeasureString(TrashGame.difficultyInfoList.levels[board.currentDifficulty].name).X / 2),
                                    (int)(PlayingAreaConstants.DifficultyChangeLocation.Y),
                                    (int)(subSpriteFont.MeasureString(TrashGame.difficultyInfoList.levels[board.currentDifficulty].name).X),
                                    (int)(subSpriteFont.MeasureString(TrashGame.difficultyInfoList.levels[board.currentDifficulty].name).Y));
                                break;

                            case CursorLocation.OnStart:
                                areaToDraw = new Rectangle((int)(PlayingAreaConstants.StartLocation.X - mainSpriteFont.MeasureString("Start").X / 2),
                                    (int)(PlayingAreaConstants.StartLocation.Y),
                                    (int)(mainSpriteFont.MeasureString("Start").X),
                                    (int)(mainSpriteFont.MeasureString("Start").Y));
                                if (player.gameType == GameType.Link && !allowLinkPlayStart)
                                    colorToDraw = Color.LightGray;
                                break;
                        }
                        break;

                }
                Debug.Assert(!areaToDraw.Equals(Rectangle.Empty));
                areaToDraw.Inflate(5, 5);
                SpriteBatch.Draw(cursorTexture, areaToDraw, colorToDraw);
            }
        }

        /// <summary>
        /// Draw the text options
        /// </summary>
        /// <param name="player">The player to draw</param>
        /// <param name="allowLinkPlayStart">Whether allowed to start a link game. If not grey the Start option</param>
        private void DrawText(PlayerDetails player, bool allowLinkPlayStart)
        {
            BoardSetupDetails board = player.boardSetupDetails;

            //if we are exiting just draw the confirm exit text
            if (player.isExiting)
            {
                //Write text to indicate status in center of the screen
                TextDrawer.DrawText(mainFont, "EXIT GAME?\n\n\nPress [" + player.inputHelper.enterKey.ToString() + "] to Quit the application"
                    + "\n\nOr Press [" + player.inputHelper.backKey.ToString() + "] to Resume",
                    PlayingAreaConstants.BoardCentre, Color.White,
                    TextAlignment.Center, TextVerticalAlignment.Middle,  PlayingAreaConstants.Width);
            }
            else
            {
                switch (board.selectionScreen)
                {
                    case SelectionScreen.PlayerNumbers:
                        TextDrawer.DrawText(mainFont, "1 Player Game", PlayingAreaConstants.P1Location, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        TextDrawer.DrawText(mainFont, "Multiplayer Game", PlayingAreaConstants.P2Location, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        TextDrawer.DrawText(mainFont, "Quit Game", PlayingAreaConstants.ExitLocation, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        break;
                    case SelectionScreen.LevelAndDifficulty:
                        TextDrawer.DrawText(mainFont, "Level", PlayingAreaConstants.LevelTextLocation, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        TextDrawer.DrawText(subFont, (board.currentLevel +1).ToString(), PlayingAreaConstants.LevelChangeLocation, Color.White,
                            TextAlignment.Center, TextVerticalAlignment.Top);

                        TextDrawer.DrawText(mainFont, "Difficulty", PlayingAreaConstants.DifficultyTextLocation, Color.Goldenrod,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        TextDrawer.DrawText(subFont, TrashGame.difficultyInfoList.levels[board.currentDifficulty].name, PlayingAreaConstants.DifficultyChangeLocation, Color.White,
                            TextAlignment.Center, TextVerticalAlignment.Top);

                        Color startColor;
                        if (player.gameType == GameType.Solo || allowLinkPlayStart)
                            startColor = Color.Goldenrod;
                        else
                            startColor = Color.LightGray;

                        TextDrawer.DrawText(mainFont, "Start", PlayingAreaConstants.StartLocation, startColor,
                            TextAlignment.Center, TextVerticalAlignment.Top);
                        break;
                }
            }
        }

        /// <summary>
        /// Called when a player presses back to go to the player select screen
        /// </summary>
        /// <param name="player">The player who pressed back</param>
        private void GoToTopScreen(PlayerDetails player)
        {
            player.gameType = GameType.None;
            player.boardSetupDetails.selectionScreen = SelectionScreen.PlayerNumbers;
            player.boardSetupDetails.cursorLocation = CursorLocation.OnSolo;
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
        /// Change the game type depending on the cursor location
        /// </summary>
        /// <param name="player">The player to change</param>
        private void SetPlayerGameType(PlayerDetails player)
        {

            switch (player.boardSetupDetails.cursorLocation)
            {
                case CursorLocation.OnSolo:
                    player.gameType = GameType.Solo;
                    player.boardSetupDetails.selectionScreen = SelectionScreen.LevelAndDifficulty;
                    player.boardSetupDetails.cursorLocation = CursorLocation.OnLevel;
                    break;
                case CursorLocation.OnLink:
                    player.gameType = GameType.Link;
                    player.boardSetupDetails.selectionScreen = SelectionScreen.LevelAndDifficulty;
                    player.boardSetupDetails.cursorLocation = CursorLocation.OnLevel;
                    break;
            }
        }

        /// <summary>
        /// Start a solo or link game
        /// </summary>
        /// <param name="player">The player to start</param>
        private void StartGame(PlayerDetails player)
        {
            //change the state of the player that hit start, 
            //and also any players that were selected for link play if they were
            if (player.gameType == GameType.Link)
            {
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                    if (_player.gameType == GameType.Link)
                        _player.gameState = GameState.Started;
            }
            else
                player.gameState = GameState.Started;
            ((TrashGame)Game).StartGame();
        }

        /// <summary>
        /// Called to update the screen of each player when in settings mode
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //Check to see if we should enable being able to start a linked game
            CheckAllowedLinkPlayStart();
            //move cursor if inputHelper indicates it is required for each player.
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.SelectSettings)
                    UpdateCursor(player);

        }

        /// <summary>
        /// Upate the location of the cursor depending on key presses by a player
        /// </summary>
        /// <param name="player">The player to use</param>
        private void UpdateCursor(PlayerDetails player)
        {
            BoardSetupDetails board = player.boardSetupDetails;
            InputHelper helper = player.inputHelper;

            //first see if we are exiting and if so if we need to close the game
            if (player.isExiting)
            {
                //confirm exit selected so close the game
                if (player.inputHelper.IsEnterPressed())
                    Game.Exit();
                //cancel seleceted so undo exiting state
                if (player.inputHelper.IsBackPressed())
                    player.isExiting = false;
            }
            switch (board.selectionScreen)
            {
                case SelectionScreen.PlayerNumbers:
                    switch (board.cursorLocation)
                    {
                        case CursorLocation.OnSolo:
                            if (helper.IsEnterPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                SetPlayerGameType(player);
                            }
                            else if (helper.IsDownPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                board.cursorLocation = CursorLocation.OnLink;
                            }
                            break;
                        case CursorLocation.OnLink:
                            if (helper.IsEnterPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                SetPlayerGameType(player);
                            }
                            else if (helper.IsUpPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                board.cursorLocation = CursorLocation.OnSolo;
                            }
                            else if (helper.IsDownPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                board.cursorLocation = CursorLocation.OnExit;
                            }
                            break;
                        case CursorLocation.OnExit:
                            if (helper.IsEnterPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                player.isExiting = true; ;
                            }
                            else if (helper.IsUpPressed())
                            {
                                Sound.Play(SoundEffectType.Navigate);
                                board.cursorLocation = CursorLocation.OnLink;
                            }
                            break;
                    }
                    break;

                case SelectionScreen.LevelAndDifficulty:
                    if (helper.IsBackPressed())
                        GoToTopScreen(player);
                    else
                        switch (board.cursorLocation)
                        {
                            case CursorLocation.OnLevel:
                                if (helper.IsEnterPressed() || helper.IsRightPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.currentLevel++;
                                }
                                if (helper.IsLeftPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.currentLevel--;
                                }
                                if (helper.IsDownPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.cursorLocation = CursorLocation.OnDifficulty;
                                }
                                break;
                            case CursorLocation.OnDifficulty:
                                if (helper.IsEnterPressed() || helper.IsRightPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.currentDifficulty++;
                                }
                                if (helper.IsLeftPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.currentDifficulty--;
                                }
                                if (helper.IsDownPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.cursorLocation = CursorLocation.OnStart;
                                }
                                if (helper.IsUpPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.cursorLocation = CursorLocation.OnLevel;
                                }
                                break;
                            case CursorLocation.OnStart:
                                if (player.gameType == GameType.Solo || allowLinkPlayStart)
                                {
                                    if (helper.IsEnterPressed())
                                    {
                                        Sound.Play(SoundEffectType.Navigate);
                                        StartGame(player);
                                    }
                                }
                                if (helper.IsUpPressed())
                                {
                                    Sound.Play(SoundEffectType.Navigate);
                                    board.cursorLocation = CursorLocation.OnDifficulty;
                                }
                                break;
                        }
                    break;
            }
        }
    }
}
