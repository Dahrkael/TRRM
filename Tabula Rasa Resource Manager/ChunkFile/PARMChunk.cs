using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class PARMChunk : Chunk
    {
        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2, 3, 4, 5, 7, 8, 9 ) )
            {
                return false;
            }

            string algo = reader.ReadCString();
            Int32 otro = reader.ReadInt32();

            switch ( Header.Version )
            {
                case 1:
                    reader.ReadInt32();
                    break;
                case 2:
                    reader.ReadInt32();
                    break;
                case 3:
                    reader.ReadInt32();
                    break;
                case 4:
                    reader.ReadCString();
                    break;
                case 5:
                case 7:
                case 8:
                case 9:
                    /*
                    if ( something == 2 )
                    {
                        // assID
                    }
                    else
                    {
                        reader.ReadCString();
                    }
                    */
                    break;
            }

            LogInfo( "unk1: " + algo + " unk2: " + otro );
            Skip( reader );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.pfxParameter;
        }
    }
}
