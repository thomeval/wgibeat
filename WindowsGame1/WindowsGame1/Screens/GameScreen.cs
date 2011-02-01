using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WGiBeat.Helpers;
using WGiBeat.Managers;

namespace WGiBeat.Screens
{
    public abstract class GameScreen : GameComponent
    {        
        protected FiniteState State = new FiniteState(1);
        protected readonly GameCore Core;

        public GameScreen(GameCore core) : base(core)
        {
            Core = core;
        }

        /// <summary>
        /// Informs a GameScreen that a specific key has been pressed. Unless necessary, use PerformAction() instead.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public virtual void PerformKey(Keys key)
        {
            //Virtual so not all GameScreens need to implement it.
        }
        /// <summary>
        /// Informs a GameScreen that a specific button on a game controller has been pressed. Unless necessary,
        /// use PerformAction() instead.
        /// </summary>
        /// <param name="buttons">The button that was pressed,</param>
        /// <param name="controllerNumber">The player number of the controller (1-4).</param>
        public virtual void PerformButton(Buttons buttons, int controllerNumber)
        {
            //Virtual since it is optional for GameScreens.
        }

        /// <summary>
        /// Executes when the GameScreen should draw itself. Normally occurs about 60 times a second, but timing is
        /// not guarunteed to be stable. This method assumes GraphicsDevice.Clear() is already called before this.
        /// </summary>
        /// <param name="gameTime">The current real and game time.</param>
        /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        /// <summary>
        /// Informs a GameScreen that a player has performed a specific inputAction, and should respond accordingly.
        /// </summary>
        /// <param name="inputAction">The action that was requested.</param>
        public abstract void PerformAction(InputAction inputAction);


        public virtual void PerformActionReleased(InputAction inputAction)
        {
            //Virtual since it is optional for GameScreens.
        }
    }
}
