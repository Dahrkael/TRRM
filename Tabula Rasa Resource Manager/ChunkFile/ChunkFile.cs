using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    // C H N K B L X X
    // 43 48 4E 4B 42 4C 58 58
    struct ChunkFileHeader
    {
        public string Magic;    // CHNK
        public char Type;     // T or B
        public char Unknown1; // L
        public string Unknown2; // XX

        public bool Valid()
        {
            if ( Magic != "CHNK" ) { return false; }
            if ( Type != 'B' && Type != 'T' ) { return false; }
            if ( Unknown1 != 'L' ) { return false; }
            if ( Unknown2 != "XX" ) { return false; }
            return true;
        }
    }

    public class ChunkFile
    {
        public List<Chunk> Chunks;

        public ChunkFile()
        {
        }

        public bool Load( Stream stream )
        {
            Chunks = new List<Chunk>();
            using ( BinaryReader reader = new BinaryReader( stream ) )
            {
                if ( !ReadHeader( reader ) )
                {
                    return false;
                }

                ChunkType chunkType = ChunkUtils.PeekNextChunk( reader );
                while ( chunkType != ChunkType.None )
                {
                    Chunk masterChunk = ChunkUtils.Instance( chunkType );
                    if ( !masterChunk.Load( reader ) )
                    {
                        return false;
                    }

                    Chunks.Add( masterChunk );
                    chunkType = ChunkUtils.PeekNextChunk( reader );
                }
            }

            return true;
        }

        private bool ReadHeader( BinaryReader reader )
        {
            UInt32 headerOffset = 0;
            string check = reader.ReadBytesAsString( 4, true );
            if ( check != "CHNK" )
            {
                reader.BaseStream.Seek( -4, SeekOrigin.End );
                headerOffset = reader.ReadUInt32();    
            }
            
            reader.BaseStream.Seek( headerOffset, SeekOrigin.Begin );

            ChunkFileHeader header = new ChunkFileHeader()
            {
                Magic = reader.ReadBytesAsString( 4, true ),
                Type = reader.ReadChar(),
                Unknown1 = reader.ReadChar(),
                Unknown2 = reader.ReadBytesAsString( 2 )
            };

            if ( !header.Valid() )
            {
                Console.WriteLine( "ChunkFile: Header is not valid." );
                if ( header.Type == 'T' )
                {
                    Console.WriteLine( "ChunkFile: Text format not supported." );
                }
                return false;
            }

            if ( headerOffset > 0 )
            {
                // GLM Pack File
                Console.WriteLine( "This Chunk file is a pack. Use GLMFile class to load it." );
                return false;
            }

            return true;
        }
    }
}
