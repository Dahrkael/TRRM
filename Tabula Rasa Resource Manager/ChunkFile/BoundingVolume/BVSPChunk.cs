using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVSPChunk : Chunk
    {
        public BVOLChunk BoundingVolume;
        public Vertex Vertex;
        public float Radius;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            BoundingVolume = new BVOLChunk();
            if ( !BoundingVolume.Load( reader ) )
            {
                return false;
            }

            Vertex = reader.ReadVertex();
            Radius = reader.ReadSingle();

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBVSphere;
        }
    }
}
