using Flags = System.FlagsAttribute;
using UnityEngine;


namespace Yu5h1Lib.Runtime
{
    [Flags]
    public enum Direction
    {
        none = ~0 & forward | backward | left | right | up | down & (~(forward | backward | left | right | up)),
        forward     = 1 << 0,
        backward    = 1 << 1,
        left        = 1 << 2,
        right       = 1 << 3,
        up          = 1 << 4,
        down        = 1 << 5
    }
}
