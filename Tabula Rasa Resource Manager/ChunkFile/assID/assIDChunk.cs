using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class assIDChunk : Chunk
    {
        Int32 ID1;
        string ID2;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );

            Chunk child = new assIDChunk(); // inception

            ChunkType nextChunk = ChunkUtils.PeekNextChunk( reader );
            switch(nextChunk)
            {
                case ChunkType.assIDNone:
                    child = new INONChunk();
                    if ( !child.Load( reader ) )
                        return false;
                    break;
                case ChunkType.assIDInteger:
                    child = new IINTChunk();
                    if ( !child.Load( reader ) )
                        return false;
                    ID1 = ( child as IINTChunk ).ID;
                    break;
                case ChunkType.assIDString:
                    child = new ISTRChunk();
                    if ( !child.Load( reader ) )
                        return false;
                    ID2 = ( child as ISTRChunk ).ID;
                    break;
                default:
                    Debugger.Break();
                    break;
            }

            Header = child.Header;

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.assID;
        }
    }
}
