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
        public string WSName;
        public float Unknown1;
        public BBOXChunk BoundingBox;
        public CPDGChunk CPDefinitions;
        public PSKEChunk Skeleton;
        public List<Chunk> Children;
        public List<Chunk> Shadows;

        public override bool Load( BinaryReader reader )
        {
            Children = new List<Chunk>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2, 3 ) )
            {
                return false;
            }

            if ( Header.Version == 3 )
            {
                Unknown1 = reader.ReadSingle();
                FastConsole.WriteLine( "unk1" + Unknown1 );
            }

            // Auto Assault hack
            {
                ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                if ( nextChunk == ChunkType.None )
                {
                    //UInt32 blockSize = reader.ReadUInt32();
                    UInt32 count = reader.ReadUInt32();
                    for(UInt32 i = 0; i < count; i++ )
                    {
                        string shaderName = reader.ReadCString();
                    }
                }
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
                return false;
                //throw new NotImplementedException();
            }

            Int32 piecesCount = reader.ReadInt32();
            Children = new List<Chunk>();
            for ( Int32 i = 0; i < piecesCount; i++ )
            {
                ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                Chunk chunk = ChunkUtils.Instance( nextChunk );
                Debug.Assert( chunk.Load( reader ) );
                Children.Add( chunk );
            }

            Int32 shadowsCount = reader.ReadInt32();
            Shadows = new List<Chunk>();
            for ( Int32 i = 0; i < shadowsCount; i++ )
            {
                ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                Chunk chunk = ChunkUtils.Instance( nextChunk );
                Debug.Assert( chunk.Load( reader ) );
                Shadows.Add( chunk );
            }

            if ( Header.Version >= 2 )
            {
                // some kind of flag
                bool hasLODHandler = reader.ReadBoolean();
                if ( hasLODHandler )
                {
                    // gfxLODHandler
                    // LDAA LDSD
                    ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
                    Skip( reader );
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
