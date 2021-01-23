using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BDATChunk : Chunk
    {
        public string BoneName;
        public float[] Rotation;
        public float[] Translation;
        public float[] Scale;
        public float[,] BoneMatrix;
        public Chunk BoundingVolume;

        public override bool Load( BinaryReader reader )
        {
            BoundingVolume = null;

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            BoneName = reader.ReadCString();
            Rotation = reader.ReadFloatArray( 4 );
            Translation = reader.ReadFloatArray( 3 );
            Scale = new float[ 3 ] { 1.0f, 1.0f, 1.0f };

            if (Header.Version == 2)
            {
                Scale = reader.ReadFloatArray( 3 );
            }

            BoneMatrix = reader.ReadMatrix( 4, 4 );

            LogInfo( "name: " + BoneName );

            ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
            switch( nextChunk )
            {
                case ChunkType.phyBone:
                    break;
                case ChunkType.phyBVSphere:
                    BoundingVolume = new BVSPChunk();
                    break;
                case ChunkType.phyBVBox:
                    BoundingVolume = new BVBXChunk();
                    break;
                case ChunkType.phyBVAlignedCylinder:
                    BoundingVolume = new BVACChunk();
                    break;
                case ChunkType.phyBVCapsule:
                    BoundingVolume = new BVCPChunk();
                    break;
                case ChunkType.phyBVSurface:
                    BoundingVolume = new BVSFChunk();
                    break;
                case ChunkType.phyBVWalkableSurface:
                    BoundingVolume = new BVWSChunk();
                    break;
                default:
                    //Debugger.Break();
                    break;
            }

            if ( BoundingVolume != null )
            {
                if ( !BoundingVolume.Load( reader ) )
                {
                    return false;
                }
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBoneSharedData;
        }
    }
}
