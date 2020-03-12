using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TRRM
{
    public class BVBXChunk : Chunk
    {
        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            throw new NotImplementedException();
            End( reader );
        }

        public override ChunkType Type()
        {
            throw new NotImplementedException();
        }
    }
}
