using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class USDAChunk : Chunk
    {
        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            UInt32 count = reader.ReadUInt32();
            LogInfo( "count: " + count );

            for(UInt32 i = 0; i < count; i++ )
            {
                string key = reader.ReadCString();
                string value = reader.ReadCString();
                LogInfo("key: " + key + " value: " + value );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxUSDA;
        }
    }
}
