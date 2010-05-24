using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WGiBeat.Helpers;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public abstract class GameScreen : GameComponent
    {
        //UNSURE: What is this??? Zoran needs to explain himself.
        protected FiniteState State = new FiniteState(0);
        protected readonly GameCore Core;

        public GameScreen(GameCore core) : base(core)
        {
            Core = core;
        }

        // <summary>
        // Allows the game component to perform any initialization it needs to before starting
        // to run.  This is where it can query for any required services and load content.
        // </summary>
        //public override void Initialize()
        //{
        //    base.Initialize();
        //}

        // <summary>
        // Allows the game component to update itself.
        // </summary>
        // <param name="gameTime">Provides a snapshot of timing values.</param>
        //public override void Update(GameTime gameTime)
        //{
        //    base.Update(gameTime);
        //}

        public virtual void PerformKey(Keys key)
        {
            //Virtual so not all GameScreens need to implement it.
        }

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract void PerformAction(Action action);
    }
}
