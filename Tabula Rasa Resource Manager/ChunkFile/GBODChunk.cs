using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class GBODChunk : Chunk
    {
        public float Unknown1;
        public bool Unknown2;
        public BBOXChunk BoundingBox;
        public CPDGChunk CPDefinitions;
        public PSKEChunk Skeleton;
        public string WSName;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2, 3 ) )
            {
                return false;
            }

            if ( Header.Version != 3 )
            {
                Debugger.Break();
            }

            if ( Header.Version == 3 )
            {
                Unknown1 = reader.ReadSingle();
                Console.WriteLine( "GBOD: unk1 {0}", Unknown1 );
            }

            // bounding box
            BoundingBox = new BBOXChunk();
            Debug.Assert( BoundingBox.Load( reader ) );
            //cp definitions
            CPDefinitions = new CPDGChunk();
            Debug.Assert( CPDefinitions.Load( reader ) );

            // R&D
            // Seems RTTI stuff
            //if ( ChunkUtils.PeekNextChunk( reader ) == ChunkType.None )
            {
                UInt32 entryCount = reader.ReadUInt32();
                LogInfo( "rtti count: " + entryCount );

                for( UInt32 i = 0; i < entryCount; i++ )
                {
                    Int32 entryID = reader.ReadInt32(); // not sure

                    UInt32 subCount = reader.ReadUInt32();
                    LogInfo( "rtti entry: id " + entryID + " subentries " + subCount );
                    for ( UInt32 j = 0; j < subCount; j++ )
                    {
                        string key = reader.ReadCString();
                        string value = reader.ReadCString();
                        LogInfo( "rtti entry: " + key + " : " + value );
                    }
                }
            }

            // skeleton
            Skeleton = new PSKEChunk();
            Debug.Assert( Skeleton.Load( reader ) );

            Int32 morphCount = reader.ReadInt32();
            for ( Int32 i = 0; i < morphCount; i++ )
            {
                // TODO gfxMorphWeightArray
                throw new NotImplementedException();
            }

            Int32 othersCount = reader.ReadInt32(); // unsure
            List<Chunk> others = new List<Chunk>();
            for ( Int32 i = 0; i < othersCount; i++ )
            {
                // more tags
                ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                Chunk chunk = ChunkUtils.Instance( nextChunk );
                Debug.Assert( chunk.Load( reader ) );
                others.Add( chunk );
            }

            Int32 unkCount = reader.ReadInt32();
            for ( Int32 i = 0; i < unkCount; i++ )
            {
                // fDirLgtShadowRang ¿?
                throw new NotImplementedException();
            }

            if ( Header.Version >= 2 )
            {
                // some kind of flag
                Unknown2 = reader.ReadBoolean();
                if ( Unknown2 )
                {
                    ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                    Debugger.Break();
                }
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxBody;
        }
    }
}
