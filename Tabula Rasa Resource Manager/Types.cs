using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRRM
{
    public struct Vertex
    {
        public float X;
        public float Y;
        public float Z;

        public Vertex( float x, float y, float z )
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct Face
    {
        public UInt16 A;
        public UInt16 B;
        public UInt16 C;

        public Face( UInt16 a, UInt16 b, UInt16 c )
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
