using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Trash
{
    /// <summary>
    /// A Class encapsulating an entry in the high score table
    /// </summary>
    [DebuggerDisplay("Score = {Score}, Name={Name}, Date={DateAchieved}")]
    public class HighScoreEntry : IComparable
    {

        public DateTime DateAchieved { get; set; }

        public string Name { get; set; }

        public int Score { get; set; }



        public HighScoreEntry(string name, int score)
        {
            Name = name;
            Score = score;
            DateAchieved = DateTime.Now;
        }

        public HighScoreEntry()
        {
            Name = "AAA";
            Score = 0;
            DateAchieved = DateTime.Now;
        }

        #region IComparable Members

        /// <summary>
        /// Compares two HighScoreEntries
        /// </summary>
        /// <param name="obj">Must be a HighScoreEntry</param>
        /// <returns>the comparison result based on scores</returns>
        public int CompareTo(object obj)
        {
            HighScoreEntry otherObj = (HighScoreEntry)obj;
            return this.Score.CompareTo(otherObj.Score);
        }

        #endregion
    }

    /// <summary>
    /// A descending inplementation of the case insensitive comparer
    /// </summary>
    /// <typeparam name="T">The type of object to compare</typeparam>
    public class DescendingComparer<T> : IComparer<T>
    {

        CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();



        public int Compare(T x, T y)
        {
            //do the comparison in reverse
            return comparer.Compare(y, x);
        }
    }
}
