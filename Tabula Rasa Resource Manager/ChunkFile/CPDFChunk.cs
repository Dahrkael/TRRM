using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class CPDFChunk : Chunk
    {
        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            Skip( reader );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyCPDefinitionImpl;
        }
    }
}
