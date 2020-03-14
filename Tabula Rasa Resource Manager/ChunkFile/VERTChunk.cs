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
        DECLChunk VertexDeclarations;
        List<Vertex> Vertices;
        List<UV> UVs;

        public override bool Load( BinaryReader reader )
        {
            Vertices = new List<Vertex>();
            UVs = new List<UV>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }
            if ( Header.Version == 1 )
            {
                Debugger.Break();
            }

            VertexDeclarations = new DECLChunk();
            if ( !VertexDeclarations.Load( reader ) )
            {
                return false;
            }

            Int32 count = reader.ReadInt32();
            LogInfo( "vertices offset: " + reader.BaseStream.Position );

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
                        case VertexDeclUsage.TextureCoordinate:
                            UV uv = reader.ReadUV();
                            UVs.Add( uv );
                            break;
                        default:
                            reader.ReadBytes( declaration.Stride() );
                            break;
                    }
                }
            }
            
            LogInfo( "vertex count: " + count );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxVertexBufferImpl;
        }
    }
}
