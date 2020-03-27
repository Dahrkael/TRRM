using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class CPFXChunk : Chunk
    {
        public PFXDChunk PrecompiledFXData;
        public BIFXChunk ShaderData;

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 3 ) )
            {
                return false;
            }

            PrecompiledFXData = new PFXDChunk();
            if ( !PrecompiledFXData.Load( reader ) )
            {
                return false;
            }

            ShaderData = new BIFXChunk();
            if ( !ShaderData.Load( reader ) )
            {
                return false;
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.effCompiledEffect;
        }
    }
}