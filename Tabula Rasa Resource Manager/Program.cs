using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TRRM
{
    static class Program
    {
        public static Dictionary<string, string> Arguments;

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main( string[] args )
        {
            Arguments = new Dictionary<string, string>();
            for ( int i = 0; i < args.Length; i++ )
            {
                string argument = args[ i ];

                if (!argument.StartsWith("-"))
                    continue;

                argument = argument.TrimStart( '-' );
                
                if ( argument.Contains( '=' ) )
                {
                    string[] pair = argument.Split( '=' );
                    Arguments.Add( pair[ 0 ], pair[ 1 ] );
                }
                else
                {
                    Arguments.Add( argument, "" );
                }

            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new Form1() );
        }
    }
}
