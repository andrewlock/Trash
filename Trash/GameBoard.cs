using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Trash
{
    /// <summary>
    /// A Game board. The majority of game logic is implemented here
    /// </summary>
    public class GameBoard : DrawableGameComponent
    {
        static int boardHeight = PlayingAreaConstants.Height / BasicPill.height;
        static int boardWidth = PlayingAreaConstants.Width / BasicPill.width;

        //lists of lines completed this turn and the pills that were freed by completing them
        List<List<BasicPill>> completedLines = new List<List<BasicPill>>();

        //list for trashing the other player and updating score
        List<List<BasicPill>> completedLinesThisMove = new List<List<BasicPill>>();
        int currentDifficulty = 2;

        //the pill curretly being controlled by the user
        DoublePill currentDoublePill;

        //The current level and difficulty parameters
        int currentLevel = 0;
        int defaultTimeBetweenMoves = 500;

        //the state of the currently dropping pill
        PillDroppingState dropState = PillDroppingState.Ready;
        List<BasicPill> freedPills = new List<BasicPill>();
        int neckSpeed = 1;

        //the next pill that appears at the top of the screen
        DoublePill nextDoublePill;
        GameScreen parentScreen;

        //The board, an array of pill parts
        BasicPill[,] pillParts = new BasicPill[boardWidth, boardHeight];

        //how many pills are required to complete a line
        int pillsNeededForLine = 4;

        //size of gaming board
        Vector2 pillStartPosition { get { return PlayingAreaConstants.TopOfNeck; } }

        //Reference to the random number generator (only 1 used in application to ensure threadsafe)
        Random rand;
        int timeBetweenMoves;
        int timeBetweenRandoms = 300;

        //speed and time constants
        int timeTillNextMove = 0;
        int timeTillNextRandom;
        public List<Color> trashColors;
        int trashSpeed = 2;

        /// <summary>
        /// The PlayerDetails object associated with this board
        /// </summary>
        public PlayerDetails player { get; set; }

        /// <summary>
        /// Whether a trash is required on this gameboard
        /// </summary>
        public bool trashRequired { get; set; }

        /// <summary>
        /// Create a game board
        /// </summary>
        /// <param name="game">The main game</param>
        /// <param name="parentScreen">The GameScreen that handles passing trash to other players</param>
        public GameBoard(Game game, GameScreen parentScreen)
            : base(game)
        {
            Debug.Assert(parentScreen != null);
            rand = ((TrashGame)Game).rnd;
            this.parentScreen = parentScreen;
        }


        /// <summary>
        /// Converts a board grid position into a pixel screen position 
        /// </summary>
        /// <param name="x">Number of pills wide</param>
        /// <param name="y">Number of pills high</param>
        /// <returns>The screen position</returns>
        protected static Vector2 BoardToScreen(int x, int y)
        {
            return new Vector2(PlayingAreaConstants.LeftEdge + (float)(x * BasicPill.width),
                               PlayingAreaConstants.TopEdge + (float)(y * BasicPill.height));
        }

        /// <summary>
        /// Checks the given board position in all directions for connected colors
        /// </summary>
        /// <param name="boardPos">The Board POsition to check</param>
        private void CheckForLines(Point boardPos)
        {
            Debug.Assert(pillParts[boardPos.X, boardPos.Y] != null);
            Color col = pillParts[boardPos.X, boardPos.Y].color;

            List<BasicPill> horizLine = new List<BasicPill>();
            horizLine.Add(pillParts[boardPos.X, boardPos.Y]);
            List<BasicPill> vertLine = new List<BasicPill>();
            vertLine.Add(pillParts[boardPos.X, boardPos.Y]);

            //Check to the left of the pill for identically coloured pillParts
            //if found then insert at front of horizontal line list
            for (int i = boardPos.X - 1; i >= 0; i--)
            {
                if (pillParts[i, boardPos.Y] != null && pillParts[i, boardPos.Y].color == col)
                    horizLine.Insert(0, pillParts[i, boardPos.Y]);
                else
                    break;
            }
            //Check to the right of the pill for identically coloured pillParts
            //if found then insert at end of horizontal line list
            for (int i = boardPos.X + 1; i < boardWidth; i++)
            {
                if (pillParts[i, boardPos.Y] != null && pillParts[i, boardPos.Y].color == col)
                    horizLine.Add(pillParts[i, boardPos.Y]);
                else
                    break;
            }

            //Check above the pill for identically coloured pillParts
            //if found then insert at end of vertical line list
            for (int i = boardPos.Y - 1; i >= 0; i--)
            {
                if (pillParts[boardPos.X, i] != null && pillParts[boardPos.X, i].color == col)
                    vertLine.Add(pillParts[boardPos.X, i]);
                else
                    break;
            }
            //check down
            for (int i = boardPos.Y + 1; i < boardHeight; i++)
            {
                if (pillParts[boardPos.X, i] != null && pillParts[boardPos.X, i].color == col)
                    vertLine.Insert(0, pillParts[boardPos.X, i]);
                else
                    break;
            }

            //if lists contain enough members then add to completed lines list

            //checkto see if contents of the line exactly matches a line already created
            if (horizLine.Count >= pillsNeededForLine)
            {
                bool alreadyUsed = false;
                foreach (List<BasicPill> linesAlreadyFound in completedLines)
                {
                    alreadyUsed = true;
                    foreach (BasicPill pill in horizLine)
                    {
                        alreadyUsed = alreadyUsed && linesAlreadyFound.Contains(pill);
                    }
                    if (alreadyUsed == true)
                        break;
                }
                if (!alreadyUsed)
                {
                    completedLines.Add(horizLine);
                    Sound.Play(SoundEffectType.BreakPills);
                    //linesCompletedThisMove++;
                    completedLinesThisMove.Add(horizLine);
                    //lineColorsThisMove.Add(horizLine[0].color);
                }
            }

            if (vertLine.Count >= pillsNeededForLine)
            {
                bool alreadyUsed = false;
                foreach (List<BasicPill> linesAlreadyFound in completedLines)
                {
                    alreadyUsed = true;

                    foreach (BasicPill pill in vertLine)
                    {
                        alreadyUsed = alreadyUsed && linesAlreadyFound.Contains(pill);
                    }
                    if (alreadyUsed == true)
                        break;

                }
                if (!alreadyUsed)
                {
                    completedLines.Add(vertLine);
                    Sound.Play(SoundEffectType.BreakPills);
                    //linesCompletedThisMove++;
                    //lineColorsThisMove.Add(vertLine[0].color);
                    completedLinesThisMove.Add(vertLine);

                }
            }
        }

        /// <summary>
        /// Check any freed pills to see if they have made contact
        /// </summary>
        /// <returns>Returns true if any pills have changed state i.e. made or lost contact</returns>
        private bool CheckFreedPillsForContact()
        {
            bool changedAnyStates = false;
            //cycle through all pills in freed(dropping) pills
            foreach (BasicPill pillPart in freedPills)
            {
                //only change those pills that have not made contact
                if (pillPart.droppingState != PillDroppingState.Contact)
                {
                    //check to see if going to make contact, 
                    //if it is, then change this and any connected pill's state and 
                    //add this to the pillparts matrix and increment number of pills whose state changed
                    if (WillContactPill(pillPart, new Point(0, trashSpeed)) || !WillStayInBounds(pillPart, new Point(0, trashSpeed)))
                    {
                        pillPart.droppingState = PillDroppingState.Contact;
                        Sound.Play(Random.Shared.Next(2) == 0
                            ? SoundEffectType.LandPills1
                            : SoundEffectType.LandPills2);
                        Point boardPos = ScreenToBoard(pillPart.position);
                        pillParts[boardPos.X, boardPos.Y] = pillPart;
                        changedAnyStates = true;
                        if (pillPart.ConnectedPill != null)
                        {
                            pillPart.ConnectedPill.droppingState = PillDroppingState.Contact;
                            boardPos = ScreenToBoard(pillPart.ConnectedPill.position);
                            pillParts[boardPos.X, boardPos.Y] = pillPart.ConnectedPill;
                        }
                    }
                }
            }
            return changedAnyStates;
        }

        /// <summary>
        /// Check if we have lost the game by filling the neck of the board
        /// </summary>
        /// <returns>Returns true if the game is over</returns>
        private bool CheckHaveGameOver()
        {
            //game is over when we have a pill part stored in the middle of the top row of the board
            if (pillParts[(boardWidth - 1) / 2, 0] != null || pillParts[((boardWidth - 1) / 2) + 1, 0] != null)
            {
                GameOver();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if we have completed the game by eliminating all the germs
        /// </summary>
        /// <returns>Returns true if the game is over</returns>
        private bool CheckHaveLevelComplete()
        {
            foreach (BasicPill pillPart in pillParts)
                if (pillPart != null)
                    if (pillPart.Immobile == true)
                        return false;

            //didn't find any so level complete
            return true;
        }

        /// <summary>
        /// Removes all pills from the board
        /// </summary>
        private void ClearBoard()
        {
            for (int x = 0; x < boardWidth; x++)
                for (int y = 0; y < boardHeight; y++)
                    pillParts[x, y] = null;
            currentDoublePill = null;
            nextDoublePill = null;

        }

        /// <summary>
        /// Create a random number of germs based on the current difficuly level
        /// </summary>
        private void CreateGerms()
        {
            //create germs using the current level and difficulty and list of parameters

            for (int y = boardHeight - 1; y > boardHeight - 1 - TrashGame.levelInfoList.levels[currentLevel].germsRows; y--)
            {
                //see how many germs to create in this row
                int numberGerms = rand.Next(TrashGame.levelInfoList.levels[currentLevel].minGermsPerRow, TrashGame.levelInfoList.levels[currentLevel].maxGermsPerRow);
                for (int x = 0; x < numberGerms; x++)
                {
                    int horiz = FindRandomHoleInRow(y);

                    Debug.Assert(horiz > -1);
                    //if horiz = -1 then we have no space in this row so cannot create any more germs
                    if (horiz == -1)
                        break;
                    //have a hole so create a germ in it
                    pillParts[horiz, y] = new BasicPill(Game.Content.Load<Texture2D>(@"Textures/germ"), new Point(24, 24), new Point(1, 1),
                        Game.Content.Load<Texture2D>(@"Textures/germ_breaking"), new Point(24, 24), new Point(7, 4),
                        Game.Content.Load<Texture2D>(@"Textures/pill_gone"), new Point(24, 24), new Point(1, 1),
                        Game.Content.Load<Texture2D>(@"Textures/germ_random"), new Point(24, 24), new Point(6, 8),
                        Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                        BoardToScreen(horiz, y), TrashGame.pillColours[rand.Next(TrashGame.pillColours.GetLength(0))], 0, SpriteEffects.None, PillScores.GermScore);
                    //make the germ immobile
                    pillParts[horiz, y].Immobile = true;
                }
            }
        }

        ///<summary>
        ///Create next pill at the top of the jar
        ///</summary>
        private void CreateNextDoublePill()
        {
            nextDoublePill = new DoublePill(Game.Content.Load<Texture2D>(@"Textures/pill_double"), new Point(24, 24), new Point(1, 1),
                Game.Content.Load<Texture2D>(@"Textures/pill_breaking"), new Point(24, 24), new Point(7, 4),
                Game.Content.Load<Texture2D>(@"Textures/pill_gone"), new Point(24, 24), new Point(1, 1),
                Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                pillStartPosition, TrashGame.pillColours[rand.Next(TrashGame.pillColours.GetLength(0))],
                TrashGame.pillColours[rand.Next(TrashGame.pillColours.GetLength(0))], PillScores.PillScore);
        }

        /// <summary>
        /// Creates trash on this board which has been dropped from an other player
        /// </summary>
        public void CreateTrash()
        {
            Sound.Play(SoundEffectType.TrashDrop);
            //keep note of pills we've added 
            List<BasicPill> trashCreated = new List<BasicPill>();
            foreach (Color color in trashColors)
            {
                //find a random hole at top of board and create a pill in it
                int horiz = FindRandomHoleInRow(0);

                //if horiz ==-1 no holes available
                if (horiz == -1)
                    break;

                pillParts[horiz, 0] = new BasicPill(Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                    Game.Content.Load<Texture2D>(@"Textures/pill_breaking"), new Point(24, 24), new Point(7, 4),
                    Game.Content.Load<Texture2D>(@"Textures/pill_gone"), new Point(24, 24), new Point(1, 1),
                    Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                    Game.Content.Load<Texture2D>(@"Textures/pill"), new Point(24, 24), new Point(1, 1),
                    BoardToScreen(horiz, 0), color, 0, SpriteEffects.None, PillScores.TrashScore);
                trashCreated.Add(pillParts[horiz, 0]);

            }

            //now we've added them to the board, free them
            foreach (BasicPill pill in trashCreated)
            {
                FreePill(pill);
            }

            //change drop state to allow update of dropping pills
            dropState = PillDroppingState.ProcessingLines;

            //reset old trash details
            trashRequired = false;
            trashColors.Clear();
        }

        /// <summary>
        /// Draw all of the pillparts on the board, the current dropping pill, and the next pill
        /// SpriteBatch.Begin must have been called
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch with which to draw.</param>
        public void Draw(ResizedSpriteBatch spriteBatch)
        {
            //draw all pillParts (includes germs as these are just BasicPills)
            foreach (BasicPill pill in pillParts)
            {
                if (pill != null)
                    pill.Draw(spriteBatch);
            }

            foreach (BasicPill pill in freedPills)
                pill.Draw(spriteBatch);

            if (currentDoublePill != null)
                currentDoublePill.Draw(spriteBatch);

            if (nextDoublePill != null)
                nextDoublePill.Draw(spriteBatch);
        }

        /// <summary>
        /// Set the next user controlled double pill to be the current double pill
        /// </summary>
        private void DropPill()
        {
            //when we start the game we won't have a currentDouble pill
            //This is the only time, and we need to create a next one
            if (currentDoublePill == null)
                CreateNextDoublePill();

            //make the next pill the current pill and set dropping state
            currentDoublePill = nextDoublePill;
            dropState = PillDroppingState.InNeck;
            nextDoublePill = null;

        }

        /// <summary>
        /// Used to search for a random hole in a given row of the board
        /// </summary>
        /// <param name="yBoardValue">Which row to search</param>
        /// <returns>The x position of the whole</returns>
        private int FindRandomHoleInRow(int yBoardValue)
        {
            //get position of this pill in the row
            int horiz = rand.Next(0, boardWidth);

            //try and put it in the slot, if there is a pill already there
            //look either side till find a hole

            int offset = 0;
            while (pillParts[horiz, yBoardValue] != null)
            {
                offset++;

                //if we've completely run out of holes, cannot place pill in this row
                if (((horiz + offset) >= boardWidth) && (horiz < offset))
                    return -1;

                if ((horiz + offset) < boardWidth)
                {
                    if (pillParts[horiz + offset, yBoardValue] == null)
                    {
                        horiz += offset;
                        break;
                    }
                }

                if (horiz >= offset)
                {
                    if (pillParts[horiz - offset, yBoardValue] == null)
                    {
                        horiz -= offset;
                        break;
                    }
                }
            }
            return horiz;
        }

        /// <summary>
        /// Remove a pill from the board and add it to the free pill list
        /// </summary>
        /// <param name="thisPill">The pill to free</param>
        private void FreePill(BasicPill thisPill)
        {
            freedPills.Add(thisPill);
            RemovePillPartFromBoard(thisPill);
            thisPill.droppingState = PillDroppingState.InBottle;
        }

        /// <summary>
        /// End the game and notify the parent screen
        /// </summary>
        private void GameOver()
        {
            player.gameState = GameState.GameOver;
            parentScreen.GameOver(player);
        }

        /// <summary>
        /// Initialise component
        /// </summary>
        public override void Initialize()
        {
            trashRequired = false;
            base.Initialize();
        }

        /// <summary>
        /// The Level has been cleared and completed
        /// </summary>
        private void LevelComplete()
        {
            player.gameState = GameState.LevelChange;
            parentScreen.LevelComplete(player);
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        /// <param name="level">The level to start</param>
        /// <param name="difficulty">The difficulty to use</param>
        public void NewGame(int level, int difficulty)
        {
            Debug.Assert(parentScreen != null);
            Debug.Assert(player != null);
            currentLevel = level;
            currentDifficulty = difficulty;

            //set the time between moves based on the selected level
            timeBetweenMoves = (int)(defaultTimeBetweenMoves * TrashGame.levelInfoList.levels[currentLevel].pillSpeed * TrashGame.difficultyInfoList.levels[currentDifficulty].pillSpeedModifier);
            timeTillNextRandom = timeBetweenRandoms;

            //reset the Board
            ClearBoard();
            CreateGerms();
            player.gameState = GameState.InPlay;
            player.inputHelper.allowMultipleEnterPresses = true;
        }

        /// <summary>
        /// Move to the next level
        /// </summary>
        /// <returns>Returns true if started ok, false if we have reached the final level</returns>
        public bool NextLevel()
        {
            player.boardSetupDetails.currentLevel++;
            if (currentLevel < player.boardSetupDetails.currentLevel)
            {
                currentLevel++;
                NewGame(currentLevel, currentDifficulty);
                return true;
            }
            //just completed the last level!
            else
            {
                player.gameState = GameState.GameComplete;
                ((TrashGame)Game).GameOver(player);
            }
            return false;
        }

        /// <summary>
        /// The opponent lost the game by filling their board in a link game
        /// </summary>
        public void OpponentLost()
        {
            player.gameState = GameState.LevelChange;
        }

        /// <summary>
        /// The opponent won the game by clearing all germs in a Link game
        /// </summary>
        public void OpponentWon()
        {
            player.gameState = GameState.GameOver;
        }

        /// <summary>
        /// Pick some random immobile pillparts to animate randomly
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void PickRandoms(GameTime gameTime)
        {
            timeTillNextRandom -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeTillNextRandom < 0)
            {
                timeTillNextRandom = timeBetweenRandoms;

                //pick a random square on the board, if it contains a pill, change state to random
                BasicPill pill = pillParts[rand.Next(boardWidth), rand.Next(boardHeight)];
                if (pill != null)
                    if (pill.Immobile)
                        pill.ChangeState(PillAnimationState.Random);
            }

        }

        /// <summary>
        /// Removes a pillpart from the board
        /// </summary>
        /// <param name="pillPart">The pill part to remove</param>
        private void RemovePillPartFromBoard(BasicPill pillPart)
        {
            Point boardPos = ScreenToBoard(pillPart.position);
            pillParts[boardPos.X, boardPos.Y] = null;
        }

        /// <summary>
        /// Start a new game with the same level and difficulty as the last
        /// </summary>
        public void RestartLevel()
        {
            NewGame(currentLevel, currentDifficulty);
        }

        /// <summary>
        /// Converts a pixel screen position into a board grid position
        /// </summary>
        /// <param name="screenPos">Screen Position</param>
        /// <returns>The board Position</returns>
        protected static Point ScreenToBoard(Vector2 screenPos)
        {
            return new Point((int)((screenPos.X - PlayingAreaConstants.LeftEdge) / BasicPill.width),
                               (int)((screenPos.Y - PlayingAreaConstants.TopEdge) / BasicPill.height));
        }

        /// <summary>
        /// Converts a pixel screen position into a board grid position
        /// </summary>
        /// <param name="x">Pixels across</param>
        /// <param name="y">Pixels down</param>
        /// <returns>The board Position</returns>
        protected static Point ScreenToBoard(int x, int y)
        {
            return new Point(((x - PlayingAreaConstants.LeftEdge) / BasicPill.width),
                              ((y - PlayingAreaConstants.TopEdge) / BasicPill.height));
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //perform action only if player is InPLay
            if (player.gameState == GameState.InPlay)
            {
                //Take action depending on current dropState of pills
                switch (dropState)
                {
                    case PillDroppingState.Contact:
                        if (completedLines.Count > 0)
                            UpdateBreakingPills(gameTime);
                        else
                        //updatescore if necessary and trash if required
                        {
                            if (completedLinesThisMove.Count > 0)
                                UpdateScore();
                            if (trashRequired)
                                CreateTrash();
                            else
                                dropState = PillDroppingState.Ready;
                            if (CheckHaveGameOver())
                                nextDoublePill = null;
                        }
                        break;
                    case PillDroppingState.ProcessingLines:
                        UpdateFreedPills(gameTime);
                        break;
                    case PillDroppingState.Ready:
                    case PillDroppingState.InNeck:
                    case PillDroppingState.InBottle:
                        if (CheckHaveLevelComplete())
                            LevelComplete();
                        else
                            UpdateDroppingPill(gameTime);
                        break;
                }
                PickRandoms(gameTime);
                UpdatePillParts(gameTime);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Update lines that require processing and pills breaking
        /// </summary>
        /// <param name="gametime">Provides a snapshot of timing values</param>
        private void UpdateBreakingPills(GameTime gametime)
        {
            //we have some lines to process
            for (int i = 0; i < completedLines.Count; i++)
            {
                List<BasicPill> line = completedLines[i];
                //process each pill in the line, setting its animation state to breaking 
                //if it is currently not breaking or gone
                bool lineCompletelyBroken = true;
                foreach (BasicPill pillPart in line)
                {
                    if (pillPart.animationState == PillAnimationState.None || pillPart.animationState == PillAnimationState.Random)
                        pillPart.ChangeState(PillAnimationState.Breaking);


                    //check to see if line completely broken
                    lineCompletelyBroken = lineCompletelyBroken && (pillPart.animationState == PillAnimationState.Gone);
                }

                //if line is completely broken, we need to check surrounding pills, 
                //disconnect them, from their partners and  
                //and add them to the dropping list if they are no longer supported
                //also keep track of number of germs removed
                if (lineCompletelyBroken)
                {
                    foreach (BasicPill pillPart in line)
                    {
                        //if it was a connected pill, disconnect the pills, check if that pill is supported
                        //and if not add to freedPills list
                        if (pillPart.ConnectedPill != null)
                        {
                            //make sure the connected pill is not in the line to prevent repeat calls 
                            if (!line.Contains(pillPart.ConnectedPill))
                            {
                                //WillDrop(ScreenToBoard(pillPart.ConnectedPill.position), false);
                                pillPart.ConnectedPill.Disconnect();
                                pillPart.Disconnect();
                            }
                        }
                        //remove the pill part from the pillParts array
                        RemovePillPartFromBoard(pillPart);
                    }

                    //now have all disconnected pills added to freed pills (if they currently are not supported)
                    //now check all pills including and above the freed lines to find if they are disconnected

                    //find lowest row we need to check
                    float maxPos = PlayingAreaConstants.TopEdge;
                    foreach (BasicPill pillPart in line)
                    {
                        if (pillPart.position.Y > maxPos)
                            maxPos = pillPart.position.Y;
                    }
                    Point boardPos = ScreenToBoard(new Vector2(PlayingAreaConstants.LeftEdge, maxPos));
                    for (int y = boardPos.Y; y >= 0; y--)
                    {
                        for (int x = 0; x < boardWidth; x++)
                        {
                            if (pillParts[x, y] != null)
                                WillDrop(ScreenToBoard(pillParts[x, y].position));
                        }
                    }

                    //as line was completely broken, change game state and remove from completed lines list
                    completedLines.RemoveAt(i);
                    i--;
                    dropState = PillDroppingState.ProcessingLines;

                }

            }
        }

        /// <summary>
        /// Update the dropping pill, 
        /// checking for user interaction and contact with other pills
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdateDroppingPill(GameTime gameTime)
        {
            //If we don't have a pill then first drop one
            if (currentDoublePill == null)
                DropPill();


            //update dropping pill by moving down the screen a single pill distance
            if (dropState == PillDroppingState.InBottle)
            {
                //move and drop pill if keys pressed and path is clear
                if (player.inputHelper.IsLeftPressed()
                    && !WillContactPill(currentDoublePill, new Point(-BasicPill.width, 0))
                    && WillStayInBounds(currentDoublePill, new Point(-BasicPill.width, 0)))
                    currentDoublePill.Move(new Vector2(-BasicPill.width, 0));
                if (player.inputHelper.IsRightPressed()
                    && !WillContactPill(currentDoublePill, new Point(BasicPill.width, 0))
                    && WillStayInBounds(currentDoublePill, new Point(BasicPill.width, 0)))
                    currentDoublePill.Move(new Vector2(BasicPill.width, 0));
                if (player.inputHelper.IsEnterPressed()
                    && !WillContactPill(currentDoublePill, new Point(0, BasicPill.height))
                    && WillStayInBounds(currentDoublePill, new Point(0, BasicPill.height)))
                    currentDoublePill.Move(new Vector2(0, BasicPill.height));

                //perform rotation if keys pressed and path is clear
                if (player.inputHelper.IsDownPressed())
                {
                    //want to rotate but depends if we have room which depends on orientation of dropping pill
                    //easiest way is to rotate the pill, find out if ok and rotate back if not
                    currentDoublePill.Rotate(MathHelper.PiOver2);
                    if (WillContactPill(currentDoublePill, Point.Zero) || !WillStayInBounds(currentDoublePill, Point.Zero))
                    {
                        //Have the possibility of shifting the pill to the left �ne if available
                        if (WillContactPill(currentDoublePill, new Point(-BasicPill.width, 0)) || !WillStayInBounds(currentDoublePill, new Point(-BasicPill.width, 0)))
                            currentDoublePill.Rotate(-MathHelper.PiOver2);
                        else
                            currentDoublePill.Move(new Vector2(-BasicPill.width, 0));
                    }
                }

                if (player.inputHelper.IsUpPressed())
                {
                    //want to rotate but depends if we have room which depends on orientation of dropping pill
                    currentDoublePill.Rotate(-MathHelper.PiOver2);
                    if (WillContactPill(currentDoublePill, Point.Zero) || !WillStayInBounds(currentDoublePill, Point.Zero))
                    {
                        //Have the possibility of shifting the pill to the left �ne if available
                        if (WillContactPill(currentDoublePill, new Point(-BasicPill.width, 0)) || !WillStayInBounds(currentDoublePill, new Point(-BasicPill.width, 0)))
                            currentDoublePill.Rotate(MathHelper.PiOver2);
                        else
                            currentDoublePill.Move(new Vector2(-BasicPill.width, 0));
                    }
                }

                //want to make sure time for next drop has elapsed before checking for contact
                timeTillNextMove -= gameTime.ElapsedGameTime.Milliseconds;
                if (timeTillNextMove < 0)
                {
                    timeTillNextMove = timeBetweenMoves;
                    if (WillContactPill(currentDoublePill, new Point(0, BasicPill.height)) ||
                        !WillStayInBounds(currentDoublePill, new Point(0, BasicPill.height)))
                    {
                        //pill has made contact with other pills
                        dropState = PillDroppingState.Contact;
                        Sound.Play(Random.Shared.Next(2) == 0
                            ? SoundEffectType.LandPills1
                            : SoundEffectType.LandPills2);

                        Point leftBoardPos = ScreenToBoard((int)currentDoublePill.leftPillPart.position.X, (int)currentDoublePill.leftPillPart.position.Y);
                        pillParts[leftBoardPos.X, leftBoardPos.Y] = currentDoublePill.leftPillPart;
                        Point rightBoardPos = ScreenToBoard((int)currentDoublePill.rightPillPart.position.X, (int)currentDoublePill.rightPillPart.position.Y);
                        pillParts[rightBoardPos.X, rightBoardPos.Y] = currentDoublePill.rightPillPart;

                        CheckForLines(leftBoardPos);
                        CheckForLines(rightBoardPos);
                    }
                    else
                    {
                        currentDoublePill.Move(new Vector2(0, BasicPill.height));
                    }
                }
            }
            //if still in the neck of the bottle, increment speed smoothly until 
            //out of the neck
            else if (dropState == PillDroppingState.InNeck)
            {
                int distanceToBottle = PlayingAreaConstants.TopEdge - (int)currentDoublePill.position.Y;
                //if we would shoot to far out the end, control the drop
                if (distanceToBottle < neckSpeed)
                {
                    currentDoublePill.Move(new Vector2(0, distanceToBottle));
                    dropState = PillDroppingState.InBottle;
                    //create a next pill
                    CreateNextDoublePill();
                    timeTillNextMove = timeBetweenMoves;
                }
                else
                    currentDoublePill.Move(new Vector2(0, neckSpeed));
            }
            //if we have processed all contact and breaking lines then drop another pill
            else if (dropState == PillDroppingState.Ready)
            {
                DropPill();
            }
        }

        /// <summary>
        /// Update any pills that have bee freed by breaking lines
        /// </summary>
        /// <param name="gametime">Provides a snapshot of timing values</param>
        private void UpdateFreedPills(GameTime gametime)
        {
            //repeatedly check falling pills for contact as can have cascade effect
            while (CheckFreedPillsForContact()) ;

            //cycle again to actually move the pills, keeping track if all have made contact 
            //and adding them back into the pillParts array
            bool allPillsContacted = true;
            foreach (BasicPill pillPart in freedPills)
            {
                if (pillPart.droppingState != PillDroppingState.Contact)
                {
                    pillPart.Move(new Vector2(0, (float)trashSpeed));
                    allPillsContacted = false;
                }
            }

            //if all pills have made contact, need to check for new lines, remove pills from freed array
            //and update status
            if (allPillsContacted)
            {
                dropState = PillDroppingState.Contact;
                for (int i = 0; i < freedPills.Count; i++)
                {
                    BasicPill pillPart = freedPills[i];
                    CheckForLines(ScreenToBoard(pillPart.position));
                }
                freedPills.Clear();
            }

        }

        /// <summary>
        /// Updates all pillparts on the board
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values</param>
        private void UpdatePillParts(GameTime gameTime)
        {
            foreach (BasicPill pillPart in pillParts)
            {
                if (pillPart != null)
                    pillPart.Update(gameTime);
            }
        }

        /// <summary>
        /// Update the plaeyer's score. 
        /// Called after lines have been completed and all pills have stopped moving
        /// </summary>
        private void UpdateScore()
        {
            //calculate score by summing score of all pills broken
            int score = 0;
            foreach (List<BasicPill> line in completedLinesThisMove)
                foreach (BasicPill pillPart in line)
                    score += pillPart.score;

            //multiply score by number of lines broken this turn and update the player's score
            score *= completedLinesThisMove.Count;
            player.score += score;

            //if have completed 2 or more lines then throw trash
            if (completedLinesThisMove.Count >= 2)
            {
                //Create color List for throwing trash
                List<Color> colorList = new List<Color>();
                foreach (List<BasicPill> line in completedLinesThisMove)
                    colorList.Add(line[0].color);

                //throw trash
                parentScreen.SendTrash(player, colorList);
            }

            //clear the record of lines for this move
            completedLinesThisMove.Clear();
        }

        /// <summary>
        /// Checks if a given pill will contact with another pill after moving a given offset
        /// </summary>
        /// <param name="pillToCheck">The IBasicPill to check</param>
        /// <param name="offset">The direction and distance to move</param>
        /// <returns>Returns false if the move is valid and will not contact another pill</returns>
        private bool WillContactPill(IBasicPill pillToCheck, Point offset)
        {
            //get collision rectangle for the pill to check offset by given amount
            Rectangle collisionRectToCheck = pillToCheck.GetCollisionRect();
            collisionRectToCheck.Offset(offset);

            //cycles through every pillPart, checks to see if intersects
            //and that it is not intersecting with itself
            foreach (BasicPill pillPart in pillParts)
            {
                if (pillPart != null)
                    if (collisionRectToCheck.Intersects(pillPart.GetCollisionRect()))
                        if (pillToCheck != pillPart)
                            return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a pill at a point on the board will drop given a recent line completion
        /// </summary>
        /// <param name="pillBoardPosition">The position to check</param>
        /// <returns>Returns true if the pill will drop</returns>
        private bool WillDrop(Point pillBoardPosition)
        {
            return WillDrop(pillBoardPosition, true);
        }

        /// <summary>
        /// Check if a pill at a point on the board will drop given a recent line completion
        /// </summary>
        /// <param name="pillBoardPosition">The position to check</param>
        /// <param name="checkConnectedPill">Whether or not to check the pill's connected pill for anchoring</param>
        /// <returns>Returns true if the pill will drop</returns>
        private bool WillDrop(Point pillBoardPosition, bool checkConnectedPill)
        {
            //first check if we are at the bottom of the board, if so, pill will not drop
            if (pillBoardPosition.Y + 1 >= boardHeight)
                return false;
            //to see if pill will drop, need to see if there is a pill below it

            BasicPill thisPill = pillParts[pillBoardPosition.X, pillBoardPosition.Y];
            BasicPill pillBelow = pillParts[pillBoardPosition.X, pillBoardPosition.Y + 1];
            BasicPill connectedPill = thisPill.ConnectedPill;

            //if this pill is immobile it will not drop
            if (thisPill.Immobile)
                return false;

            if (pillBelow == null)
            {
                if (checkConnectedPill)
                {
                    if (connectedPill == null)
                    {
                        //this pill is now freed so set state to dropping and remove it 
                        //from the pillParts array (we will add it again when it contacts)
                        FreePill(thisPill);
                        return true;
                    }
                    //pill won't drop if connected to pill that won't drop
                    else
                    {
                        //first make sure we're not looking above us
                        if (connectedPill.position.Y >= thisPill.position.Y)
                        {
                            //check if the connected pill is freed or will be dropping
                            if (freedPills.Contains(connectedPill) || WillDrop(ScreenToBoard(connectedPill.position), false))
                            {
                                //it will so add this one to freed pills list
                                FreePill(thisPill);
                                return true;
                            }
                            else
                                //it won't be dopping so this one won't either
                                return false;
                        }
                        //we are looking above us so we have no pill below us and no anchored pill
                        else
                        {
                            FreePill(thisPill);
                            return true;
                        }

                    }
                }
                //not checking connected pill and no pill below so will fall
                else
                {
                    FreePill(thisPill);
                    return true;
                }

            }
            else
            {
                //if pill below is immobile then pill will not drop
                if (pillBelow.Immobile)
                    return false;

                //?check connected pills (unless that pill is above this pill) 
                //i.e. it's vertical and we're checking the bottom one
                //in that case have to find out if this pill will drop before can determine the next one
                if (checkConnectedPill && connectedPill != null && connectedPill.position.Y <= thisPill.position.Y)
                {
                    //check if the connected pill is freed or will be dropping
                    if (freedPills.Contains(connectedPill) || WillDrop(ScreenToBoard(connectedPill.position), false))
                    {
                        //it will so add this one to freed pills list
                        FreePill(thisPill);
                        return true;
                    }
                    else
                        //it won't be dopping so this one won't either
                        return false;
                }

                //if pill below is freed then this pill will drop
                if (freedPills.Contains(pillBelow))
                {
                    FreePill(thisPill);
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Checks if a given pill will still be in bounds after a given offset
        /// </summary>
        /// <param name="pillToCheck">The IBasicPill to check</param>
        /// <param name="offset">The direction and distance to move</param>
        /// <returns>Returns true if the move is valid and will not push pill off board</returns>
        private bool WillStayInBounds(IBasicPill pillToCheck, Point offset)
        {
            Rectangle boardRectangle = PlayingAreaConstants.BoardArea;// new Rectangle(PlayingAreaConstants.LeftEdge,
            //PlayingAreaConstants.TopEdge, PlayingAreaConstants.Width, PlayingAreaConstants.Height);
            Rectangle collisionRectToCheck = pillToCheck.GetCollisionRect();
            collisionRectToCheck.Offset(offset);

            //check that after move the rectangle will still be in bounds
            return boardRectangle.Contains(collisionRectToCheck);
        }
    }
}