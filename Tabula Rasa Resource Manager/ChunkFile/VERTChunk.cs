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
        DECLChunk DeclChunk;
        List<Vertex> Vertices;

        public override bool Load( BinaryReader reader )
        {
            Vertices = new List<Vertex>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }
            if ( Header.Version == 1 )
            {
                Debugger.Break();
            }
            
            DeclChunk = new DECLChunk();
            if ( !DeclChunk.Load( reader ) )
            {
                return false;
            }

            Int32 count = reader.ReadInt32();
            LogInfo( "vertices offset: " + reader.BaseStream.Position );

            for (Int32 i = 0; i < count; i++ )
            {
                /*
                long hackPos = reader.BaseStream.Position;
                for( Int32 j = 0; j < DeclChunk.Entries.Count; j++ )
                {
                    DECLData data = DeclChunk.Entries[ j ];
                    Int32 items = data.Stride / sizeof( float );
                    if (items == 2)
                    {
                        float u = reader.ReadSingle();
                        float v = reader.ReadSingle();
                        LogInfo( "u: +" + u + " v: " + v );
                    }
                    else
                    {
                        reader.ReadBytes( data.Stride );
                    }
                }

                reader.BaseStream.Seek( hackPos, SeekOrigin.Begin );
                */
                Vertex vertex = reader.ReadVertex();
                Vertices.Add( vertex );
                reader.ReadBytes( DeclChunk.TotalStride - 12 );
            }
            
            LogInfo( "vertex count: " + count );
            
            Skip( reader );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxVertexBufferImpl;
        }
    }
}
