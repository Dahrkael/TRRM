using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TRRM
{
    [StructLayout( LayoutKind.Sequential )]
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

    [StructLayout( LayoutKind.Sequential )]
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

    [StructLayout( LayoutKind.Sequential )]
    public struct UV
    {
        public float U;
        public float V;

        public UV( float u, float v )
        {
            U = u;
            V = v;
        }
    }

    public enum VertexDeclType : byte
    {
        Float1 = 0,
        Float2 = 1,
        Float3 = 2,
        Float4 = 3,
        Color = 4,
        Ubyte4 = 5,
        Short2 = 6,
        Short4 = 7,
        Ubyte4N = 8,
        Short2N = 9,
        Short4N = 10,
        UShort2N = 11,
        UShort4N = 12,
        UDec3 = 13,
        Dec3N = 14,
        Float16Two = 15,
        Float16Four = 16,
        Unused = 17
    }

    public enum VertexDeclUsage : byte
    {
        Position = 0,
        BlendWeight = 1,
        BlendIndices = 2,
        Normal = 3,
        PointSize = 4,
        TextureCoordinate = 5,
        Tangent = 6,
        BiNormal = 7,
        TessellateFactor = 8,
        PositionTransformed = 9,
        Color = 10,
        Fog = 11,
        Depth = 12,
        Sample = 13
    }

    public enum VertexDeclMethod : byte
    {
        Default = 0,
        PartialU = 1,
        PartialV = 2,
        CrossUv = 3,
        UV = 4,
        LookUp = 5,
        LookUpPresampled = 6
    }

    public struct VertexDeclaration
    {
        public UInt16 Offset;
        public VertexDeclType Type;
        public VertexDeclMethod Method;
        public VertexDeclUsage Usage;
        public byte UsageIndex;

        public UInt16 Stride()
        {
            switch ( Type )
            {
                case VertexDeclType.Float1:
                case VertexDeclType.Color:
                case VertexDeclType.Ubyte4:
                case VertexDeclType.Short2:
                case VertexDeclType.Ubyte4N:
                case VertexDeclType.Short2N:
                case VertexDeclType.UShort2N:
                case VertexDeclType.UDec3:
                case VertexDeclType.Dec3N:
                case VertexDeclType.Float16Two:
                    return 4;
                case VertexDeclType.Float2:
                case VertexDeclType.Short4:
                case VertexDeclType.Short4N:
                case VertexDeclType.UShort4N:
                case VertexDeclType.Float16Four:
                    return 8;
                case VertexDeclType.Float3:
                    return 12;
                case VertexDeclType.Float4:
                    return 16;
                default:
                    return 0;
            }
        }
    }
}
