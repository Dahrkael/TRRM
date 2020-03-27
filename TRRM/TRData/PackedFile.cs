
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO.Compression;

namespace TRRM
{
    /*
        type: .anm	items: 6238
        type: .cat	items: 5205
        type: .dat	items: 2
        type: .dds	items: 9403
        type: .fxc	items: 521
        type: .geo	items: 7240
        type: .glm	items: 3
        type: .jpg	items: 1
        type: .mkr	items: 1253
        type: .mp3	items: 97
        type: .ltmp	items: 157
        type: .ogg	items: 2752
        type: .pfx	items: 2097
        type: .pkg	items: 2637
        type: .shc	items: 42
        type: .sky	items: 144
        type: .tab	items: 117
        type: .tga	items: 20
        type: .thd	items: 66
        type: .tpc	items: 10677
        type: .wav	items: 1966
        type: .xml	items: 1
    */
    public enum TRFileType
    {
        TGA,
        DDS,
        GEO,
        ANM,
        JPG,
        MP3,
        OGG,
        WAV,
        XML,
        FXC,
        Other
    }

    public class PackedFile
    {
        public string Filename;
        public GLMFile Parent;
        // 
        public UInt32 Size;
        public UInt32 SizeUncompressed;
        public UInt32 Offset;
        public UInt32 FilenameOffset;
        public UInt16 Flags;
        public UInt32 Timestamp;

        public PackedFile( GLMFile parent )
        {
            Parent = parent;
        }

        public bool IsCompressed
        {
            get { return Flags != 0; }
        }

        public TRFileType GetFileType()
        {
            if ( Filename.EndsWith( ".tga" ) ) return TRFileType.TGA;
            if ( Filename.EndsWith( ".dds" ) ) return TRFileType.DDS;
            if ( Filename.EndsWith( ".geo" ) ) return TRFileType.GEO;
            if ( Filename.EndsWith( ".anm" ) ) return TRFileType.ANM;
            if ( Filename.EndsWith( ".jpg" ) ) return TRFileType.JPG;
            if ( Filename.EndsWith( ".mp3" ) ) return TRFileType.MP3;
            if ( Filename.EndsWith( ".ogg" ) ) return TRFileType.OGG;
            if ( Filename.EndsWith( ".wav" ) ) return TRFileType.WAV;
            if ( Filename.EndsWith( ".xml" ) ) return TRFileType.XML;
            if ( Filename.EndsWith( ".fxc" ) ) return TRFileType.FXC;
            return TRFileType.Other;
        }

        public byte[] GetContents()
        {
            BinaryReader reader = new BinaryReader( File.Open( Parent.Path, FileMode.Open, FileAccess.Read ) );
            reader.BaseStream.Seek( Offset, SeekOrigin.Begin );
            byte[] buffer = reader.ReadBytes( (Int32)Size );
            reader.Close();

            if ( IsCompressed )
            {
                using ( MemoryStream compressedStream = new MemoryStream( buffer ) )
                {
                    using ( Ionic.Zlib.ZlibStream zlibStream = new Ionic.Zlib.ZlibStream( compressedStream, Ionic.Zlib.CompressionMode.Decompress ) )
                    {
                        byte[] decompressed = new byte[ SizeUncompressed ];
                        zlibStream.Read( decompressed, 0, (Int32)SizeUncompressed );
                        return decompressed;
                    }
                }
            }

            return buffer;
        }
    }
}