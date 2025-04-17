using System.Collections.Generic;

namespace Trash
{
    /// <summary>
    /// A list of all the levels associated with the game
    /// </summary>
    public class LevelSettings
    {
        public List<LevelInfo> levels = new List<LevelInfo>();

        /// <summary>
        /// The default constructor populating the level list
        /// </summary>
        public LevelSettings()
        {
            levels.Add(new LevelInfo(5, 7, 1, 1, 1));
            levels.Add(new LevelInfo(5, 7, 2, 1, 1));
            levels.Add(new LevelInfo(5, 7, 3, 1, 1));
            levels.Add(new LevelInfo(5, 7, 4, 1, 1));
            levels.Add(new LevelInfo(5, 7, 5, 1, 1));
            levels.Add(new LevelInfo(6, 8, 5, 1, 1));
            levels.Add(new LevelInfo(6, 8, 5, 1, 1));
            levels.Add(new LevelInfo(6, 9, 6, 1, 1));
            levels.Add(new LevelInfo(6, 9, 7, 1, 1));
            levels.Add(new LevelInfo(7, 9, 7, 1, 1));
            levels.Add(new LevelInfo(6, 9, 8, 1, 1));
            levels.Add(new LevelInfo(7, 9, 8, 1, 1));
            levels.Add(new LevelInfo(7, 10, 8, 1, 1));
            levels.Add(new LevelInfo(7, 10, 9, 1, 1));
            levels.Add(new LevelInfo(8, 10, 10, 1, 1));
            levels.Add(new LevelInfo(9, 10, 10, 1, 1));
                
        }
    }

    /// <summary>
    /// A list of all the difficulty levels associated with the game
    /// </summary>
    public class DifficultySettings
    {
        public List<DifficultlyInfo> levels = new List<DifficultlyInfo>();

        /// <summary>
        /// The default constructor populating the difficulty level list
        /// </summary>

        public DifficultySettings()
        {
            levels.Add(new DifficultlyInfo(2f, 1f, @"Very Easy"));
            levels.Add(new DifficultlyInfo(1.5f, 1f, @"Easy"));
            levels.Add(new DifficultlyInfo(1f, 1f, @"Normal"));
            levels.Add(new DifficultlyInfo(0.75f, 1f, @"Hard"));
            levels.Add(new DifficultlyInfo(0.5f, 1f, @"Very Hard"));

        }
    }

}
