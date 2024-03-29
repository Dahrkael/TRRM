﻿using System;
using System.Collections.Generic;
using System.IO;

namespace TRRM
{
    public class CPDFChunk : Chunk
    {
        public int Id { get; set; }
        public int ParentBoneIndex { get; set; }
        public Quat Orientation { get; set; }
        public Vertex Position { get; set; }
        public List< Tuple< string, string > > KPVector { get; set; }

        public override bool Load( BinaryReader reader )
        {
            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1 ) )
            {
                return false;
            }

            Id = reader.ReadInt32();
            ParentBoneIndex = reader.ReadInt32();
            Orientation = new Quat( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() );
            Position = new Vertex( reader.ReadSingle(),reader.ReadSingle(), reader.ReadSingle() );

            var stringPairCount = reader.ReadInt32();
            if ( stringPairCount > 0 )
            {
                KPVector = new List< Tuple< string, string > >( stringPairCount );
            }

            for ( var i = 0; i < stringPairCount; ++i )
            {
                KPVector.Add( new Tuple< string, string >( reader.ReadCString(), reader.ReadCString() ) );
            }

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.phyCPDefinitionImpl;
        }
    }
}
