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
        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 8, 9, 10, 11, 12 ) )
            {
                return false;
            }

            switch( Header.Version )
            {
                case 12:
                    bool hasMaterial = reader.ReadBoolean();
                    EFCTChunk effect = new EFCTChunk();
                    if ( !effect.Load( reader ) )
                    {
                        return false;
                    }
                    // indices
                    INDXChunk indexBuffer = new INDXChunk();
                    if ( !indexBuffer.Load( reader ) )
                    {
                        return false;
                    }
                    // vertices
                    VERTChunk vertexBuffer = new VERTChunk();
                    if ( !vertexBuffer.Load( reader ) )
                    {
                        return false;
                    }
                    // bounding box
                    BBOXChunk boundingBox = new BBOXChunk();
                    if ( !boundingBox.Load( reader ) )
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
                        USDAChunk usda = new USDAChunk();
                        if ( !usda.Load( reader ) )
                        {
                            return false;
                        }
                    }
                    Int32 unk6 = reader.ReadInt32();
                    UInt32 unk7 = reader.ReadUInt32();
                    Int32 unk8 = reader.ReadInt32();
                    Int32 unk9 = reader.ReadInt32();

                    Int32 count = reader.ReadInt32();
                    for ( Int32 i = 0; i < count; i++ )
                    {
                        // index buffer
                        INDXChunk indices = new INDXChunk();
                        if ( !indexBuffer.Load( reader ) )
                        {
                            return false;
                        }
                    }
                    break;
                default:
                    Debugger.Break();
                    break;
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
