using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BVOLChunk : Chunk
    {
        public float Unknown1;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            Unknown1 = reader.ReadSingle();

            LogInfo( "unk1: " + Unknown1 );
            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBoundingVolume;
        }
    }
}
