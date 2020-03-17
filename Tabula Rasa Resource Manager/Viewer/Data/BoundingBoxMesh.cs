using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    class BoundingBoxMesh : Mesh
    {
        // bb info
        // FIXME: take origin into account
        public Vector3 VMin { get; set; }
        public Vector3 VMax { get; set; }

        public BoundingBoxMesh( D3D9.Device device )
            : base( device )
        {
            Primitive = D3D9.PrimitiveType.LineList;
        }

        ~BoundingBoxMesh()
        {
        }

        public override void CreateIndexBuffer( List<Face> faces )
        {
            // unused
        }

        public override void CreateVertexBuffer( List<Vertex> vertices, List<Vertex> normals = null )
        {
            // unused
        }

        public override void CreateVertexDeclaration()
        {
            var vertexElems = new[] {
                new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                new D3D9.VertexElement(0, 16, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                D3D9.VertexElement.VertexDeclarationEnd
            };

            VertexDecl = new D3D9.VertexDeclaration( device, vertexElems );
            VertexDeclStride = 16 + 16;
        }

        public void Create( Vector3 vMin, Vector3 vMax, Vector3 origin )
        { 
            VMin = vMin;
            VMax = vMax;

            Vector4 color = new Vector4( 0.0f, 1.0f, 0.0f, 1.0f );

            // create vertices

            Vector4[] vertices = new Vector4[] {
                new Vector4( vMin.X, vMin.Y, vMin.Z, 1.0f ), color,
                new Vector4( vMax.X, vMin.Y, vMin.Z, 1.0f ), color,
                new Vector4( vMax.X, vMax.Y, vMin.Z, 1.0f ), color,
                new Vector4( vMin.X, vMax.Y, vMin.Z, 1.0f ), color,
                new Vector4( vMin.X, vMin.Y, vMax.Z, 1.0f ), color,
                new Vector4( vMax.X, vMin.Y, vMax.Z, 1.0f ), color,
                new Vector4( vMax.X, vMax.Y, vMax.Z, 1.0f ), color,
                new Vector4( vMin.X, vMax.Y, vMax.Z, 1.0f ), color
            };
            
            VertexBuffer = new D3D9.VertexBuffer( device, sizeof( float ) * 8 * vertices.Length, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
            VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( vertices );
            VertexBuffer.Unlock();

            VertexCount = vertices.Length;

            //create indices

            int[] indices = new int[ 24 ];
            int i = 0;
            for ( int n = 0; n < 4; n++ )
            {
                indices[ i++ ] = n;
                indices[ i++ ] = ( n + 1 ) % 4;

                indices[ i++ ] = n + 4;
                indices[ i++ ] = ( n + 1 ) % 4 + 4;

                indices[ i++ ] = n;
                indices[ i++ ] = n + 4;
            }

            IndexBuffer = new D3D9.IndexBuffer( device, sizeof(int) * 24, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, false );
            IndexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( indices );
            IndexBuffer.Unlock();

            IndexCount = indices.Length;

            CreateVertexDeclaration();

            Ready = true;
        }
    }
}
