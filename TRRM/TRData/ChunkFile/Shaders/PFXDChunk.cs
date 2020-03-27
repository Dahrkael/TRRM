using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class PFXDChunk : Chunk
    {
        public UInt32 Hash;
        public List<assIDChunk> Entries;

        public override bool Load( BinaryReader reader )
        {
            Entries = new List<assIDChunk>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            Hash = reader.ReadUInt32();

            UInt32 count = reader.ReadUInt32();
            for ( UInt32 i = 0; i < count; i++ )
            {
                assIDChunk idChunk = new assIDChunk();
                if ( !idChunk.Load( reader ) )
                {
                    return false;
                }

                if ( Header.Version == 2 )
                {
                    UInt32 hash = reader.ReadUInt32();
                    LogInfo( "hash: " + hash );
                }

                Entries.Add( idChunk );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.effPrecompiledFXData;
        }
    }
}