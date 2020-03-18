using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class CPDGChunk : Chunk
    {
        List<CPDFChunk> CPDefinitions;

        public override bool Load( BinaryReader reader )
        {
            CPDefinitions = new List<CPDFChunk>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            UInt32 count = reader.ReadUInt32();
            LogInfo( "definitions: " + count );

            for ( UInt32 i = 0; i < count; i++ )
            {
                CPDFChunk chunk = new CPDFChunk();
                if ( !chunk.Load( reader ) )
                {
                    return false;
                }
                CPDefinitions.Add( chunk );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyCPDefinitionGroupImpl;
        }
    }
}
