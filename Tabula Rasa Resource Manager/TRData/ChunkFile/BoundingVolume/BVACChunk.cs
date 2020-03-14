using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BVACChunk : Chunk
    {
        public BVOLChunk BoundingVolume;
        public Vertex Vertex;
        public float Length;
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
            // or the other way around
            Length = reader.ReadSingle();
            Radius = reader.ReadSingle();

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            throw new NotImplementedException();
        }
    }
}
