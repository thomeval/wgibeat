using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WGiBeat.Drawing
{
    public class SineSwayParticle : Sprite
    {
        public double Position { get; set; } //Between 0 and 1.
        public double Frequency { get; set; }
        public Boolean Vertical { get; set; }

        //Convert sine wave amplitude to fit width. default Vertical = true.

        public SineSwayParticle()
        {
            Vertical = true;


        }

    }
}
