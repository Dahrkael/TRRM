using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class GSKNChunk : Chunk
    {
        public GPCEChunk Geometry;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            Geometry = new GPCEChunk();
            if ( !Geometry.Load( reader ) )
            {
                return false;
            }

            UInt32 count = reader.ReadUInt32();
            for(UInt32 i = 0; i < count; i++ )
            {
                string boneName = reader.ReadCString();
                LogInfo( "bone: " + boneName );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxGeometryPieceSkinned;
        }
    }
}
