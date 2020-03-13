using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public struct DECLData
    {
        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public Int32 Stride;
    }

    public class DECLChunk : Chunk
    {
        public List<DECLData> Entries;
        public Int32 TotalStride;

        public override bool Load( BinaryReader reader )
        {
            Entries = new List<DECLData>();

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
                        for ( Int32 i = 0; i < count; i++ )
                        {
                            reader.ReadUInt16();
                            reader.ReadUInt16();
                            reader.ReadUInt16();
                            reader.ReadUInt16();
                        }
                    }
                    break;
                case 2:
                    {
                        Int32 count = reader.ReadInt32();
                        for ( Int32 i = 0; i < count; i++ )
                        {
                            DECLData data = new DECLData()
                            {
                                A = reader.ReadByte(),
                                B = reader.ReadByte(),
                                C = reader.ReadByte(),
                                D = reader.ReadByte()
                            };
                            data.Stride = Stride( data.A );
                            Entries.Add( data );
                        }
                    }
                    break;
            }

            TotalStride = Entries.Sum( d => d.Stride );

            Entries.ForEach( data => LogInfo( "a: " + data.A + " b: " + data.B + " c: " + data.C + " d: " + data.D + " stride: " + data.Stride ) );
            LogInfo( "total stride: " + TotalStride );

            End( reader );
            return true;
        }

        private Int32 Stride( byte data )
        {
            // ¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?¿?
            switch ( data )
            {
                case 0:
                case 4:
                case 5:
                case 6:
                case 8:
                case 9:
                case 11:
                case 13:
                case 14:
                case 15:
                    return 4;
                case 1:
                case 7:
                case 10:
                case 12:
                case 16:
                    return 8;
                case 2:
                    return 12;
                case 3:
                    return 16;
                default:
                    return 0;
            }
        }

        public override ChunkType Type()
        {
            return ChunkType.effVertexDecl;
        }
    }
}
