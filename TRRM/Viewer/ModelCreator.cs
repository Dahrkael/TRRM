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
            if ( gbodChunk.Children.Count == 0 )
                return meshes;

            foreach ( var child in gbodChunk.Children )
            {
                if ( child is GSKNChunk )
                {
                    GPCEChunk gpceChunk = (child as GSKNChunk).Geometry;

                    // Tabula Rasa uses 'LOD'. Auto Assault uses 'LODLevel'
                    var lods = gpceChunk.USDA.Values.Where( kv => kv.Item1 == "LOD" || kv.Item1 == "LODLevel" ).ToList();
                    if ( lods.Count > 0 && lods[ 0 ].Item2 != "0" )
                        continue;

                    PBONChunk bone = null;
                    if ( !String.IsNullOrEmpty( gpceChunk.BoneName ) )
                    {
                        gbodChunk.Skeleton.Bones.Where( b => b.BoneData.BoneName == gpceChunk.BoneName ).First();
                    }

                    Data.Mesh mesh = Generate( (GSKNChunk)child, bone, dx );
                    meshes.Add( mesh );
                }

                if ( child is GPCEChunk )
                {
                    GPCEChunk gpceChunk = child as GPCEChunk;

                    // Tabula Rasa uses 'LOD'. Auto Assault uses 'LODLevel'
                    var lods = gpceChunk.USDA.Values.Where( kv => kv.Item1 == "LOD" || kv.Item1 == "LODLevel" ).ToList();
                    if ( lods.Count > 0 && lods[ 0 ].Item2 != "0" )
                        continue;

                    PBONChunk bone = null;
                    if ( !String.IsNullOrEmpty( gpceChunk.BoneName ) )
                    {
                        gbodChunk.Skeleton.Bones.Where( b => b.BoneData.BoneName == gpceChunk.BoneName ).First();
                    }

                    Data.Mesh mesh = Generate( gpceChunk, bone, dx );
                    meshes.Add( mesh );
                }
            }

            return meshes;
        }

        public static Data.Mesh Generate( GSKNChunk gsknChunk, PBONChunk bone, DX dx )
        {
            return Generate( gsknChunk.Geometry, bone, dx );
        }

        public static Data.Mesh Generate( GPCEChunk gpceChunk, PBONChunk bone, DX dx )
        {
            var faces = gpceChunk.IndexBuffer.Faces;
            var vertices = gpceChunk.VertexBuffer.Vertices;
            var normals = gpceChunk.VertexBuffer.Normals;
            var uvs = gpceChunk.VertexBuffer.UVs;
            var colors = gpceChunk.VertexBuffer.Colors;
            Vertex origin = gpceChunk.BoundingBox.Origin;
            Vertex vMin = gpceChunk.BoundingBox.VertexMin;
            Vertex vMax = gpceChunk.BoundingBox.VertexMax;

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

            if ( bone != null )
            {             
                // apply bone rotation
                Quaternion boneRotation = new Quaternion( bone.BoneData.Rotation );
                for ( int i = 0; i < vertices3.Count; i++ )
                {
                    vertices3[ i ] = Vector3.Transform( vertices3[ i ], boneRotation );
                }

                // apply bone scaling
                Matrix boneScale = Matrix.Identity;
                boneScale.ScaleVector = new Vector3( bone.BoneData.Scale );
                for ( int i = 0; i < vertices3.Count; i++ )
                {
                    Vector4 temp = Vector3.Transform( vertices3[ i ], boneScale );
                    vertices3[ i ] = new Vector3( temp.X, temp.Y, temp.Z );
                }

                // apply bone translation
                Vector3 boneOffset = new Vector3( bone.BoneData.Translation );
                for ( int i = 0; i < vertices3.Count; i++ )
                {
                    vertices3[ i ] += boneOffset;
                }
            }

            Data.Mesh mesh = new Viewer.Data.Mesh( dx );
            mesh.Create( vertices3, faces, uvs2, colors1, normals3 );
            mesh.LoadDiffuseTexture( textureData );
            mesh.BoundingBox = new Data.BoundingBox( vertices3 );
            //mesh.BoundingBox = new Data.BoundingBox()
            //{
            //    VMin = new Vector3( vMin.X, vMin.Y, vMin.Z ),
            //    VMax = new Vector3( vMax.X, vMax.Y, vMax.Z )
            //};

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
    }
}
