using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRRM
{
    public static class ExtensionMethods
    {
        public static string ReadBytesAsString( this System.IO.BinaryReader reader, uint length, bool bigEndian = false )
        {
            byte[] bytes = reader.ReadBytes( (int)length );
            if ( !bigEndian )
            {
                Array.Reverse( bytes ); // little endian
            }
            return System.Text.Encoding.ASCII.GetString( bytes );
        }

        public static string ReadCString( this System.IO.BinaryReader reader )
        {
            List<byte> bytes = new List<byte>();

            byte current;
            do
            {
                current = reader.ReadByte();
                bytes.Add( current );
            } while ( current != 0x00 );

            // remove null terminator
            bytes.RemoveAt( bytes.Count - 1 );

            return System.Text.Encoding.ASCII.GetString( bytes.ToArray() );
        }

        public static float[] ReadFloatArray( this System.IO.BinaryReader reader, uint length )
        {
            float[] data = new float[ length ];
            for ( int i = 0; i < length; i++ )
            {
                data[ i ] = reader.ReadSingle();
            }
            return data;
        }

        public static Vertex ReadVertex( this System.IO.BinaryReader reader )
        {
            Vertex vertex = new Vertex()
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };

            return vertex;
        }

        public static Face ReadFace( this System.IO.BinaryReader reader )
        {
            Face face = new Face()
            {
                A = reader.ReadUInt16(),
                B = reader.ReadUInt16(),
                C = reader.ReadUInt16()
            };

            return face;
        }

        public static float[,] ReadMatrix( this System.IO.BinaryReader reader, uint rows, uint columns )
        {
            float[,] matrix = new float[ rows, columns ];
            for ( int i = 0; i < rows; i++ )
            {
                float[] row = reader.ReadFloatArray( columns );
                for ( int j = 0; j < columns; j++ )
                {
                    matrix[ i, j ] = row[ j ];
                }
            };
            return matrix;
        }
    }
}
