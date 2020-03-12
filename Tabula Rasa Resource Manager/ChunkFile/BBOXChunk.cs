using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BBOXChunk : Chunk
    {
        bool Unknown2;
        float[,] UnknownMatrix3x3;
        float Unknown1;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            if ( Header.Version == 2 )
            {
                Unknown2 = reader.ReadBoolean();
            }
            UnknownMatrix3x3 = reader.ReadMatrix( 3, 3 );
            Unknown1 = reader.ReadSingle();

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBoundingBox;
        }
    }
}
