using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Trash
{
    /// <summary>
    /// Contains details relating to a current player's board setup
    /// </summary>
    public class BoardSetupDetails
    {

        public CursorLocation cursorLocation = CursorLocation.OnSolo;
        private int difficulty = 2;
        private int level = 0;
        public SelectionScreen selectionScreen = SelectionScreen.PlayerNumbers;
        public Viewport viewport;

        /// <summary>
        /// Get and set the current difficulty, limiting to only available levels
        /// </summary>
        public int currentDifficulty
        {
            get { return difficulty; }
            set
            {
                if (value < 0)
                    difficulty = 0;
                else if (value >= TrashGame.difficultyInfoList.levels.Count)
                    difficulty = TrashGame.difficultyInfoList.levels.Count - 1;
                else
                    difficulty = value;
            }
        }

        /// <summary>
        /// Get and set the current level, limiting to only available levels
        /// </summary>
        public int currentLevel
        {
            get { return level; }
            set
            {
                if (value < 0)
                    level = 0;
                else if (value >= TrashGame.levelInfoList.levels.Count)
                    level = TrashGame.levelInfoList.levels.Count - 1;
                else
                    level = value;
            }
        }

        /// <summary>
        /// default Constructor
        /// </summary>
        /// 
        public BoardSetupDetails()
        {
        }
    }

    /// <summary>
    /// A class to encapsulate a player's screen and game play
    /// Includes variables required to manipulate a players screen
    /// </summary>
    public class PlayerDetails
    {

        public GameBoard gameBoard;
        public int GameOverDisplayTime = 0;
        public GameState gameState = GameState.None;
        public GameType gameType = GameType.None;
        public bool isExiting = false;
        public bool isPaused = false;

        //Name Using highScoreLetter indexes
        public int[] Name = new int[3] { 0, 0, 0 };
        public int score = 0;
        public int wins = 0;

        /// <summary>
        /// the Board Setup details associated with this player
        /// </summary>
        public BoardSetupDetails boardSetupDetails { get; private set; }

        /// <summary>
        /// The inputHelper associated with this player
        /// </summary>
        public InputHelper inputHelper { get; private set; }

        /// <summary>
        /// The viewport associated with this player
        /// </summary>
        public Viewport viewport { get; set; }

        /// <summary>
        /// Construct a new player
        /// </summary>
        /// <param name="inputHelper">The assoicated inputHelper for the player</param>
        /// <param name="gameBoard">The associated GameBoad for the player</param>
        /// <param name="boardSetupDetails">The Associated BoardSetupDetails for the player</param>
        public PlayerDetails(InputHelper inputHelper, GameBoard gameBoard,
            BoardSetupDetails boardSetupDetails)
        {
            this.inputHelper = inputHelper;
            this.viewport = viewport;
            this.gameBoard = gameBoard;
            this.boardSetupDetails = boardSetupDetails;
            gameBoard.player = this;
        }
    }
}
