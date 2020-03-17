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
            List<Vector3> vertices3 = vertices.Select( v => new Vector3( v.X, v.Y, v.Z ) ).ToList();

            var normals = gpceChunk.VertexBuffer.Normals;
            List<Vector3> normals3 = normals == null ? null : normals.Select( n => new Vector3( n.X, n.Y, n.Z ) ).ToList();

            Vertex origin = gpceChunk.BoundingBox.Origin;

            Data.Mesh mesh = new Viewer.Data.Mesh( dx );
            mesh.Create( vertices3, faces, normals3 );
            //mesh.CreateBoundingBox( vMin, vMax, origin );
            mesh.Origin = Matrix.Translation( new Vector3( -origin.X, -origin.Y, -origin.Z ) );

            return mesh;
        }
    }
}
