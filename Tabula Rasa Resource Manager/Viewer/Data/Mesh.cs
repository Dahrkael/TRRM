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
        public Vector4 Position;
        public Vector4 Color;
    }

    class Mesh
    {
        public bool Ready { get; set; }

        protected D3D9.Device device;

        public D3D9.VertexDeclaration VertexDecl { get; protected set; }
        public Int32 VertexDeclStride { get; protected set; }

        public D3D9.IndexBuffer IndexBuffer { get; protected set; }
        public Int32 IndexCount { get; protected set; }

        public D3D9.VertexBuffer VertexBuffer { get; protected set; }
        public Int32 VertexCount { get; protected set; }

        public D3D9.PrimitiveType Primitive { get; protected set; }

        // textures
        private D3D9.Texture diffuseTexture;
        private D3D9.Texture normalTexture;
        private D3D9.Texture glowTexture;

        // matrices
        public Matrix Origin { get; set; }
        public Matrix Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        public Mesh( D3D9.Device device )
        {
            this.device = device;
            Origin = Matrix.Identity;
            Position = Matrix.Identity;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;

            Primitive = D3D9.PrimitiveType.TriangleList;
        }

        ~Mesh()
        {
            if ( IndexBuffer != null )
                IndexBuffer.Dispose();
            if ( VertexBuffer != null )
                VertexBuffer.Dispose();
            if ( VertexDecl != null )
                VertexDecl.Dispose();
            if ( diffuseTexture != null )
                diffuseTexture.Dispose();
            if ( normalTexture != null )
                normalTexture.Dispose();
            if ( glowTexture != null )
                glowTexture.Dispose();
        }

        public virtual void CreateIndexBuffer( List<Face> faces )
        {
            IndexBuffer = new D3D9.IndexBuffer( device, faces.Count * Face.Size, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, true );
            IndexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( faces.ToArray() );
            IndexBuffer.Unlock();

            IndexCount = faces.Count * 3;
        }

        public virtual void CreateVertexBuffer( List<Vertex> vertices, List<Vertex> normals = null )
        {
            MeshVertex[] data = new MeshVertex[ vertices.Count ];
            for( int i = 0; i < vertices.Count; i++ )
            {
                MeshVertex vertex = new MeshVertex()
                {
                    Position = new Vector4( vertices[ i ].X, vertices[ i ].Y, vertices[ i ].Z, 1.0f ),
                    Color = new Vector4( 1.0f, 1.0f, 1.0f, 1.0f ),
                };
                data[ i ] = vertex;
            }

            VertexBuffer = new D3D9.VertexBuffer( device, vertices.Count * ( 16 * 2), D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
            VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( data );
            VertexBuffer.Unlock();

            VertexCount = vertices.Count;
        }

        public virtual void CreateVertexDeclaration()
        {
            var vertexElems = new[] {
                new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                //new D3D9.VertexElement(0, Vertex.Size, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Normal, 0),
                new D3D9.VertexElement(0, Vertex.Size, D3D9.DeclarationType.Float4, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                D3D9.VertexElement.VertexDeclarationEnd
            };

            VertexDecl = new D3D9.VertexDeclaration( device, vertexElems );
            VertexDeclStride = 16 * 2;
        }

        public void LoadDiffuseTexture( byte[] data )
        {
            diffuseTexture = D3D9.Texture.FromMemory( device, data, D3D9.Usage.WriteOnly, D3D9.Pool.Managed );
        }
        /*
        public void CreateBoundingBox( Vertex vMin, Vertex vMax, Vertex origin )
        {
            BoundingBox = new BoundingBoxMesh( device );
            BoundingBox.Create( new Vector3( vMin.X, vMin.Y, vMin.Z ), new Vector3( vMax.X, vMax.Y, vMax.Z ), new Vector3( origin.X, origin.Y, origin.Z ) );
        }*/
    }
}
