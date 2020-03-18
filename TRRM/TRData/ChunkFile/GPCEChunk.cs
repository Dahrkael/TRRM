using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class GPCEChunk : Chunk
    {
        public EFCTChunk Effect;
        public INDXChunk IndexBuffer;
        public VERTChunk VertexBuffer;
        public BBOXChunk BoundingBox;
        public USDAChunk USDA;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 8, 9, 10, 11, 12 ) )
            {
                return false;
            }

            if ( Header.Version >= 9 )
            {
                bool hasMaterial = reader.ReadBoolean();
            }

            Effect = new EFCTChunk();
            if ( !Effect.Load( reader ) )
            {
                return false;
            }
            // indices
            IndexBuffer = new INDXChunk();
            if ( !IndexBuffer.Load( reader ) )
            {
                return false;
            }
            // vertices
            if ( Header.Version >= 10 )
            {
                VertexBuffer = new VERTChunk();
                if ( !VertexBuffer.Load( reader ) )
                {
                    return false;
                }
            }
            else
            {
                // wtf
                Int32 unk0 = reader.ReadInt32();
                if ( unk0 == 2 )
                {
                    Debugger.Break();
                    VertexBuffer = new VERTChunk();
                    if ( !VertexBuffer.Load( reader ) )
                    {
                        return false;
                    }
                    VERTChunk vertexBuffer2 = new VERTChunk();
                    if ( !vertexBuffer2.Load( reader ) )
                    {
                        return false;
                    }
                }
                else
                {
                    VertexBuffer = new VERTChunk();
                    if ( !VertexBuffer.Load( reader ) )
                    {
                        return false;
                    }
                }
            }
            // bounding box
            BoundingBox = new BBOXChunk();
            if ( !BoundingBox.Load( reader ) )
            {
                return false;
            }
            string unk1 = reader.ReadCString();
            UInt32 unk2 = reader.ReadUInt32();
            Int32 unk3 = reader.ReadInt32();
            string unk4 = reader.ReadCString();
            string unk5 = reader.ReadCString();
            ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
            if ( nextChunk == ChunkType.gfxUSDA )
            {
                USDA = new USDAChunk();
                if ( !USDA.Load( reader ) )
                {
                    return false;
                }
            }
            Int32 unk6 = reader.ReadInt32();
            UInt32 unk7 = reader.ReadUInt32();
            Int32 unk8 = reader.ReadInt32();

            if ( Header.Version == 12 )
            {
                Int32 unk9 = reader.ReadInt32();
            }

            if ( Header.Version >= 11 )
            {
                Int32 count = reader.ReadInt32();
                for ( Int32 i = 0; i < count; i++ )
                {
                    //Debugger.Break();
                    // index buffer
                    INDXChunk indices = new INDXChunk();
                    if ( !indices.Load( reader ) )
                    {
                        return false;
                    }
                }
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.gfxGeometryPiece;
        }
    }
}
