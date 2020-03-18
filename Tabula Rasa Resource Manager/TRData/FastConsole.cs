using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TRRM
{
    // https://stackoverflow.com/questions/5272177/console-writeline-slow
    public static class FastConsole
    {
        private static bool disabled = false;

        private static StringBuilder _sb = new StringBuilder();
        private static int _lineCount;

        public static void WriteLine( string message )
        {
            if ( disabled )
                return;

            _sb.AppendLine( message );
            ++_lineCount;
            if ( _lineCount >= 100 )
                Flush();
        }

        public static void Flush()
        {
            if ( disabled )
                return;

            Console.WriteLine( _sb.ToString() );
            _lineCount = 0;
            _sb.Clear();
        }
    }
}
