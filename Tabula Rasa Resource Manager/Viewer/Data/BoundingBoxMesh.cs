using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    struct BBVertex
    {
        public Vector3 Position;
        public Color Color;
    };

    class BoundingBoxMesh : Mesh
    {
        // bb info
        // FIXME: take origin into account
        public Vector3 VMin { get; set; }
        public Vector3 VMax { get; set; }

        public BoundingBoxMesh( DX dx )
            : base( dx )
        {
            Primitive = D3D9.PrimitiveType.LineList;
        }

        ~BoundingBoxMesh()
        {
        }

        public override void CreateIndexBuffer( List<Face> faces )
        {
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

            lock ( DX.GlobalLock )
            {
                IndexBuffer = new D3D9.IndexBuffer( DX.Device, sizeof( int ) * 24, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, false );
                IndexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( indices );
                IndexBuffer.Unlock();
            }
            IndexCount = indices.Length;
        }

        public override void CreateVertexBuffer( List<Vector3> vertices, List<Vector3> normals )
        {
            // unused
        }

        public override void CreateVertexDeclaration()
        {
            lock ( DX.GlobalLock )
            {
                var vertexElems = new[] 
                {
                    new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                    new D3D9.VertexElement(0, 12, D3D9.DeclarationType.Color, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                    D3D9.VertexElement.VertexDeclarationEnd
                };

                VertexDecl = new D3D9.VertexDeclaration( DX.Device, vertexElems );
            }
            VertexDeclStride = 16;
        }

        public void Create( Vector3 vMin, Vector3 vMax, Vector3 origin )
        { 
            VMin = vMin;
            VMax = vMax;

            Color color = Color.LimeGreen;

            // create vertices
            
            BBVertex[] vertices = new BBVertex[] 
            {
                new BBVertex() { Position = new Vector3( vMin.X, vMin.Y, vMin.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMax.X, vMin.Y, vMin.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMax.X, vMax.Y, vMin.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMin.X, vMax.Y, vMin.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMin.X, vMin.Y, vMax.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMax.X, vMin.Y, vMax.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMax.X, vMax.Y, vMax.Z ), Color = color },
                new BBVertex() { Position = new Vector3( vMin.X, vMax.Y, vMax.Z ), Color = color }
            };

            lock ( DX.GlobalLock )
            {
                VertexBuffer = new D3D9.VertexBuffer( DX.Device, vertices.Length * 16, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
                VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( vertices );
                VertexBuffer.Unlock();
            }
            VertexCount = vertices.Length;

            //create indices
            CreateIndexBuffer( null );

            CreateVertexDeclaration();

            Ready = true;
        }
    }
}
