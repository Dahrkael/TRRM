using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TRRM
{
    public enum ChunkType
    {
        None,
        // assID
        assID,
        assIDInteger, // totally made up
        assIDString, // totally made up
        assIDNone, // totally made up
        // ?!
        AnimationMarkers,
        ConstColorGenerator,
        SfxPackage,
        assCatalog,
        // EFF
        effEffect,
        effVertexDecl,
        effCompiledEffect,
        effPrecompiledFXData,
        effShaderData,
        // ANM
        anmAnimEventsImpl,
        anmAnim,
        anmEventBase,
        anmTrackEvents,
        anmTrack,
        // GFX
        gfxBody,
        gfxGeometryNode,
        gfxGeometryPiece,
        gfxGeometryPieceSkinned,
        gfxIndexBufferImpl,
        gfxMorphWeightArrayImpl,
        gfxMorphedGeometryNode,
        gfxShadowVolume,
        gfxSkinnedGeometryNode,
        gfxVertexBufferImpl,
        // PHY
        phyBVAlignedCylinder,
        phyBVBox,
        phyBVCapsule,
        phyBVSphere,
        phyBVSurface,
        phyBVWalkableSurface,
        phyBone,
        phyBoneSharedData,
        phyBoundingBox,
        phyBoundingVolume,
        phyCPDefinitionGroupImpl,
        phyCPDefinitionImpl,
        phySkeleton,
        // PFX
        pfxAccelerate,
        pfxAddGenerator,
        pfxAlignToVelocity,
        pfxApplyAfter,
        pfxBounce,
        pfxBox,
        pfxBoxPointGenerator,
        pfxCatmullRomBlendGenerator,
        pfxCompositeParticleOperator,
        pfxConstGenerator,
        pfxCylinder,
        pfxDisc,
        pfxDiscBounce,
        pfxDrag,
        pfxDummyOp,
        pfxEmit,
        pfxEmitFromParticle,
        pfxGaussianGenerator,
        pfxGeoGenerator,
        pfxGravity,
        pfxHalfSpace,
        pfxHoldGenerator,
        pfxKeyframeColorOperator,
        pfxKeyframeGenerator,
        pfxLinearBlendGenerator,
        pfxMove,
        pfxParticleGroup,
        pfxParticleGroupSorted,
        pfxParticleSystem,
        pfxPointColorGenerator,
        pfxRangePointGenerator,
        pfxRestore,
        pfxSequence,
        pfxSequenceGenerator,
        pfxSequenceStage,
        pfxSetAttribute,
        pfxSineGenerator,
        pfxSphere,
        pfxSpherePointGenerator,
        pfxTargetAlpha,
        pfxTargetColor,
        pfxTerminateAfter,
        pfxTerminateOutside,
        pfxTerminateWithin,
        pfxUniformGenerator,
        pfxUniformUnaryGenerator,
        pfxVortex,
        //  ????
        pfxParameter, // totally made up
        gfxUSDA // totally made up
    }

    public class ChunkUtils
    {
        static private Dictionary<ChunkType, string> Type2Tag = new Dictionary<ChunkType, string>()
        {
            { ChunkType.assID, "IGEN" }, // totally made up
            { ChunkType.assIDInteger, "IINT" },
            { ChunkType.assIDString, "ISTR" },
            { ChunkType.assIDNone, "INON" },
            { ChunkType.gfxBody, "GBOD" },
            { ChunkType.phyBoundingBox, "BBOX" },
            { ChunkType.phyCPDefinitionGroupImpl, "CPDG" },
            { ChunkType.phyCPDefinitionImpl, "CPDF" },
            { ChunkType.gfxGeometryPiece, "GPCE" },
            { ChunkType.gfxGeometryPieceSkinned, "GSKN" },
            { ChunkType.phySkeleton, "PSKE" },
            { ChunkType.phyBone, "PBON" },
            { ChunkType.phyBoneSharedData, "BDAT" },
            { ChunkType.gfxGeometryNode, "GNOD" },
            { ChunkType.phyBoundingVolume, "BVOL" },
            { ChunkType.phyBVAlignedCylinder, "BVAC" },
            { ChunkType.phyBVBox, "BVBX" },
            { ChunkType.phyBVCapsule, "BVCP" },
            { ChunkType.phyBVSphere, "BVSP" },
            { ChunkType.phyBVSurface, "BVSF" },
            { ChunkType.phyBVWalkableSurface, "BVWS" },
            { ChunkType.gfxIndexBufferImpl, "INDX" },
            { ChunkType.gfxVertexBufferImpl, "VERT" },
            { ChunkType.effVertexDecl, "DECL" },
            { ChunkType.effEffect, "EFCT" },
            { ChunkType.pfxParameter, "PARM" },
            { ChunkType.gfxUSDA, "USDA" },
            { ChunkType.effCompiledEffect, "CPFX" },
            { ChunkType.effPrecompiledFXData, "PFXD" },
            { ChunkType.effShaderData, "BIFX" },
        };

        static public string Tag( ChunkType type )
        {
            return Type2Tag.ContainsKey( type ) ? Type2Tag[ type ] : "NONE";
        }

        static public ChunkType Type( string tag )
        {
            if ( Type2Tag.ContainsValue( tag ) )
            {
                return Type2Tag.Where( kv => kv.Value == tag ).First().Key;
            }
            return ChunkType.None;
        }

        static public Chunk Instance( ChunkType type )
        {
            switch ( type )
            {
                case ChunkType.gfxBody:
                    return new GBODChunk();
                case ChunkType.phyBoundingBox:
                    return new BBOXChunk();
                case ChunkType.phyCPDefinitionGroupImpl:
                    return new CPDGChunk();
                case ChunkType.phyCPDefinitionImpl:
                    return new CPDFChunk();
                case ChunkType.gfxGeometryPiece:
                    return new GPCEChunk();
                case ChunkType.gfxGeometryPieceSkinned:
                    return new GSKNChunk();
                case ChunkType.effCompiledEffect:
                    return new CPFXChunk();
            }

            Debugger.Break();
            return null;
        }

        public static ChunkType PeekNextChunk( BinaryReader reader )
        {
            bool enough = ( ( reader.BaseStream.Length - reader.BaseStream.Position ) >= 4 );
            if ( !enough ) return ChunkType.None;

            string tag = reader.ReadBytesAsString( 4 );
            reader.BaseStream.Seek( -4, SeekOrigin.Current );
            ChunkType type = ChunkUtils.Type( tag );
            if ( type == ChunkType.None )
            {
                FastConsole.WriteLine( String.Format( "Next chunk: {0} ({1})", type, tag ) );
            }
            return type;
        }
    }

    public struct ChunkHeader
    {
        public string Tag;
        public UInt32 Size;
        public UInt32 Version;
        public UInt32 Unknown;
    }

    public abstract class Chunk
    {
        public ChunkHeader Header;

        protected long StartOffset;
        protected long EndOffset;

        public abstract bool Load( BinaryReader reader );
        public abstract ChunkType Type();

        protected bool ReadHeader( BinaryReader reader )
        {
            Header = new ChunkHeader()
            {
                Tag = reader.ReadBytesAsString( 4 ),
                Size = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                Unknown = reader.ReadUInt32()
            };

            string tag = ChunkUtils.Tag( Type() );
            if ( Header.Tag != tag )
            {
                FastConsole.WriteLine( String.Format( "{0}: Invalid tag.", tag ) );
                return false;
            }

            if ( Header.Unknown != 0 )
            {
                Debugger.Break();
            }

            FastConsole.WriteLine( String.Format( "[{0}] Version {1} ", Header.Tag, Header.Version ) );
            return true;
        }

        protected bool IsValidVersion( params UInt32[] validVersions )
        {
            for( int i = 0; i < validVersions.Length; i++ )
            {
                if ( Header.Version == validVersions[i])
                {
                    return true;
                }
            }

            string tag = ChunkUtils.Tag( Type() );
            FastConsole.WriteLine( String.Format( "{0}{1}: Invalid version.", tag, "Chunk" ) );
            return false;
        }

        protected void Start( BinaryReader reader )
        {
            StartOffset = reader.BaseStream.Position;
        }

        protected void End( BinaryReader reader )
        {
            EndOffset = reader.BaseStream.Position;

            long read = EndOffset - StartOffset;
            long shouldBe = Header.Size + 16;
            Debug.Assert( read == shouldBe ); // plus header bytes
        }

        protected void Skip( BinaryReader reader )
        {
            long advanced = reader.BaseStream.Position - StartOffset;
            long toSkip = ( Header.Size + 16 ) - advanced;
            LogInfo("skipping (" + toSkip + " bytes of " + Header.Size + ")");
            reader.BaseStream.Seek( toSkip, SeekOrigin.Current );
        }

        protected void LogInfo( params string[] data )
        {
            for ( int i = 0; i < data.Length; i++ )
            {
                string tag = ChunkUtils.Tag( Type() );
                FastConsole.WriteLine( String.Format( "{0}: {1}", tag, data[ i ] ) );
            }
        }
    }
}
