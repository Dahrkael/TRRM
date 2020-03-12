using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class BVACChunk : Chunk
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
