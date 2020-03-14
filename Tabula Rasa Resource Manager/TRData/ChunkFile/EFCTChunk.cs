using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class EFCTChunk : Chunk
    {
        assIDChunk assID;
        List<PARMChunk> parms;

        public override bool Load( BinaryReader reader )
        {
            parms = new List<PARMChunk>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 2, 3 ) )
            {
                return false;
            }

            if ( Header.Version == 2 )
            {
                string id = reader.ReadCString();
            }
            else if ( Header.Version == 3 )
            {
                assID = new assIDChunk();
                if ( !assID.Load( reader ) )
                {
                    return false;
                }
            }

            ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
            while ( nextChunk == ChunkType.pfxParameter )
            {
                PARMChunk parm = new PARMChunk();
                if ( !parm.Load( reader ) )
                {
                    return false;
                }
                parms.Add( parm );
                nextChunk = ChunkUtils.PeekNextChunk( reader );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.effEffect;
        }
    }
}
