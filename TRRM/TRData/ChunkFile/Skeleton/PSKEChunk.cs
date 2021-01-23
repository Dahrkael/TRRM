using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class PSKEChunk : Chunk
    {
        public List<PBONChunk> Bones;

        public override bool Load( BinaryReader reader )
        {
            Bones = new List<PBONChunk>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            UInt32 count = reader.ReadUInt32();
            LogInfo( "bone count: " + count );

            for ( UInt32 i = 0; i < count; i++ )
            {
                PBONChunk bone = new PBONChunk();
                if ( !bone.Load( reader ) )
                {
                    return false;
                }
                Bones.Add( bone );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phySkeleton;
        }
    }
}
