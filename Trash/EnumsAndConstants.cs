using System;
using Microsoft.Xna.Framework;

namespace Trash
{
    //Various enums used in the game
    public enum PillAnimationState { None, Random, Breaking, Gone };
    public enum PillDroppingState { Ready, InNeck, InBottle, Contact, ProcessingLines };
    public enum CursorLocation { OnLevel, OnDifficulty, OnSolo, OnLink, OnStart, OnSettings, OnLetter1, OnLetter2, OnLetter3, OnLettersOk, OnExit };
    public enum SelectionScreen { PlayerNumbers, LevelAndDifficulty, EnterHighScore };
    public enum GameState { None, Started, SelectSettings, LevelChange, ReadyToContinue, InPlay, GameOver, GameComplete, EnterHighScore, Credits };
    public enum GameType { None, Solo, Link };

    /// <summary>
    /// A list of the scores assigned to each pill type
    /// </summary>
    class PillScores
    {
        public const int GermScore = 20;
        public const int PillScore = 10;
        public const int TrashScore = 10;
    }

    /// <summary>
    /// A collection of all the playing area constants used throughout the game
    /// </summary>
    public class PlayingAreaConstants
    {
        static float scale = 1;
        static Vector2 offset = Vector2.Zero;
        static int leftEdge = 200;
        static int topEdge = 312;
        static int width = 240;
        static int height = 360;
        static int rightEdge = leftEdge + width;
        static int bottomEdge = topEdge + height;
        static Vector2 topOfNeck = new Vector2(296, 252);
        static Vector2 levelLocation = new Vector2(544, 385);
        static Vector2 difficultyLocation = new Vector2(544, 571);
        static Vector2 scoreLocation = new Vector2(86, 385);
        static Vector2 winsLocation = new Vector2(90, 571);
        static Vector2 boardCentre = new Vector2(leftEdge + width / 2, topEdge + height / 2);

        static Vector2 p1Location = new Vector2(leftEdge + width / 2, (topEdge + height / 2) - 30);
        static Vector2 p2Location = new Vector2(leftEdge + width / 2, (topEdge + height / 2) + 30);
        static Vector2 levelTextLocation = new Vector2(leftEdge + width / 2, topEdge + 50);
        static Vector2 levelChangeLocation = new Vector2(leftEdge + width / 2, topEdge + 90);
        static Vector2 difficultyTextLocation = new Vector2(leftEdge + width / 2, topEdge + 190);
        static Vector2 difficultyChangeLocation = new Vector2(leftEdge + width / 2, topEdge + 230);
        static Vector2 startLocation = new Vector2(leftEdge + width / 2, bottomEdge - 30);
        static Vector2 settingsLocation = new Vector2(leftEdge + width / 2, topEdge + 150);
        static Vector2 exitLocation = startLocation;
        static Vector2 highScoreLabelLocation = new Vector2(leftEdge + width / 2, topEdge + height / 2 - 100);
        static Vector2 highScoreLocation = boardCentre;
        static Vector2 highScoreNameLocation = new Vector2(leftEdge + width / 2, topEdge + height / 2 + 100);

        public static int LeftEdge { get { return leftEdge; } }
        public static int TopEdge { get { return topEdge; } }
        public static int Width { get { return width; } }
        public static int Height { get { return height; } }
        public static int RightEdge { get { return rightEdge; } }
        public static int BottomEdge { get { return bottomEdge; } }
        public static Vector2 TopOfNeck { get { return topOfNeck; } }
        public static Vector2 BoardCentre { get { return boardCentre; } }

        public static Vector2 LevelLocation { get { return levelLocation; } }
        public static Vector2 DifficultyLocation { get { return difficultyLocation; } }
        public static Vector2 ScoreLocation { get { return scoreLocation; } }
        public static Vector2 WinsLocation { get { return winsLocation; } }
        public static Vector2 P1Location { get { return p1Location; } }
        public static Vector2 P2Location { get { return p2Location; } }
        public static Vector2 LevelTextLocation { get { return levelTextLocation; } }
        public static Vector2 LevelChangeLocation { get { return levelChangeLocation; } }
        public static Vector2 DifficultyTextLocation { get { return difficultyTextLocation; } }
        public static Vector2 DifficultyChangeLocation { get { return difficultyChangeLocation; } }
        public static Vector2 StartLocation { get { return startLocation; } }
        public static Vector2 SettingsLocation { get { return settingsLocation; } }
        public static Vector2 ExitLocation { get { return exitLocation; } }
        public static Vector2 HighScoreLabelLocation { get { return highScoreLabelLocation; } }
        public static Vector2 HighScoreLocation { get { return highScoreLocation; } }
        public static Vector2 HighScoreNameLocation { get { return highScoreNameLocation; } }

        public static Rectangle BoardArea { get { return new Rectangle(leftEdge, topEdge, width, height); } }

        public static float Scale { get { return scale; } }
        public static Vector2 Offset { get { return offset; } }

        public static void Resize(int viewportWidth, int viewportHeight, int GameFullWidth, int GameFullheight)
        {
            // Scale is used to stretch or shrink the drawn images so that everything
            // is visible on screen.
            scale =
                Math.Min((float)viewportHeight / (float)GameFullheight,
                (float)viewportWidth / (float)GameFullWidth);
            // The offset used to center the drawn images on the screen
            offset =
                new Vector2((viewportWidth - GameFullWidth * scale) / 2,
                (viewportHeight - GameFullheight * scale) / 2);
        }

    };

}