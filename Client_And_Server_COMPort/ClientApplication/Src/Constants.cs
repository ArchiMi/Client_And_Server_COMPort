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
        //Old constant
        //public const int FRAME_LENGTH = 32;

        //Frame standart ( Start(:) / Address / Function / Data / LRC Check / End(:) / 1 Char / 2 Chars / n Chars / CR / LF )
        public const Char CHR_COLON = ':';

        public const Char CHR_MOTOR_X = '1';
        public const Char CHR_MOTOR_Y = '2';
        public const Char CHR_MOTOR_Z = '3';

        public const Char CHR_FUNC_MOVE_UP = '1';
        public const Char CHR_FUNC_MOVE_DOWN = '2';
        public const Char CHR_FUNC_MOVE_LEFT = '3';
        public const Char CHR_FUNC_MOVE_RIGHT = '4';
    }
}
