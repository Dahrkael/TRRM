using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVBXChunk : Chunk
    {
        public BVOLChunk BoundingVolume;
        public Vertex Vertex1;
        public Vertex Vertex2;

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

            Vertex1 = reader.ReadVertex();
            Vertex2 = reader.ReadVertex();

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBVBox;
        }
    }
}
