using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    struct MeshVertex
    {
        public Vertex Position;
        public Vertex Normal;
        public ColorBGRA Color;
    }

    class Mesh
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

        // textures
        private D3D9.Texture diffuseTexture;
        private D3D9.Texture normalTexture;
        private D3D9.Texture glowTexture;

        // matrices
        public Matrix Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        public Mesh( D3D9.Device device )
        {
            this.device = device;
            Position = Matrix.Identity;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;
        }

        ~Mesh()
        {
            if ( indexBuffer != null )
                indexBuffer.Dispose();
            if ( vertexBuffer != null )
                vertexBuffer.Dispose();
            if ( vertexDecl != null )
                vertexDecl.Dispose();
            if ( diffuseTexture != null )
                diffuseTexture.Dispose();
            if ( normalTexture != null )
                normalTexture.Dispose();
            if ( glowTexture != null )
                glowTexture.Dispose();
        }

        public void CreateIndexBuffer( List<Face> faces )
        {
            indexBuffer = new D3D9.IndexBuffer( device, faces.Count * Face.Size, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, true );
            indexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( faces.ToArray() );
            indexBuffer.Unlock();

            indexCount = faces.Count * 3;
        }

        public void CreateVertexBuffer( List<Vertex> vertices, List<Vertex> normals )
        {
            MeshVertex[] data = new MeshVertex[ vertices.Count ];
            for( int i = 0; i < vertices.Count; i++ )
            {
                MeshVertex vertex = new MeshVertex()
                {
                    Position = vertices[ i ],
                    Normal = normals[ i ],
                    Color = Color.White
                };
                data[ i ] = vertex;
            }

            vertexBuffer = new D3D9.VertexBuffer( device, vertices.Count * ( Vertex.Size * 2 + 4), D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
            vertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( data );
            vertexBuffer.Unlock();

            vertexCount = vertices.Count;
        }

        public void CreateVertexDeclaration()
        {
            var vertexElems = new[] {
                new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                new D3D9.VertexElement(0, Vertex.Size, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Normal, 0),
                new D3D9.VertexElement(0, Vertex.Size * 2, D3D9.DeclarationType.Color, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                D3D9.VertexElement.VertexDeclarationEnd
            };

            vertexDecl = new D3D9.VertexDeclaration( device, vertexElems );
            vertexDeclStride = Vertex.Size * 2 + 4;
        }

        public void LoadDiffuseTexture( byte[] data )
        {
            diffuseTexture = D3D9.Texture.FromMemory( device, data, D3D9.Usage.WriteOnly, D3D9.Pool.Managed );
        }

        public void Draw( D3D9.Device device )
        {
            Matrix WorldMatrix = Matrix.Multiply( Matrix.Multiply(Rotation, Position), Scale );
            device.SetTransform( D3D9.TransformState.World, WorldMatrix );

            device.SetStreamSource( 0, vertexBuffer, 0, vertexDeclStride );
            device.VertexDeclaration = vertexDecl;
            device.Indices = indexBuffer;
            device.DrawIndexedPrimitive( D3D9.PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3 );

            // restore matrix
            device.SetTransform( D3D9.TransformState.World, Matrix.Identity );
        }
    }
}
