using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer
{
    class ModelCreator
    {
        public static TRData  trData { get; set; }

        public static List<Data.Mesh> Generate( GBODChunk gbodChunk, DX dx )
        {
            List<Data.Mesh> meshes = new List<Data.Mesh>();

            foreach ( var child in gbodChunk.Children )
            {
                if ( child is GSKNChunk )
                {
                    GPCEChunk gpceChunk = (child as GSKNChunk).Geometry;

                    // Tabula Rasa uses 'LOD'. Auto Assault uses 'LODLevel'
                    var lods = gpceChunk.USDA.Values.Where( kv => kv.Item1 == "LOD" || kv.Item1 == "LODLevel" ).ToList();
                    if ( lods.Count > 0 && lods[ 0 ].Item2 != "0" )
                        continue;

                    Data.Mesh mesh = Generate( (GSKNChunk)child, dx );
                    meshes.Add( mesh );
                }

                if ( child is GPCEChunk )
                {
                    GPCEChunk gpceChunk = child as GPCEChunk;

                    // Tabula Rasa uses 'LOD'. Auto Assault uses 'LODLevel'
                    var lods = gpceChunk.USDA.Values.Where( kv => kv.Item1 == "LOD" || kv.Item1 == "LODLevel" ).ToList();
                    if ( lods.Count > 0 && lods[ 0 ].Item2 != "0" )
                        continue;

                    Data.Mesh mesh = Generate( gpceChunk, dx );
                    meshes.Add( mesh );
                }
            }

            return meshes;
        }

        public static Data.Mesh Generate( GSKNChunk gsknChunk, DX dx )
        {
            return Generate( gsknChunk.Geometry, dx );
        }

        // for testing
        public static Data.Mesh GenerateTest( GPCEChunk gpceChunk, DX dx )
        {
            var faces = gpceChunk.IndexBuffer.Faces;
            var vertices = gpceChunk.VertexBuffer.Vertices;
            var normals = gpceChunk.VertexBuffer.Normals;
            var uvs = gpceChunk.VertexBuffer.UVs;
            var colors = gpceChunk.VertexBuffer.Colors;

            List<Vector3> vertices3 = vertices.Select( v => new Vector3( v.X, v.Y, v.Z ) ).ToList();
            List<Vector3> normals3 = normals == null ? null : normals.Select( n => new Vector3( n.X, n.Y, n.Z ) ).ToList();
            List<Vector2> uvs2 = uvs.Select( uv => new Vector2( uv.U, uv.V ) ).ToList();
            List<Color> colors1 = colors.Select( c => new Color( c ) ).ToList();

            // Auto Assault Hack
            if ( colors1.Count == 0 )
            {
                vertices.ForEach( f => colors1.Add( Color.White ) );
            }
            if ( uvs2.Count == 0 )
            {
                vertices.ForEach( f => uvs2.Add( new Vector2( 0.0f, 0.0f ) ) );
            }

            PARMChunk param = gpceChunk.Effect.parms.Where( p => p.Key == "DiffuseTexture" ).FirstOrDefault();

            byte[] textureData;
            try
            {
                string textureName = param.Values[ 0 ].ToString();
                Console.WriteLine( "loading texture {0}", textureName );
                // assuming it exists
                textureData = trData.Filesystem[ textureName ].GetContents();
            }
            catch ( Exception )
            {
                // no texture, go with the flow
                // Tabula Rasa
                if ( trData.Filesystem.ContainsKey( "default.dds" ) )
                {
                    textureData = trData.Filesystem[ "default.dds" ].GetContents();
                }
                // Auto Assault
                else if ( trData.Filesystem.ContainsKey( "black_dif.dds" ) )
                {
                    textureData = trData.Filesystem[ "black_dif.dds" ].GetContents();
                }
                else
                {
                    textureData = null;
                    Debugger.Break();
                }
            }

            Data.Mesh mesh = new Viewer.Data.Mesh( dx );
            mesh.Create( vertices3, faces, uvs2, colors1, normals3 );
            mesh.LoadDiffuseTexture( textureData );
            mesh.BoundingBox = new Data.BoundingBox( vertices3 );

            return mesh;
        }

        public static Data.Mesh Generate( GPCEChunk gpceChunk, DX dx )
        {
            // vertex declaration
            D3D9.VertexDeclaration vertexDeclaration = Generate( gpceChunk.VertexBuffer.VertexDeclarations, dx );
            Int32 vertexStride = gpceChunk.VertexBuffer.VertexDeclarations.TotalStride;

            // buffers
            var faces = gpceChunk.IndexBuffer.Faces;
            var vertices = gpceChunk.VertexBuffer.Vertices.Select( v =>  new Vector3( v.X, v.Y, v.Z ) ).ToList();
            var vertexBuffer = gpceChunk.VertexBuffer.RawBuffer;

            // effect
            string effectName = gpceChunk.Effect.assID.ToString();
            byte[] effectData = trData.Shaders[ effectName ];

            D3D9.Effect effect;
            D3D9.EffectHandle parameterBlock;
            Dictionary<Data.TextureType, D3D9.Texture> textures;
            lock ( dx.GlobalLock )
            {
                effect = D3D9.Effect.FromMemory( dx.Device, effectData, D3D9.ShaderFlags.None );

                Console.WriteLine( "desc: C: {0} - F: {1} - P: {2} - T: {3}", effect.Description.Creator, effect.Description.Functions, effect.Description.Parameters, effect.Description.Techniques );
                for( int i = 0; i < effect.Description.Functions; i++ )
                {
                    var fd = effect.GetFunctionDescription( effect.GetFunction( i ) );
                    Console.WriteLine( "func: N: {0} - A: {1}", fd.Name, fd.Annotations );
                }
                for ( int i = 0; i < effect.Description.Parameters; i++ )
                {
                    var fd = effect.GetParameterDescription( effect.GetParameter( null, i ) );
                    Console.WriteLine( "param: N: {0} - A: {1} - S {2} - T: {3}", fd.Name, fd.Annotations, fd.Semantic, fd.Type );
                }
                for ( int i = 0; i < effect.Description.Techniques; i++ )
                {
                    var fd = effect.GetTechniqueDescription( effect.GetTechnique( i ) );
                    Console.WriteLine( "tech: {0} - P: {1} - A: {2}", fd.Name, fd.Passes, fd.Annotations );
                }
                Debugger.Break();
                parameterBlock = Generate( gpceChunk.Effect.parms, effect, out textures, dx );
            }

            // create the mesh with all the data
            Data.Mesh mesh = new Viewer.Data.Mesh( dx );
            mesh.Create( effect, parameterBlock, textures, vertexBuffer, vertices.Count, faces, vertexDeclaration, vertexStride );
            
            mesh.BoundingBox = new Data.BoundingBox( vertices );

            return mesh;
        }

        public static D3D9.VertexDeclaration Generate( DECLChunk declChunk, DX dx )
        {
            D3D9.VertexElement[] vertexElements = new D3D9.VertexElement[ declChunk.Declarations.Count + 1 ];
            for(int i = 0; i < declChunk.Declarations.Count; i++ )
            {
                var declElem = declChunk.Declarations[ i ];
                D3D9.VertexElement element = new D3D9.VertexElement( 0, declElem.Offset, (D3D9.DeclarationType)declElem.Type, 
                    (D3D9.DeclarationMethod)declElem.Method, (D3D9.DeclarationUsage)declElem.Usage, declElem.UsageIndex );
                vertexElements[ i ] = element;
            }
            vertexElements[ vertexElements.Length - 1 ] = D3D9.VertexElement.VertexDeclarationEnd;

            lock ( dx.GlobalLock )
            {
                D3D9.VertexDeclaration vertexDecl = new D3D9.VertexDeclaration( dx.Device, vertexElements );
                return vertexDecl;
            }
        }

        public static D3D9.EffectHandle Generate( List<PARMChunk> parmChunks, D3D9.Effect effect, out Dictionary<Data.TextureType, D3D9.Texture> textures, DX dx )
        {
            Dictionary<Data.TextureType, D3D9.Texture>  textureDict = new Dictionary<Data.TextureType, D3D9.Texture>();
            D3D9.EffectHandle parameterBlock;

            effect.BeginParameterBlock();
            foreach( PARMChunk parm in parmChunks )
            {
                switch ( parm.ValueType )
                {
                    // textures
                    case ParamType.assID:
                    case ParamType.assID2:
                        //Debugger.Break();
                        Data.TextureType type = ( Data.TextureType)Enum.Parse( typeof( Data.TextureType ), parm.Key );
                        PackedFile textureFile = null;
                        assIDChunk id = parm.Values.First() as assIDChunk;
                        trData.Filesystem.TryGetValue( id.ToString(), out textureFile );
                        D3D9.Texture texture = D3D9.Texture.FromMemory( dx.Device, textureFile.GetContents() );
                        textureDict.Add( type, texture );
                        effect.SetTexture( parm.Key, texture );
                        break;
                    case ParamType.String:
                        effect.SetString( parm.Key, ( string )parm.Values.First() );
                        break;
                    case ParamType.Bool:
                        effect.SetValue( parm.Key, ( bool )parm.Values.First() );
                        break;
                    case ParamType.Integer:
                        effect.SetValue( parm.Key, ( int )parm.Values.First() );
                        break;
                    case ParamType.Vector:
                        float[] vectorData = new float[ parm.Values.Count ];
                        for( Int32 i = 0; i < parm.Values.Count; i++ )
                        {
                            vectorData[ i ] = ( float )parm.Values[ i ];
                        }
                        effect.SetRawValue( parm.Key, vectorData );
                        break;
                }
            }
            parameterBlock = effect.EndParameterBlock();

            textures = textureDict;
            return parameterBlock;
        }
    }
}
