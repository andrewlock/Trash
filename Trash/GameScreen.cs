using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// A screen which allows drawing of gameboards for players when InPlay
    /// </summary>
    public class GameScreen : Screen
    {
        //variables for storing textures and strings
        string backgroundImage;
        Texture2D backgroundTexture;
        Texture2D darkenBoardTexture;
        string pauseFont = "Narkisim_16";

        //The number of wins required in alink game to win the game
        int winsRequired = 3;

        /// <summary>
        /// The constructor for the game screen
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="backgroundImage">The background image to display 
        /// Note that null is passed to the parent so this is not displayed</param>
        /// <param name="backgroundMusic">The background music to play</param>
        public GameScreen(Game game, string backgroundImage, MusicType backgroundMusic)
            : base(game, backgroundMusic)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Called after a Level is complete to continue to the next one or to restart in a link game
        /// </summary>
        /// <param name="player">The player to continue</param>
        /// <returns>True if game was started, false if reached last level or not all players are ready</returns>
        public bool Continue(PlayerDetails player)
        {
            //if single player, continue, otherwise set this player to ready to continue
            Debug.Assert(player.gameType != GameType.None);

            if (player.gameType == GameType.Solo)
                return player.gameBoard.NextLevel();
            else
            {
                player.gameState = GameState.ReadyToContinue;
                bool allPlayersReady = true;
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                    if (_player.gameType == GameType.Link && _player.gameState == GameState.LevelChange)
                        allPlayersReady = false;

                if (allPlayersReady)
                {
                    foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                        if (_player.gameType == GameType.Link)
                            _player.gameBoard.RestartLevel();
                    return true;
                }
                else
                    return false;
            }

        }

        /// <summary>
        /// Draws the gameboards for each player in Players
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            //set the spritefont used by the screen 
            TextDrawer.spriteBatch = SpriteBatch;
            //draw each board if required
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.InPlay)
                {
                    SpriteBatch.Begin();
                    SpriteBatch.GraphicsDevice.Viewport = player.viewport;
                    SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                    player.gameBoard.Draw(SpriteBatch);
                    //if exiting or paused then darken the board and write text to indicate status
                    if (player.isExiting)
                        DrawExit(player);
                    else if (player.isPaused)
                        DrawPause(player);
                    SpriteBatch.End();
                }
        }

        /// <summary>
        /// If the player has pressed the exit key, shows a confirmation screen
        /// </summary>
        /// <param name="player">The player who pressed exit</param>
        private void DrawExit(PlayerDetails player)
        {
            SpriteBatch.Draw(darkenBoardTexture, Vector2.Zero, Color.White);
            //Write text to indicate status in center of the screen
            TextDrawer.DrawText(pauseFont, "QUIT GAME?\n\n\nPress [" + player.inputHelper.enterKey.ToString() + "] to Exit the game"
                + "\n\nOr Press [" + player.inputHelper.backKey.ToString() + "] to Resume",
                PlayingAreaConstants.BoardCentre, Color.White,
                TextAlignment.Center, TextVerticalAlignment.Middle, PlayingAreaConstants.Width);

        }

        /// <summary>
        /// If the player has pressed the pause key, shows a resume screen
        /// </summary>
        /// <param name="player">The player who pressed pause</param>
        private void DrawPause(PlayerDetails player)
        {
            SpriteBatch.Draw(darkenBoardTexture, Vector2.Zero, Color.White);
            //Write text to indicate status in center of the screen
            TextDrawer.DrawText(pauseFont, "PAUSED\n\n\nPress [" + player.inputHelper.pauseKey.ToString() + "] to Continue",
                PlayingAreaConstants.BoardCentre, Color.White,
                TextAlignment.Center, TextVerticalAlignment.Middle, PlayingAreaConstants.Width);
        }

        /// <summary>
        /// Called when a player presses the exit key during a game
        /// </summary>
        /// <param name="player">The player who pressed exit</param>
        private void ExitGame(PlayerDetails player)
        {
            //toggle the player's exiting state
            player.isExiting = !player.isExiting;

            //if the player is playing a link game then toggle all screens
            if (player.gameType == GameType.Link)
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                    if (_player.gameType == GameType.Link && _player.gameState == GameState.InPlay)
                        _player.isExiting = player.isExiting;
        }

        /// <summary>
        /// Called after a player confirms the desire to exit this game
        /// </summary>
        /// <param name="player">The player who confirmed the exit</param>
        private void ExitGameConfirm(PlayerDetails player)
        {
            //A player has confirmed they wish to quit the game set the state of 
            //this and all link players to GameOver and change them to solo(to avoid showing a winner)

            player.gameState = GameState.GameOver;

            if (player.gameType == GameType.Link)
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                    if (_player.gameType == GameType.Link && _player.gameState == GameState.InPlay)
                    {
                        _player.gameState = GameState.GameOver;
                        _player.gameType = GameType.Solo;
                    }

            player.gameType = GameType.Solo;

            //we call the Game1 GameOver function to skip determining a winner etc in link games
            ((TrashGame)Game).GameOver(player);
        }

        /// <summary>
        /// Called by a player's gameboard when they lose the game
        /// </summary>
        /// <param name="player">The player who lost</param>
        public void GameOver(PlayerDetails player)
        {
            //check to see if we are playing a solo game
            Debug.Assert(player.gameType != GameType.None);
            if (player.gameType == GameType.Solo)
                ((TrashGame)Game).GameOver(player);
            else
            //we are playing a link game
            {

                //cycle through all players - if we are playing a link game, and this was the last 
                //competitor left, then the round is over
                int numPlayersRemaining = 0;
                PlayerDetails winner = null;
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                {
                    if (_player.gameType == GameType.Link && _player.gameState == GameState.InPlay)
                    {
                        winner = _player;
                        numPlayersRemaining++;
                    }
                }
                if (numPlayersRemaining == 1)
                {
                    winner.wins++;
                    if (winner.wins == winsRequired)
                    {
                        //need to set all link players to state GameOver
                        foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                            if (_player.gameType == GameType.Link)
                                _player.gameState = GameState.GameOver;
                        ((TrashGame)Game).GameOver(winner);
                    }
                    else
                    {
                        //need to set all link players to state Levelchange
                        foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                            if (_player.gameType == GameType.Link)
                                _player.gameState = GameState.LevelChange;

                        ((TrashGame)Game).LevelComplete(winner);
                    }
                }

            }
        }

        /// <summary>
        /// Initializes the game screen and the game board for each player
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                player.gameBoard.Initialize();
        }

        /// <summary>
        /// Called when a player completes the board
        /// </summary>
        /// <param name="winner">The player who completed the level</param>
        public void LevelComplete(PlayerDetails winner)
        {
            //check to see if we are playing a solo game
            Debug.Assert(winner.gameType != GameType.None);
            if (winner.gameType == GameType.Solo)
                ((TrashGame)Game).LevelComplete(winner);
            else
            //we are playing a link game
            {
                //cycle through all players - if we are playing a link game, and this was the last 
                //competitor left, then the round is over
                winner.wins++;
                foreach (PlayerDetails player in ((TrashGame)Game).Players)
                {
                    if (player.gameType == GameType.Link && player.gameState == GameState.InPlay)
                    {
                        player.gameBoard.OpponentWon();
                        if (winner.wins != winsRequired)
                            player.gameState = GameState.LevelChange;
                    }
                    if (winner.wins == winsRequired)
                    {
                        winner.gameState = GameState.GameOver;
                        ((TrashGame)Game).GameOver(winner);
                    }
                    else
                    {
                        ((TrashGame)Game).LevelComplete(winner);
                    }
                }
            }
        }

        /// <summary>
        /// Load any background textures and fonts
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            backgroundTexture = Game.Content.Load<Texture2D>(backgroundImage);
            darkenBoardTexture = Game.Content.Load<Texture2D>(@"Textures/Darken_board");
        }

        /// <summary>
        /// Begin a new game
        /// </summary>
        public void NewGame()
        {
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.Started)
                {
                    //reset the paused and exiting variables
                    player.isPaused = false;
                    player.isExiting = false;
                    player.gameBoard.NewGame(player.boardSetupDetails.currentLevel, player.boardSetupDetails.currentDifficulty);
                }
        }

        /// <summary>
        /// Called when a player presses the pause key during a game
        /// </summary>
        /// <param name="player">The player who pressed pause</param>
        private void PausePlayer(PlayerDetails player)
        {
            //toggle the player's pause state
            player.isPaused = !player.isPaused;

            //if the player is playing a link game then toggle all screens
            if (player.gameType == GameType.Link)
                foreach (PlayerDetails _player in ((TrashGame)Game).Players)
                    if (_player.gameType == GameType.Link && _player.gameState == GameState.InPlay)
                        _player.isPaused = player.isPaused;
        }

        /// <summary>
        /// Called when a player gets multiple lines in one move
        /// Sends trash to opposing players
        /// </summary>
        /// <param name="origin">The player who achieved the multpile lines</param>
        /// <param name="trashColors">The colors of the lines used to trash</param>
        public void SendTrash(PlayerDetails origin, List<Color> trashColors)
        {
            if (origin.gameType == GameType.Link)
                foreach (PlayerDetails player in ((TrashGame)Game).Players)
                    if (player.gameType == GameType.Link && player != origin)
                    {
                        player.gameBoard.trashColors = trashColors;
                        player.gameBoard.trashRequired = true;
                    }
        }

        /// <summary>
        /// Called to update the gameboards when the game is inplay
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (PlayerDetails player in ((TrashGame)Game).Players)
                if (player.gameState == GameState.InPlay)
                {
                    //check to see if the player has pressed pause
                    if (player.inputHelper.IsPausePressed())
                        PausePlayer(player);
                    //if the player was previously exiting then return to game otherwise start exiting
                    if (player.inputHelper.IsBackPressed())
                        ExitGame(player);
                    //if the player is exiting and has pressed enter then continue
                    if (player.isExiting)
                        if (player.inputHelper.IsEnterPressed())
                            ExitGameConfirm(player);

                    if (!player.isPaused && !player.isExiting)
                        player.gameBoard.Update(gameTime);
                }
        }
    }
}
