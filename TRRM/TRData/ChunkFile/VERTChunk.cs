using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class VERTChunk : Chunk
    {
        public DECLChunk VertexDeclarations;
        public List<Vertex> Vertices;
        public List<Vertex> Normals;
        public List<UV> UVs;
        public List<UInt32> Colors;

        public byte[] RawBuffer;

        public override bool Load( BinaryReader reader )
        {
            Vertices = new List<Vertex>();
            Normals = new List<Vertex>();
            UVs = new List<UV>();
            Colors = new List<UInt32>();
            RawBuffer = new byte[0];

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2, 3 ) )
            {
                return false;
            }
            if ( Header.Version == 1 )
            {
                Debugger.Break();
            }

            // NOTE: Auto Assault
            if ( Header.Version == 3 )
            {
                UInt64 crc = reader.ReadUInt64();
            }

            VertexDeclarations = new DECLChunk();
            if ( !VertexDeclarations.Load( reader ) )
            {
                return false;
            }

            Int32 count = reader.ReadInt32();
            Int64 verticesOffset = reader.BaseStream.Position;
            LogInfo( "vertices offset: " + verticesOffset );

            for (Int32 i = 0; i < count; i++ )
            {
                for(Int32 j = 0; j < VertexDeclarations.Declarations.Count; j++ )
                {
                    VertexDeclaration declaration = VertexDeclarations.Declarations[ j ];
                    switch( declaration.Usage )
                    {
                        case VertexDeclUsage.Position:
                            Vertex vertex = reader.ReadVertex();
                            Vertices.Add( vertex );
                            break;
                        case VertexDeclUsage.Normal:
                            Vertex normal = reader.ReadVertex();
                            Normals.Add( normal );
                            break;
                        case VertexDeclUsage.TextureCoordinate:
                            UV uv = reader.ReadUV();
                            if ( declaration.UsageIndex == 0 )
                            {
                                UVs.Add( uv );
                            }
                            break;
                        case VertexDeclUsage.Color:
                            UInt32 color = reader.ReadUInt32();
                            Colors.Add( color );
                            break;
                        default:
                            reader.ReadBytes( declaration.Stride() );
                            break;
                    }
                }
            }

            LogInfo( "vertex count: " + count );

            // rewind and grab the full buffer
            RawBuffer = new byte[ reader.BaseStream.Position - verticesOffset ];
            reader.BaseStream.Seek( verticesOffset, SeekOrigin.Begin );
            reader.ReadBytes( RawBuffer.Length );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxVertexBufferImpl;
        }
    }
}
