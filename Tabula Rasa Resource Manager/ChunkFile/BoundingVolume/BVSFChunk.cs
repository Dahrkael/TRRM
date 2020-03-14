using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVSFChunk : Chunk
    {
        public BVOLChunk BoundingVolume;
        public UInt32 IndexCount;
        public UInt32 VertexCount;
        public List<Face> Faces;
        public List<Vertex> Vertices;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            BoundingVolume = new BVOLChunk();
            if ( !BoundingVolume.Load( reader ) )
            {
                return false;
            }

            IndexCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();

            LogInfo( "vertices offset: " + reader.BaseStream.Position );
            for ( UInt32 i = 0; i < VertexCount; i++ )
            {
                Vertex vertex = reader.ReadVertex();
                Vertices.Add( vertex );
            }

            LogInfo( "faces offset: " + reader.BaseStream.Position );
            for ( UInt32 i = 0; i < IndexCount; i++ )
            {
                Face face = reader.ReadFace();
                Faces.Add( face );
            }

            LogInfo( "face count: " + IndexCount );
            LogInfo( "vertex count: " + VertexCount );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBVSurface;
        }
    }
}
