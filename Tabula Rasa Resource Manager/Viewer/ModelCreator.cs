using SharpDX;
using System;
using System.Collections.Generic;
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
                    Data.Mesh mesh = Generate( (GSKNChunk)child, dx );
                    meshes.Add( mesh );
                }

                if ( child is GPCEChunk )
                {
                    Data.Mesh mesh = Generate( (GPCEChunk)child, dx );
                    meshes.Add( mesh );
                }
            }

            return meshes;
        }

        public static Data.Mesh Generate( GSKNChunk gsknChunk, DX dx )
        {
            return Generate( gsknChunk.Geometry, dx );
        }

        public static Data.Mesh Generate( GPCEChunk gpceChunk, DX dx )
        {
            var faces = gpceChunk.IndexBuffer.Faces;
            var vertices = gpceChunk.VertexBuffer.Vertices;
            var normals = gpceChunk.VertexBuffer.Normals;
            var uvs = gpceChunk.VertexBuffer.UVs;
            var colors = gpceChunk.VertexBuffer.Colors;
            Vertex origin = gpceChunk.BoundingBox.Origin;

            List<Vector3> vertices3 = vertices.Select( v => new Vector3( v.X, v.Y, v.Z ) ).ToList();
            List<Vector3> normals3 = normals == null ? null : normals.Select( n => new Vector3( n.X, n.Y, n.Z ) ).ToList();
            List<Vector2> uvs2 = uvs.Select( uv => new Vector2( uv.U, uv.V ) ).ToList();
            List<Color> colors1 = colors.Select( c => new Color( c ) ).ToList();

            PARMChunk param = gpceChunk.Effect.parms.Where( p => p.Key == "DiffuseTexture" ).FirstOrDefault();

            byte[] textureData;
            if ( param != null )
            {
                string textureName = param.Values[ 0 ].ToString();
                // assuming it exists
                textureData = trData.Filesystem[ textureName ].GetContents();
            }
            else
            {
                // no texture, go with the flow
                textureData = trData.Filesystem["default.dds"].GetContents();
            }

            Data.Mesh mesh = new Viewer.Data.Mesh( dx );
            mesh.Create( vertices3, faces, uvs2, colors1, normals3 );
            mesh.LoadDiffuseTexture( textureData );
            //mesh.CreateBoundingBox( vMin, vMax, origin );
            mesh.Origin = Matrix.Translation( new Vector3( -origin.X, -origin.Y, -origin.Z ) );

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
