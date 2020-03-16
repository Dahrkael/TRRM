using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public enum ParamType : Int32
    {
        Bool = 1,
        Integer = 2,
        Vector = 3,
        String = 4,
        assID = 5,
        assID2 = 8
    }

    public class PARMChunk : Chunk
    {
        public string Key;
        public List<object> Values;
        public ParamType ValueType;

        public override bool Load( BinaryReader reader )
        {
            Values = new List<object>();

            Start( reader );
            if ( !ReadHeader( reader ) || !IsValidVersion( 1, 2 ) )
            {
                return false;
            }

            Key = reader.ReadCString();
            ValueType = (ParamType)reader.ReadInt32();

            // there seems to be no other versions than 2 in the files
            switch ( Header.Version )
            {
                case 2:
                    switch( ValueType )
                    {
                        case ParamType.Bool:
                            {
                                bool value = reader.ReadUInt32() == 1 ? true : false;
                                Values.Add( value );
                            }
                            break;
                        case ParamType.Integer:
                            {
                                Int32 value = reader.ReadInt32();
                                Values.Add( value );
                            }
                            break;
                        case ParamType.Vector:
                            {
                                Int32 count = reader.ReadInt32();
                                for(Int32 i = 0; i < count; i++ )
                                {
                                    Values.Add( reader.ReadSingle() );
                                }
                            }
                            break;
                        case ParamType.String:
                            {
                                string value = reader.ReadCString();
                                Values.Add( value );
                            }
                            break;
                        case ParamType.assID:
                        case ParamType.assID2:
                            {
                                assIDChunk value = new assIDChunk();
                                if ( !value.Load( reader ) )
                                {
                                    return false;
                                }
                                Values.Add( value );
                            }
                            break;
                        default:
                            Debugger.Break();
                            break;
                    }
                    break;
                default:
                    Debugger.Break();
                    break;
            }

            LogInfo( "key: " + Key + " values count: " + Values.Count );

            End( reader );
            return true;
        }

        public override ChunkType Type()
        {
            return ChunkType.pfxParameter;
        }
    }
}
