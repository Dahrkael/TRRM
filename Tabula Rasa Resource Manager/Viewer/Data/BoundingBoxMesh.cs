using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    class BoundingBoxMesh
    {
        public bool Ready { get; set; }

        private D3D9.Device device;

        private D3D9.VertexDeclaration vertexDecl;
        private Int32 vertexDeclStride;

        // buffers
        private D3D9.IndexBuffer indexBuffer;
        private Int32 indexCount;
        private D3D9.VertexBuffer vertexBuffer;
        private Int32 vertexCount;

        // matrices
        public Matrix Origin { get; set; }
        public Matrix Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        // bb info
        // FIXME: take origin into account
        public Vector3 VMin { get; set; }
        public Vector3 VMax { get; set; }

        public BoundingBoxMesh( D3D9.Device device )
        {
            this.device = device;
            Origin = Matrix.Identity;
            Position = Matrix.Identity;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;
        }

        ~BoundingBoxMesh()
        {
            if ( indexBuffer != null )
                indexBuffer.Dispose();
            if ( vertexBuffer != null )
                vertexBuffer.Dispose();
            if ( vertexDecl != null )
                vertexDecl.Dispose();
        }

        private void CreateVertexDeclaration()
        {
            var vertexElems = new[] {
                new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                new D3D9.VertexElement(0, 16, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                D3D9.VertexElement.VertexDeclarationEnd
            };

            vertexDecl = new D3D9.VertexDeclaration( device, vertexElems );
            vertexDeclStride = 16 + 16;
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
            
            vertexBuffer = new D3D9.VertexBuffer( device, sizeof( float ) * 8 * vertices.Length, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
            vertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( vertices );
            vertexBuffer.Unlock();

            vertexCount = vertices.Length;

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

            indexBuffer = new D3D9.IndexBuffer( device, sizeof(int) * 24, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, false );
            indexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( indices );
            indexBuffer.Unlock();

            indexCount = indices.Length;

            CreateVertexDeclaration();

            Ready = true;
        }

        public void Draw()
        {
            D3D9.Material currentMaterial = device.Material;

            D3D9.Material material = new D3D9.Material();
            material.Ambient = new RawColor4( 0.0f, 1.0f, 0.0f, 1.0f );
            device.Material = material;

            Matrix corrected = Matrix.Multiply( Origin, Rotation );
            Matrix positioned = Matrix.Multiply( corrected, Position );
            Matrix WorldMatrix = Matrix.Multiply( positioned, Scale );
            device.SetTransform( D3D9.TransformState.World, WorldMatrix );

            device.SetStreamSource( 0, vertexBuffer, 0, vertexDeclStride );
            device.VertexDeclaration = vertexDecl;
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitive( D3D9.PrimitiveType.LineList, 0, 0, vertexCount, 0, indexCount / 2 );

            // restore state
            device.SetTransform( D3D9.TransformState.World, Matrix.Identity );
            device.Material = currentMaterial;
        }
    }
}
