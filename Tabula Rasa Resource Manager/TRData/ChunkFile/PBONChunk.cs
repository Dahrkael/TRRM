using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class PBONChunk : Chunk
    {
        public Int32 ParentBoneID; // not sure
        public Int32 Unknown1;
        public Int32 Unknown2;
        public float Unknown3;
        BDATChunk BoneData;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            ParentBoneID = reader.ReadInt32();
            Unknown1 = reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            Unknown3 = reader.ReadSingle();

            LogInfo( "parent id: " + ParentBoneID );
            LogInfo( "unk1: " + Unknown1 );
            LogInfo( "unk2: " + Unknown2 );
            LogInfo( "unk3: " + Unknown3 );

            BoneData = new BDATChunk();
            if ( !BoneData.Load( reader ) )
            {
                return false;
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBone;
        }
    }
}
