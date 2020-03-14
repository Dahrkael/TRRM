using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class GLMFile
    {
        public string Path;
        public string Filename;
        public List<PackedFile> Files;
        //
        public UInt32 Size;
        public UInt32 HeaderOffset;
        public UInt32 FileTableOffset;
        public UInt32 FileTableLength;
        public UInt32 FilesCount;

        public GLMFile( string root, string path )
        {
            BinaryReader file = new BinaryReader( File.Open( path, FileMode.Open, FileAccess.Read ) );
            Size = (UInt32)file.BaseStream.Length;
            file.BaseStream.Seek( Size - 4, SeekOrigin.Begin );

            HeaderOffset = file.ReadUInt32();
            file.BaseStream.Seek( HeaderOffset, SeekOrigin.Begin );

            // C H N K B L X X
            // 43 48 4E 4B 42 4C 58 58
            UInt64 header = file.ReadUInt64();
            if ( header != 0x58584C424B4E4843 )
            {
                Debug.Fail( "Header doesnt match!\nWrong file?" );
                return;
            }

            Path = path;
            Filename = path.Replace( root, "" );

            FileTableOffset = file.ReadUInt32();
            FileTableLength = file.ReadUInt32();
            FilesCount = file.ReadUInt32();

            // retrieve inner files
            Files = new List<PackedFile>();
            for ( int i = 0; i < FilesCount; i++ )
            {
                PackedFile trFile = new PackedFile( this )
                {
                    Offset = file.ReadUInt32(),
                    Size = file.ReadUInt32(),
                    SizeUncompressed = file.ReadUInt32(),
                    FilenameOffset = file.ReadUInt32(),
                    Flags = file.ReadUInt16(),
                    Timestamp = file.ReadUInt32()
                };
                Files.Add( trFile );
            }

            // retrieve inner filenames
            foreach ( PackedFile trFile in Files )
            {
                file.BaseStream.Seek( trFile.FilenameOffset + FileTableOffset, SeekOrigin.Begin );
                List<byte> bytes = new List<byte>();
                while ( true )
                {
                    byte last = file.ReadByte();
                    if ( last == 0x00 ) { break; }
                    bytes.Add( last );
                }
                System.Text.Encoding enc = System.Text.Encoding.ASCII;
                trFile.Filename = enc.GetString( bytes.ToArray() );
            }

            file.Close();
        }
    }
}
