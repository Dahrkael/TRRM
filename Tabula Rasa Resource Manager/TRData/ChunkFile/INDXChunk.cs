using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class INDXChunk : Chunk
    {
        public Int32 FaceCount;
        public List<Face> Faces;

        public override bool Load( BinaryReader reader )
        {
            Faces = new List<Face>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            FaceCount = reader.ReadInt32() / 3;
            LogInfo( "Faces offset: " + reader.BaseStream.Position );

            for ( Int32 i = 0; i < FaceCount; i++ )
            {
                Face face = reader.ReadFace();
                Faces.Add( face );
            }

            LogInfo( "Face count: " + FaceCount );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxIndexBufferImpl;
        }
    }
}
