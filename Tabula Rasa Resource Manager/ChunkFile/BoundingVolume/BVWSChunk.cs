using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVWSChunk : Chunk
    {
        BVOLChunk BoundingVolume;
        UInt32 IndexCount;
        UInt32 VertexCount;
        List<Face> Faces;
        List<Vertex> Vertices;
        bool Unknown1;

        public override bool Load( BinaryReader reader )
        {
            Faces = new List<Face>();
            Vertices = new List<Vertex>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            BoundingVolume = new BVOLChunk();
            if (!BoundingVolume.Load( reader ))
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

            if (Header.Version == 2)
            {
                Unknown1 = reader.ReadBoolean();
            }

            LogInfo( "face count: " + IndexCount );
            LogInfo( "vertex count: " + VertexCount );
            LogInfo( "unk1: " + Unknown1 );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyBVWalkableSurface;
        }
    }
}
