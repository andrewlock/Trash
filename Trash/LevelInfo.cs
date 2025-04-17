namespace Trash
{
    /// <summary>
    /// class for holding details about a level
    /// </summary>
    public class LevelInfo
    {
        //number of germs per row
        public int minGermsPerRow;
        public int maxGermsPerRow;

        //number of rows of germs
        public int germsRows;

        //speed of pills and Trash
        public int pillSpeed;
        public int trashSpeed;

        /// <summary>
        /// Create a new LevelInfo Object
        /// </summary>
        /// <param name="minGermsPerRow">The minimum (inclusive) number of germs per row</param>
        /// <param name="maxGermsPerRow">The maximum (exclusive) number of germs per row</param>
        /// <param name="germsRows">The number of germ rows</param>
        /// <param name="pillSpeed">The relative speed of pills</param>
        /// <param name="trashSpeed">the relative speed of trash and falling pills</param>
        public LevelInfo(int minGermsPerRow, int maxGermsPerRow, int germsRows, int pillSpeed, int trashSpeed)
        {
            this.minGermsPerRow = minGermsPerRow;
            this.maxGermsPerRow = maxGermsPerRow;
            this.germsRows = germsRows;
            this.pillSpeed = pillSpeed;
            this.trashSpeed = trashSpeed;
        }
    }

    public class DifficultlyInfo
    {
        //speed multiplier
        public float pillSpeedModifier;
        public float trashSpeedModifier;
        public string name {get; private set;}
        public DifficultlyInfo(float pillSpeedModifier, float trashSpeedModifier, string name)
        {
            this.pillSpeedModifier = pillSpeedModifier;
            this.trashSpeedModifier = trashSpeedModifier;
            this.name = name;
        }

    }
}
