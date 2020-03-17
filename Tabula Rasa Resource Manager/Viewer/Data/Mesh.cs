using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
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
        }

        public void Dispose()
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

        public void Create( List<Vector3> vertices, List<Face> faces, List<Vector3> normals = null )
        {
            CreateIndexBuffer( faces );
            if ( normals == null )
                normals = ComputeNormals( vertices, faces );
            CreateVertexBuffer( vertices, normals );
            CreateVertexDeclaration();
        }

        public virtual void CreateIndexBuffer( List<Face> faces )
        {
            IndexBuffer = new D3D9.IndexBuffer( device, faces.Count * Face.Size, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, true );
            IndexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( faces.ToArray() );
            IndexBuffer.Unlock();

            IndexCount = faces.Count * 3;
        }

        public virtual void CreateVertexBuffer( List<Vector3> vertices, List<Vector3> normals )
        {
            MeshVertex[] data = new MeshVertex[ vertices.Count ];
            for( int i = 0; i < vertices.Count; i++ )
            {
                MeshVertex vertex = new MeshVertex()
                {
                    Position = vertices[ i ],
                    Normal = normals[ i ],
                    Color = Color.White,
                };
                data[ i ] = vertex;
            }

            VertexBuffer = new D3D9.VertexBuffer( device, vertices.Count * 28, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
            VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( data );
            VertexBuffer.Unlock();

            VertexCount = vertices.Count;
        }

        public virtual void CreateVertexDeclaration()
        {
            var vertexElems = new[] {
                new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                new D3D9.VertexElement(0, 12, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Normal, 0),
                new D3D9.VertexElement(0, 24, D3D9.DeclarationType.Color, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                D3D9.VertexElement.VertexDeclarationEnd
            };

            VertexDecl = new D3D9.VertexDeclaration( device, vertexElems );
            VertexDeclStride = 28;
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

        private List<Vector3> ComputeNormals( List<Vector3> vertices, List<Face> faces )
        {
            List<Vector3> normals = new List<Vector3>();
            foreach ( Face face in faces )
            {
                Vector3 a = vertices[ face.A ];
                Vector3 b = vertices[ face.B ];
                Vector3 c = vertices[ face.C ];

                // check this is the correct order
                Vector3 u = b - a;
                Vector3 v = c - a;

                Vector3 normal = Vector3.Cross( u, v );
                normal.Normalize();
                normals.Add( normal );
            }

            return normals;
        }

        private List<Vector3> ComputeNormals( List<Vector3> vertices )
        {
            if (vertices.Count % 3 != 0 )
                Debugger.Break();

            List<Vector3> normals = new List<Vector3>();
            for(int i = 0; i < vertices.Count; i += 3 )
            {
                Vector3 a = vertices[ i ];
                Vector3 b = vertices[ i+1 ];
                Vector3 c = vertices[ i+2 ];

                // check this is the correct order
                Vector3 u = b - a;
                Vector3 v = c - a;

                Vector3 normal = Vector3.Cross( u, v );
                normal.Normalize();
                normals.Add( normal );
            }

            return normals;
        }
    }
}
