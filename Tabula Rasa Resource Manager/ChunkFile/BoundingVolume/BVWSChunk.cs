using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVWSChunk : Chunk
    {
        BVOLChunk BoundingVolume;
        UInt32 IndexCount;
        UInt32 VertexCount;
        List<UInt16[]> Indices;
        List<float[]> Vertices;
        bool Unknown1;

        public override bool Load( BinaryReader reader )
        {
            Indices = new List<UInt16[]>();
            Vertices = new List<float[]>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            BoundingVolume = new BVOLChunk();
            if (!BoundingVolume.Load( reader ))
            {
                return false;
            }

            IndexCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();

            LogInfo( "vertices offset: " + reader.BaseStream.Position );
            for ( UInt32 i = 0; i < VertexCount; i++ )
            {
                Vertices.Add( reader.ReadFloatArray( 3 ) );
            }

            LogInfo( "indices offset: " + reader.BaseStream.Position );
            for ( UInt32 i = 0; i < IndexCount; i++ )
            {
                UInt16[] set = new UInt16[ 3 ];
                set[ 0 ] = reader.ReadUInt16();
                set[ 1 ] = reader.ReadUInt16();
                set[ 2 ] = reader.ReadUInt16();
            }

            if (Header.Version == 2)
            {
                Unknown1 = reader.ReadBoolean();
            }

            LogInfo( "index count: " + IndexCount );
            LogInfo( "vertex count: " + VertexCount );
            LogInfo( "unk1: " + Unknown1 );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBVWalkableSurface;
        }
    }
}
