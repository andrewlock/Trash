using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Trash
{
    /// <summary>
    /// Simple Interface for manipulating pills
    /// </summary>
    interface IBasicPill
    {
        /// <summary>
        /// The collsion rectangle bounding the pill
        /// </summary>
        /// <returns>A rectangle representing the collision rectangle</returns>
        Rectangle GetCollisionRect();

        /// <summary>
        /// Moves the pill in the specified direction
        /// </summary>
        /// <param name="direction">The direction to move the pill</param>
        void Move(Vector2 direction);

        /// <summary>
        /// Rotate the pill through an angle
        /// </summary>
        /// <param name="angle">The angle in radians to rotate</param>
        void Rotate(float angle);
    }

    /// <summary>
    /// A simple, single square, basic object (pill part or germ)
    /// </summary>
    [DebuggerDisplay("Color ={color}, Pos ={(int)((position.X-PlayingAreaConstants.LeftEdge)/width)},{(int)((position.Y-PlayingAreaConstants.TopEdge)/height)}, Immobile ={Immobile}")]
    class BasicPill : IBasicPill
    {
        //dimensions, location, orientation and color
        public const int width = 24;
        public const int height = 24;
        public Vector2 position { get; private set; }
        public float rotation { get; private set; }
        public SpriteEffects flipType = SpriteEffects.None;
        public Color color { get; private set; }

        //A pill associated with this pill
        public BasicPill ConnectedPill { get; set; }
        //The pill will not drop when unsupported
        public bool Immobile = false;
        //The score achieved by eliminating this pill
        public int score { get; private set; }

        //Animation fields for different states
        public PillAnimationState animationState { get; protected set; }
        public PillDroppingState droppingState = PillDroppingState.Ready;
        Texture2D noneTexture;
        Point noneFrameSize;
        Point noneSheetSize;
        Texture2D breakingTexture;
        Point breakingFrameSize;
        Point breakingSheetSize;
        Texture2D brokenTexture;
        Point brokenFrameSize;
        Point brokenSheetSize;
        Texture2D randomTexture;
        Point randomFrameSize;
        Point randomSheetSize;
        Texture2D disconnectedTexture;
        Point disconnectedFrameSize;
        Point disconnectedSheetSize;

        //Animation fields for current state
        Texture2D currentTexture;
        Point currentFrameSize;
        Point currentFrame;
        Point sheetSize;
        int timeSinceLastFrame = 0;
        int millisecondsPerFrame;
        const int defaultMillisecondsPerFrame = 16;

        /// <summary>
        /// Creates a basic pill
        /// </summary>
        /// <param name="noneTexture">The texture to use when not animating</param>
        /// <param name="noneFrameSize">The frame size of the no animation texture</param>
        /// <param name="noneSheetSize">The sheet size of the no animation texture</param>
        /// <param name="breakingTexture">The texture to use when the pill is in the breaking state</param>
        /// <param name="breakingFrameSize">The frame size of the breaking texture</param>
        /// <param name="breakingSheetSize">The sheet size of the breaking texture</param>
        /// <param name="brokenTexture">The texture to use when the pill is in the broken state</param>
        /// <param name="brokenFrameSize">The frame size of the broken texture</param>
        /// <param name="brokenSheetSize">The sheet size of the broken texture</param>
        /// <param name="randomTexture">The texture to use when the pill is in the random animation state</param>
        /// <param name="randomFrameSize">The frame size of the random texture</param>
        /// <param name="randomSheetSize">The sheet size of the random texture</param>
        /// <param name="disconnectedTexture">The texture to use when the pill becomes disconnected</param>
        /// <param name="disconnectedFrameSize">The frame size of the disconnected texture</param>
        /// <param name="disconnectedSheetSize">The sheet size of the disconnected texture</param>
        /// <param name="position">The start position of the pill</param>
        /// <param name="color">The color of the pill</param>
        /// <param name="rotation">The initial rotation in radians of the pill</param>
        /// <param name="flipType">Whether to flip the pill of not</param>
        /// <param name="score">The score value of the pill obtained when it is broken</param>
        /// <param name="millisecondsPerFrame">The number of miliseconds between frames</param>
        public BasicPill(Texture2D noneTexture, Point noneFrameSize, Point noneSheetSize, Texture2D breakingTexture, Point breakingFrameSize, Point breakingSheetSize,
            Texture2D brokenTexture, Point brokenFrameSize, Point brokenSheetSize, Texture2D randomTexture, Point randomFrameSize, Point randomSheetSize,
            Texture2D disconnectedTexture, Point disconnectedFrameSize, Point disconnectedSheetSize,
            Vector2 position, Color color, float rotation, SpriteEffects flipType, int score, int millisecondsPerFrame)
        {
            this.noneTexture = noneTexture;
            this.noneFrameSize = noneFrameSize;
            this.noneSheetSize = noneSheetSize;
            this.breakingTexture = breakingTexture;
            this.breakingFrameSize = breakingFrameSize;
            this.breakingSheetSize = breakingSheetSize;
            this.brokenTexture = brokenTexture;
            this.brokenFrameSize = brokenFrameSize;
            this.brokenSheetSize = brokenSheetSize;
            this.randomTexture = randomTexture;
            this.randomFrameSize = randomFrameSize;
            this.randomSheetSize = randomSheetSize;
            this.disconnectedTexture = disconnectedTexture;
            this.disconnectedFrameSize = disconnectedFrameSize;
            this.disconnectedSheetSize = disconnectedSheetSize;
            this.position = position;
            this.color = color;
            this.rotation = rotation;
            this.flipType = flipType;
            this.score = score;
            this.millisecondsPerFrame = millisecondsPerFrame;
            ChangeState(PillAnimationState.None);
            ConnectedPill = null;
        }

        /// <summary>
        /// Creates a basic pill with the default miliseconds per pill
        /// </summary>
        /// <param name="noneTexture">The texture to use when not animating</param>
        /// <param name="noneFrameSize">The frame size of the no animation texture</param>
        /// <param name="noneSheetSize">The sheet size of the no animation texture</param>
        /// <param name="breakingTexture">The texture to use when the pill is in the breaking state</param>
        /// <param name="breakingFrameSize">The frame size of the breaking texture</param>
        /// <param name="breakingSheetSize">The sheet size of the breaking texture</param>
        /// <param name="brokenTexture">The texture to use when the pill is in the broken state</param>
        /// <param name="brokenFrameSize">The frame size of the broken texture</param>
        /// <param name="brokenSheetSize">The sheet size of the broken texture</param>
        /// <param name="randomTexture">The texture to use when the pill is in the random animation state</param>
        /// <param name="randomFrameSize">The frame size of the random texture</param>
        /// <param name="randomSheetSize">The sheet size of the random texture</param>
        /// <param name="disconnectedTexture">The texture to use when the pill becomes disconnected</param>
        /// <param name="disconnectedFrameSize">The frame size of the disconnected texture</param>
        /// <param name="disconnectedSheetSize">The sheet size of the disconnected texture</param>
        /// <param name="position">The start position of the pill</param>
        /// <param name="color">The color of the pill</param>
        /// <param name="rotation">The initial rotation in radians of the pill</param>
        /// <param name="flipType">Whether to flip the pill of not</param>
        /// <param name="score">The score value of the pill obtained when it is broken</param>
        public BasicPill(Texture2D noneTexture, Point noneFrameSize, Point noneSheetSize, Texture2D breakingTexture, Point breakingFrameSize, Point breakingSheetSize,
            Texture2D brokenTexture, Point brokenFrameSize, Point brokenSheetSize, Texture2D randomTexture, Point randomFrameSize, Point randomSheetSize,
            Texture2D disconnectedTexture, Point disconnectedFrameSize, Point disconnectedSheetSize,
            Vector2 position, Color color, float rotation, SpriteEffects flipType, int score)
            : this(noneTexture, noneFrameSize, noneSheetSize, breakingTexture, breakingFrameSize, breakingSheetSize,
             brokenTexture, brokenFrameSize, brokenSheetSize, randomTexture, randomFrameSize, randomSheetSize,
             disconnectedTexture, disconnectedFrameSize, disconnectedSheetSize,
             position, color, rotation, flipType, score, defaultMillisecondsPerFrame)
        {
            this.millisecondsPerFrame = defaultMillisecondsPerFrame;
        }

        /// <summary>
        /// Update the animation of the pill
        /// </summary>
        /// <param name="gameTime">The current gametime</param>
        public void Update(GameTime gameTime)
        {
            if (animationState == PillAnimationState.Breaking)
                UpdateBreaking(gameTime);
            if (animationState == PillAnimationState.Random)
                UpdateRandom(gameTime);

        }

        /// <summary>
        /// Update the breaking animation
        /// </summary>
        /// <param name="gameTime">The current gametime</param>
        private void UpdateBreaking(GameTime gameTime)
        {
            //if enough time has elapsed, cycle through the sprite sheet
            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                timeSinceLastFrame = 0;
                ++currentFrame.X;
                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;
                    ++currentFrame.Y;
                    //we have reached the last image so stop and set animation state to gone
                    if (currentFrame.Y >= sheetSize.Y)
                        ChangeState(PillAnimationState.Gone);
                }
            }
        }

        /// <summary>
        /// Update the random animation
        /// </summary>
        /// <param name="gameTime">The current gametime</param>
        private void UpdateRandom(GameTime gameTime)
        {
            //if enough time has elapsed, cycle through the sprite sheet
            timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (timeSinceLastFrame > millisecondsPerFrame)
            {
                timeSinceLastFrame = 0;
                ++currentFrame.X;
                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;
                    ++currentFrame.Y;
                    //we have reached the last image so stop and set animation state to none
                    if (currentFrame.Y >= sheetSize.Y)
                        ChangeState(PillAnimationState.None);
                }
            }
        }

        /// <summary>
        /// Draw the pill, Spritebatch.Begin() must have already been called
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to use</param>
        public void Draw(ResizedSpriteBatch spriteBatch)
        {
            Vector2 center = new Vector2(currentFrameSize.X / 2, currentFrameSize.Y / 2);
            spriteBatch.Draw(currentTexture, position + center,
                new Rectangle(currentFrame.X * currentFrameSize.X, currentFrame.Y * currentFrameSize.Y, currentFrameSize.X, currentFrameSize.Y),
                color, rotation, center, 1, flipType, 0);
        }

        /// <summary>
        /// Change the current state of the pill
        /// </summary>
        /// <param name="state">The new state to change to</param>
        public void ChangeState(PillAnimationState state)
        {
            animationState = state;
            currentFrame = new Point(0, 0);
            switch (state)
            {
                case PillAnimationState.None:
                    currentTexture = noneTexture;
                    currentFrameSize = noneFrameSize;
                    sheetSize = noneSheetSize;
                    break;
                case PillAnimationState.Gone:
                    currentTexture = brokenTexture;
                    currentFrameSize = brokenFrameSize;
                    sheetSize = brokenSheetSize;
                    break;
                case PillAnimationState.Breaking:
                    currentTexture = breakingTexture;
                    currentFrameSize = breakingFrameSize;
                    sheetSize = breakingSheetSize;
                    rotation = 0;
                    break;
                case PillAnimationState.Random:
                    currentTexture = randomTexture;
                    currentFrameSize = randomFrameSize;
                    sheetSize = randomSheetSize;
                    break;
            }

        }

        /// <summary>
        /// Move the position of the pill
        /// </summary>
        /// <param name="direction">The direction (and distance in pixels) to move</param>
        public void Move(Vector2 direction)
        {
            position += direction;
        }

        /// <summary>
        /// Rotate the pill
        /// </summary>
        /// <param name="angle">The angle in radians to rotate the pill, positive clockwise</param>
        public void Rotate(float angle)
        {
            rotation += angle;
            MathHelper.WrapAngle(rotation);
        }

        /// <summary>
        /// Get a rectangle representing the collision rectangle of the pill
        /// </summary>
        /// <returns>The bounding rectangle</returns>
        public Rectangle GetCollisionRect()
        {
            return new Rectangle((int)position.X, (int)position.Y, width, height);
        }

        /// <summary>
        /// Disconnect the pill from its connected pill
        /// </summary>
        public void Disconnect()
        {
            noneTexture = disconnectedTexture;
            noneFrameSize = disconnectedFrameSize;
            noneSheetSize = disconnectedSheetSize;
            ConnectedPill = null;
            //reset rotation to look nicer
            rotation = 0;
            //need to refresh state too
            ChangeState(animationState);
        }
    }

    /// <summary>
    /// Represents a pill that will be dropped by the user
    /// Consists of 2 BasicPill parts and a rotation value
    /// </summary>
    class DoublePill : IBasicPill
    {
        public BasicPill leftPillPart { get; private set; }
        public BasicPill rightPillPart { get; private set; }

        //clockwise rotation
        public float rotation { get; private set; }
        public Vector2 position
        {
            get { return leftPillPart.position; }
        }

        /// <summary>
        /// Create a double pill
        /// </summary>
        /// <param name="noneTexture">The texture to use when not animating</param>
        /// <param name="noneFrameSize">The frame size of the no animation texture</param>
        /// <param name="noneSheetSize">The sheet size of the no animation texture</param>
        /// <param name="breakingTexture">The texture to use when the pill is in the breaking state</param>
        /// <param name="breakingFrameSize">The frame size of the breaking texture</param>
        /// <param name="breakingSheetSize">The sheet size of the breaking texture</param>
        /// <param name="brokenTexture">The texture to use when the pill is in the broken state</param>
        /// <param name="brokenFrameSize">The frame size of the broken texture</param>
        /// <param name="brokenSheetSize">The sheet size of the broken texture</param>
        /// <param name="randomTexture">The texture to use when the pill is in the random animation state</param>
        /// <param name="randomFrameSize">The frame size of the random texture</param>
        /// <param name="randomSheetSize">The sheet size of the random texture</param>
        /// <param name="disconnectedTexture">The texture to use when the pill becomes disconnected</param>
        /// <param name="disconnectedFrameSize">The frame size of the disconnected texture</param>
        /// <param name="disconnectedSheetSize">The sheet size of the disconnected texture</param>
        /// <param name="position">The start position of the left pill of the double pill</param>
        /// <param name="leftColor">The color of the left pill</param>
        /// <param name="rightColor">The color of the right pill</param>
        /// <param name="score">The score value of each basic pill when they are broken</param>
        /// <param name="millisecondsPerFrame">The number of miliseconds between frames</param>
        public DoublePill(Texture2D noneTexture, Point noneFrameSize, Point noneSheetSize, Texture2D breakingTexture, Point breakingFrameSize, Point breakingSheetSize,
            Texture2D brokenTexture, Point brokenFrameSize, Point brokenSheetSize, Texture2D randomTexture, Point randomFrameSize, Point randomSheetSize,
            Texture2D disconnectedTexture, Point disconnectedFrameSize, Point disconnectedSheetSize,
            Vector2 position, Color leftColor, Color rightColor, int score, int millisecondsPerFrame)
        {
            leftPillPart = new BasicPill(noneTexture, noneFrameSize, noneSheetSize, breakingTexture, breakingFrameSize, breakingSheetSize,
                brokenTexture, brokenFrameSize, brokenSheetSize, randomTexture, randomFrameSize, randomSheetSize,
                disconnectedTexture, disconnectedFrameSize, disconnectedSheetSize,
                position, leftColor, 0, SpriteEffects.None, score, millisecondsPerFrame);

            rightPillPart = new BasicPill(noneTexture, noneFrameSize, noneSheetSize, breakingTexture, breakingFrameSize, breakingSheetSize,
                brokenTexture, brokenFrameSize, brokenSheetSize, randomTexture, randomFrameSize, randomSheetSize,
                disconnectedTexture, disconnectedFrameSize, disconnectedSheetSize,
                new Vector2(position.X + BasicPill.width, position.Y), rightColor, 0, SpriteEffects.FlipHorizontally, score, millisecondsPerFrame);
            rotation = 0;

            leftPillPart.ConnectedPill = rightPillPart;
            rightPillPart.ConnectedPill = leftPillPart;
        }

        /// <summary>
        /// Create a double pill using the default milliseconds per frame
        /// </summary>
        /// <param name="noneTexture">The texture to use when not animating</param>
        /// <param name="noneFrameSize">The frame size of the no animation texture</param>
        /// <param name="noneSheetSize">The sheet size of the no animation texture</param>
        /// <param name="breakingTexture">The texture to use when the pill is in the breaking state</param>
        /// <param name="breakingFrameSize">The frame size of the breaking texture</param>
        /// <param name="breakingSheetSize">The sheet size of the breaking texture</param>
        /// <param name="brokenTexture">The texture to use when the pill is in the broken state</param>
        /// <param name="brokenFrameSize">The frame size of the broken texture</param>
        /// <param name="brokenSheetSize">The sheet size of the broken texture</param>
        /// <param name="randomTexture">The texture to use when the pill is in the random animation state</param>
        /// <param name="randomFrameSize">The frame size of the random texture</param>
        /// <param name="randomSheetSize">The sheet size of the random texture</param>
        /// <param name="disconnectedTexture">The texture to use when the pill becomes disconnected</param>
        /// <param name="disconnectedFrameSize">The frame size of the disconnected texture</param>
        /// <param name="disconnectedSheetSize">The sheet size of the disconnected texture</param>
        /// <param name="position">The start position of the left pill of the double pill</param>
        /// <param name="leftColor">The color of the left pill</param>
        /// <param name="rightColor">The color of the right pill</param>
        /// <param name="score">The score value of each basic pill when they are broken</param>
        public DoublePill(Texture2D noneTexture, Point noneFrameSize, Point noneSheetSize, Texture2D breakingTexture, Point breakingFrameSize, Point breakingSheetSize,
            Texture2D brokenTexture, Point brokenFrameSize, Point brokenSheetSize, Texture2D randomTexture, Point randomFrameSize, Point randomSheetSize,
            Texture2D disconnectedTexture, Point disconnectedFrameSize, Point disconnectedSheetSize,
            Vector2 position, Color leftColor, Color rightColor, int score)
        {
            leftPillPart = new BasicPill(noneTexture, noneFrameSize, noneSheetSize, breakingTexture, breakingFrameSize, breakingSheetSize,
                brokenTexture, brokenFrameSize, brokenSheetSize, randomTexture, randomFrameSize, randomSheetSize,
                    disconnectedTexture, disconnectedFrameSize, disconnectedSheetSize,
                position, leftColor, 0, SpriteEffects.None, score);

            rightPillPart = new BasicPill(noneTexture, noneFrameSize, noneSheetSize, breakingTexture, breakingFrameSize, breakingSheetSize,
                brokenTexture, brokenFrameSize, brokenSheetSize, randomTexture, randomFrameSize, randomSheetSize,
                disconnectedTexture, disconnectedFrameSize, disconnectedSheetSize,
                new Vector2(position.X + BasicPill.width, position.Y), rightColor, 0, SpriteEffects.FlipHorizontally, score);
            rotation = 0;

            leftPillPart.ConnectedPill = rightPillPart;
            rightPillPart.ConnectedPill = leftPillPart;
        }

        /// <summary>
        /// Move the position of the pill
        /// </summary>
        /// <param name="direction">The direction (and distance in pixels) to move</param>
        public void Move(Vector2 direction)
        {
            leftPillPart.Move(direction);
            rightPillPart.Move(direction);
        }

        /// <summary>
        /// Rotate the pill
        /// </summary>
        /// <param name="angle">The angle in radians to rotate the pill, positive clockwise</param>
        public void Rotate(float angle)
        {
            bool direction = Math.Sign(angle) > 0;

            //on the same row
            if (leftPillPart.position.Y == rightPillPart.position.Y)
            {
                if (leftPillPart.position.X < rightPillPart.position.X)
                {
                    if (direction)
                    {
                        leftPillPart.Move(new Vector2(0, -BasicPill.height));
                        rightPillPart.Move(new Vector2(-BasicPill.width, 0));
                    }
                    else
                        rightPillPart.Move(new Vector2(-BasicPill.width, -BasicPill.height));
                }
                else
                {
                    if (direction)
                    {
                        rightPillPart.Move(new Vector2(0, -BasicPill.height));
                        leftPillPart.Move(new Vector2(-BasicPill.width, 0));
                    }
                    else
                        leftPillPart.Move(new Vector2(-BasicPill.width, -BasicPill.height));

                }
            }
            else
            {
                if (leftPillPart.position.Y > rightPillPart.position.Y)
                    if (direction)
                        rightPillPart.Move(new Vector2(BasicPill.width, BasicPill.height));
                    else
                    {
                        rightPillPart.Move(new Vector2(0, BasicPill.height));
                        leftPillPart.Move(new Vector2(BasicPill.width, 0));
                    }
                else
                    if (direction)
                        leftPillPart.Move(new Vector2(BasicPill.width, BasicPill.height));
                    else
                    {
                        leftPillPart.Move(new Vector2(0, BasicPill.height));
                        rightPillPart.Move(new Vector2(BasicPill.width, 0));
                    }
            }

            //now rotate maintaining the position of this pill (even if actually pill swaps)
            leftPillPart.Rotate(angle);
            rightPillPart.Rotate(angle);
        }

        /// <summary>
        /// Draw the pill, Spritebatch.Begin() must have already been called
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to use</param>
        public void Draw(ResizedSpriteBatch spriteBatch)
        {
            leftPillPart.Draw(spriteBatch);
            rightPillPart.Draw(spriteBatch);
        }

        /// <summary>
        /// Get a rectangle representing the collision rectangle of the pill
        /// </summary>
        /// <returns>The bounding rectangle</returns>
        public Rectangle GetCollisionRect()
        {
            return Rectangle.Union(leftPillPart.GetCollisionRect(), rightPillPart.GetCollisionRect());
        }
    }
}

