using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    class ISTRChunk : Chunk
    {
        public string ID;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            ID = reader.ReadCString();
            LogInfo( "id: " + ID );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.assIDString;
        }
    }
}
