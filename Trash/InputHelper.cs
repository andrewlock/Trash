using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Trash
{
    /// <summary>
    /// A class which handles a single player's keyboard input, automatically updating it's state
    /// </summary>
    public class InputHelper : GameComponent
    {

        bool backReleased;
        bool downReleased;
        bool enterReleased;
        bool leftReleased;
        bool pauseReleased;
        bool rightReleased;
        bool upReleased;

        bool ignoreEnterInput = false;
        KeyboardState state;

        public bool allowMultipleEnterPresses { get; set; }
        public Keys backKey { get; private set; }
        public Keys downKey { get; private set; }
        public Keys enterKey { get; private set; }
        public Keys leftKey { get; private set; }
        public Keys pauseKey { get; private set; }
        public Keys rightKey { get; private set; }
        public Keys upKey { get; private set; }


        /// <summary>
        /// Create an input helper
        /// </summary>
        /// <param name="game">The parent game</param>
        /// <param name="upKey">The key to use for Up</param>
        /// <param name="downKey">The key to use for Down</param>
        /// <param name="leftKey">The key to use for Left</param>
        /// <param name="rightKey">The key to use for Right</param>
        /// <param name="enterKey">The key to use for Enter</param>
        /// <param name="backKey">The key to use for Back</param>
        /// <param name="pauseKey">The key to use for Pause</param>
        public InputHelper(Game game, Keys upKey, Keys downKey, Keys leftKey, 
            Keys rightKey, Keys enterKey, Keys backKey, Keys pauseKey)
            : base(game)
        {
            this.upKey = upKey;
            this.downKey = downKey;
            this.leftKey = leftKey;
            this.rightKey = rightKey;
            this.enterKey = enterKey;
            this.backKey = backKey;
            this.pauseKey = pauseKey;
            allowMultipleEnterPresses = false;
        }

        /// <summary>
        /// Sets ignoreInput = true, preventing the first enter press from registering multiple times
        /// </summary>
        public void IgnoreSingleEnterPress()
        {
            ignoreEnterInput = true;
        }

        //these functions work as single shot items with the exception
        //of enter when allowMultipleEnterPresses = true;

        public bool IsBackPressed()
        {
            if (state.IsKeyDown(backKey) && backReleased)
            {
                backReleased = false;
                return true;
            }
            return false;
        }

        public bool IsDownPressed()
        {
            if (state.IsKeyDown(downKey) && downReleased)
            {
                downReleased = false;
                return true;
            }
            return false;
        }

        public bool IsEnterPressed()
        {
            if (state.IsKeyDown(enterKey) && !ignoreEnterInput && (enterReleased || allowMultipleEnterPresses))
            {
                enterReleased = false;
                ignoreEnterInput = false;
                return true;
            }
            return false;
        }

        public bool IsLeftPressed()
        {
            if (state.IsKeyDown(leftKey) && leftReleased)
            {
                leftReleased = false;
                return true;
            }
            return false;
        }

        public bool IsPausePressed()
        {
            if (state.IsKeyDown(pauseKey) && pauseReleased)
            {
                pauseReleased = false;
                return true;
            }
            return false;
        }

        public bool IsRightPressed()
        {
            if (state.IsKeyDown(rightKey) && rightReleased)
            {
                rightReleased = false;
                return true;
            }
            return false;
        }

        public bool IsUpPressed()
        {
            if (state.IsKeyDown(upKey) && upReleased)
            {
                upReleased = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the keyboard state
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing</param>
        public override void Update(GameTime gameTime)
        {
            state = Keyboard.GetState();
            if (!upReleased)
                upReleased = state.IsKeyUp(upKey);
            if (!downReleased)
                downReleased = state.IsKeyUp(downKey);
            if (!leftReleased)
                leftReleased = state.IsKeyUp(leftKey);
            if (!rightReleased)
                rightReleased = state.IsKeyUp(rightKey);
            if (!backReleased)
                backReleased = state.IsKeyUp(backKey);
            if (!pauseReleased)
                pauseReleased = state.IsKeyUp(pauseKey);
            if (!enterReleased)
                enterReleased = state.IsKeyUp(enterKey);
            if (ignoreEnterInput)
                ignoreEnterInput = !state.IsKeyUp(enterKey);

            base.Update(gameTime);
        }
    }
}
