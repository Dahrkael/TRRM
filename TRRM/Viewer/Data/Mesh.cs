﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer.Data
{
    enum TextureType
    {
        DiffuseTexture,
        GlowTexture,
        DetailTexture,
        DecalTexture,
        EmissiveTexture,
        NormalMapTexture,
        GlossMapTexture
    }

    class EffectParameter
    {
        public EffectParameter()
        {

        }
    }

    [StructLayout( LayoutKind.Sequential, Pack = 4 )]
    struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Vector2 UV;
    }

    class BoundingBox
    {
        public Vector3 VMin { get; set; }
        public Vector3 VMax { get; set; }
        public Vector3 Origin { get; set; }

        public BoundingBox()
        {
            VMin = Vector3.Zero;
            VMax = Vector3.Zero;
            Origin = Vector3.Zero;
        }

        public BoundingBox( List<Vector3> vertices )
        {
            float minX = vertices.Min( v => v.X );
            float minY = vertices.Min( v => v.Y );
            float minZ = vertices.Min( v => v.Z );

            float maxX = vertices.Max( v => v.X );
            float maxY = vertices.Max( v => v.Y );
            float maxZ = vertices.Max( v => v.Z );

            VMin = new Vector3( minX, minY, minZ );
            VMax = new Vector3( maxX, maxY, maxZ );
            Origin = ( VMin + VMax ) / 2.0f;
        }

        public BoundingBox( List<BoundingBox> boxes )
        {
            float minX = boxes.Min( b => b.VMin.X );
            float minY = boxes.Min( b => b.VMin.Y );
            float minZ = boxes.Min( b => b.VMin.Z );

            float maxX = boxes.Max( b => b.VMax.X );
            float maxY = boxes.Max( b => b.VMax.Y );
            float maxZ = boxes.Max( b => b.VMax.Z );

            VMin = new Vector3( minX, minY, minZ );
            VMax = new Vector3( maxX, maxY, maxZ );
            Origin = ( VMin + VMax ) / 2.0f;
        }

        public float SphereRadius()
        {
            float maxX = Math.Max( Math.Abs( VMax.X ), Math.Abs( VMin.X ) );
            float maxY = Math.Max( Math.Abs( VMax.Y ), Math.Abs( VMin.Y ) );
            float maxZ = Math.Max( Math.Abs( VMax.Z ), Math.Abs( VMin.Z ) );

            return Math.Max( Math.Max( maxX, maxY ), maxZ );
        }
    }

    class Mesh
    {
        public bool Ready { get; set; }

        protected DX DX;

        public D3D9.Effect Effect { get; protected set; }
        public D3D9.EffectHandle ParameterBlock { get; protected set; }

        public D3D9.VertexDeclaration VertexDecl { get; protected set; }
        public Int32 VertexDeclStride { get; protected set; }

        public D3D9.IndexBuffer IndexBuffer { get; protected set; }
        public Int32 IndexCount { get; protected set; }

        public D3D9.VertexBuffer VertexBuffer { get; protected set; }
        public Int32 VertexCount { get; protected set; }

        public D3D9.PrimitiveType Primitive { get; protected set; }

        // textures
        public Dictionary<TextureType, D3D9.Texture> Textures { get; protected set; }
        public D3D9.Texture DiffuseTexture { get; protected set; }

        // bbox info
        public BoundingBox BoundingBox { get; set; }

        // matrices
        public Matrix Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        public Mesh( DX dx )
        {
            this.DX = dx;
            Position = Matrix.Identity;
            Rotation = Matrix.Identity;
            Scale = Matrix.Identity;

            Primitive = D3D9.PrimitiveType.TriangleList;

            Textures = new Dictionary<TextureType, D3D9.Texture>();
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
            if ( DiffuseTexture != null )
                DiffuseTexture.Dispose();
        }

        public void Create( D3D9.Effect effect, D3D9.EffectHandle parameterBlock, Dictionary<Data.TextureType, D3D9.Texture> textures, byte[] vertexData, Int32 vertexCount, List<Face> faces, D3D9.VertexDeclaration vertexDeclaration, Int32 vertexStride )
        {
            // load effect
            Effect = effect;
            Textures = textures;
            ParameterBlock = parameterBlock;
            
            // setup vertex declaration
            VertexDecl = vertexDeclaration;
            VertexDeclStride = vertexStride;

            // create buffers
            CreateIndexBuffer( faces );
            CreateVertexBuffer( vertexData, vertexCount );

            Ready = true;
        }

        // for testing
        public void Create( List<Vector3> vertices, List<Face> faces, List<Vector2> uvs, List<Color> colors, List<Vector3> normals = null )
        {
            CreateIndexBuffer( faces );
            if ( normals == null )
                normals = ComputeNormals( vertices, faces );
            CreateVertexBuffer( vertices, normals, colors, uvs );
            CreateVertexDeclaration();

            Ready = true;
        }

        public virtual void CreateIndexBuffer( List<Face> faces )
        {
            lock ( DX.GlobalLock )
            {
                IndexBuffer = new D3D9.IndexBuffer( DX.Device, faces.Count * Face.Size, D3D9.Usage.WriteOnly, D3D9.Pool.Managed, true );
                IndexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( faces.ToArray() );
                IndexBuffer.Unlock();
            }
            IndexCount = faces.Count * 3;
        }

        // for testing
        public virtual void CreateVertexBuffer( List<Vector3> vertices, List<Vector3> normals, List<Color> colors, List<Vector2> uvs )
        {
            MeshVertex[] data = new MeshVertex[ vertices.Count ];
            for( int i = 0; i < vertices.Count; i++ )
            {
                MeshVertex vertex = new MeshVertex()
                {
                    Position = vertices[ i ],
                    Normal = normals[ i ],
                    Color = colors[ i ],
                    UV =  uvs[ i ]
                };
                data[ i ] = vertex;
            }

            lock ( DX.GlobalLock )
            {
                VertexBuffer = new D3D9.VertexBuffer( DX.Device, vertices.Count * 36, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
                VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( data );
                VertexBuffer.Unlock();
            }
            VertexCount = vertices.Count;
        }

        public virtual void CreateVertexBuffer( byte[] rawBuffer, Int32 vertexCount )
        {
            lock ( DX.GlobalLock )
            {
                VertexBuffer = new D3D9.VertexBuffer( DX.Device,rawBuffer.Length, D3D9.Usage.WriteOnly, D3D9.VertexFormat.None, D3D9.Pool.Managed );
                VertexBuffer.Lock( 0, 0, D3D9.LockFlags.None ).WriteRange( rawBuffer );
                VertexBuffer.Unlock();
            }
            VertexCount = vertexCount;
        }

        // for testing
        public virtual void CreateVertexDeclaration()
        {
            lock ( DX.GlobalLock )
            {
                var vertexElems = new[] 
                {
                    new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Position, 0),
                    new D3D9.VertexElement(0, 12, D3D9.DeclarationType.Float3, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Normal, 0),
                    new D3D9.VertexElement(0, 24, D3D9.DeclarationType.Color, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.Color, 0),
                    new D3D9.VertexElement(0, 28, D3D9.DeclarationType.Float2, D3D9.DeclarationMethod.Default, D3D9.DeclarationUsage.TextureCoordinate, 0),
                    D3D9.VertexElement.VertexDeclarationEnd
                };

                VertexDecl = new D3D9.VertexDeclaration( DX.Device, vertexElems );
            }
            
            VertexDeclStride = 36;
        }

        public void LoadDiffuseTexture( byte[] data )
        {
            lock ( DX.GlobalLock )
            {
                DiffuseTexture = D3D9.Texture.FromMemory( DX.Device, data );
            }
        }

        public void SetEffectParameters()
        {

        }

        private void LoadTextures()
        {

        }

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
