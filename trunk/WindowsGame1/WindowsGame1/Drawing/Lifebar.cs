using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsGame1.Drawing
{
    public abstract class Lifebar : DrawableObject 
    {

        protected double Life { get; set; }

        public abstract void SetLife(double amount);

        public abstract event EventHandler LifebarEmpty;
        public abstract event EventHandler LifebarFull;
    }
}
