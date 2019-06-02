using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientAppNameSpace.Src
{
    enum Commands
    {
        ping,
        move_up,
        move_down,
        move_left,
        move_right,
        get_length,
        get_temperature
    }

    public class Const
    {
        public const int FRAME_LENGTH = 17;
    }
}
