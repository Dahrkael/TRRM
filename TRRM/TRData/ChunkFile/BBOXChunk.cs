using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BBOXChunk : Chunk
    {
        public bool IsEmpty { get; set; }
        public Vertex Min { get; set; }
        public Vertex Max { get; set; }
        public Vertex Center { get; set; }
        public float Radius { get; set; }
        public float Radius2D
        {
            get
            {
                return (float)Math.Abs(Math.Sqrt(Math.Pow(Max.Z - Center.Z, 2.0f) + Math.Pow(Max.X - Center.X, 2.0f)));
            }
        }

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            if ( Header.Version == 2 )
            {
                IsEmpty = reader.ReadBoolean();
            }
            
            Min = reader.ReadVertex();
            Max = reader.ReadVertex();
            Center = reader.ReadVertex();
            Radius = reader.ReadSingle();

            if ( Header.Version == 1 )
            {
                IsEmpty = Radius <= 0.0;
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBoundingBox;
        }
    }
}
