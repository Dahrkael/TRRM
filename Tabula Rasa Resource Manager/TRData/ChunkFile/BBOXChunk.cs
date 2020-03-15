using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BBOXChunk : Chunk
    {
        public bool Unknown2;
        public Vertex VertexMin;
        public Vertex VertexMax;
        public Vertex Origin;
        public float Unknown1;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            if ( Header.Version == 2 )
            {
                Unknown2 = reader.ReadBoolean();
            }
            
            VertexMin = reader.ReadVertex();
            VertexMax = reader.ReadVertex();
            Origin = reader.ReadVertex();
            Unknown1 = reader.ReadSingle();

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBoundingBox;
        }
    }
}
