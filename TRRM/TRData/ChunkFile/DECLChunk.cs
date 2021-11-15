using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    

    public class DECLChunk : Chunk
    {
        public readonly byte[] UsageLookup = new byte[] { 0, 1, 2, 3, 4, 0xA, 0xA, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD };
        public List<VertexDeclaration> Declarations;
        public Int32 TotalStride;

        public override bool Load( BinaryReader reader )
        {
            Declarations = new List<VertexDeclaration>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            switch( Header.Version )
            {
                case 1:
                    {
                        Debugger.Break();
                        Int32 count = reader.ReadInt32();
                        byte currentOffset = 0;
                        for ( Int32 i = 0; i < count; i++ )
                        {
                            VertexDeclaration declaration = new VertexDeclaration()
                            {
                                Offset = currentOffset,
                                Type = (VertexDeclType)(byte)reader.ReadInt16(),
                                Method = (VertexDeclMethod)(byte)reader.ReadInt16(),
                                Usage = (VertexDeclUsage)UsageLookup[(byte)reader.ReadInt16()],
                                UsageIndex = (byte)reader.ReadInt16()
                            };
                            Declarations.Add(declaration);
                            currentOffset += declaration.Stride();
                        }
                    }
                    break;
                case 2:
                    {
                        Int32 count = reader.ReadInt32();
                        byte currentOffset = 0;
                        for ( Int32 i = 0; i < count; i++ )
                        {
                            VertexDeclaration declaration = new VertexDeclaration()
                            {
                                Offset = currentOffset,
                                Type = (VertexDeclType)reader.ReadByte(),
                                Method = (VertexDeclMethod)reader.ReadByte(),
                                Usage = (VertexDeclUsage)reader.ReadByte(),
                                UsageIndex = reader.ReadByte()
                            };
                            Declarations.Add( declaration );
                            currentOffset += declaration.Stride();
                        }
                    }
                    break;
            }

            TotalStride = Declarations.Sum( d => d.Stride() );

            Declarations.ForEach( data => LogInfo( "O: " + data.Offset + " T: " + data.Type + " M: " + data.Method + " U: " + data.Usage + " I: " + data.UsageIndex + " stride: " + data.Stride() ) );
            LogInfo( "total stride: " + TotalStride );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.effVertexDecl;
        }
    }
}
