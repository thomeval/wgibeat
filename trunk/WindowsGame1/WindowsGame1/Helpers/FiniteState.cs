using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WGiBeat.Helpers
{
    public class FiniteState //Now that I've made this class it seems kinda redundant. Lol. Could lead to something more, though, in the future.
    {
        public int CurrentState { get; set; }

        public FiniteState(int startState)
        {
            CurrentState = startState;
        }
    }
}
