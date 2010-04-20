using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1.Screens
{
    public abstract class GameScreen : GameComponent
    {
 
        protected GameCore Core;
        public GameScreen(GameCore core) : base(core)
        {
            Core = core;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            base.Update(gameTime);

        }

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public abstract void PerformAction(Action action);
    }
}
