using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BIFXChunk : Chunk
    {
        public Int32 Size;
        public byte[] Data;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 3 ) )
            {
                return false;
            }

            Size = reader.ReadInt32();
            Data = reader.ReadBytes( Size );

            LogInfo( "size: " + Size + " bytes" );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.effShaderData;
        }
    }
}
