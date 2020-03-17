using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using SharpDX;
using SharpDX.Mathematics;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer
{
    [StructLayout( LayoutKind.Sequential )]
    struct ExampleVertex
    {
        public Vector3 Position;
        public SharpDX.ColorBGRA Color;
    }

    class FormViewer3D : RenderForm
    {
        private D3D9.Direct3D Direct3D;
        public D3D9.Device Device;

        private Thread RenderThread;
        private object d3dLock;

        // for showing images
        private D3D9.Sprite sprite;
        private Matrix spriteTransform;
        private D3D9.Texture texture;
        // for showing models
        private List<Data.Mesh> meshes;
        // for testing
        private Data.BoundingBoxMesh testCube;

        private D3D9.Effect basicEffect;

        public FormViewer3D()
            : base( "TRRM - 3D Viewer" )
        {
            Icon = TRRM.Properties.Resources.IconTR;
            ClientSize = new System.Drawing.Size( 800, 600 );

            Direct3D = new D3D9.Direct3D();
            Device = new D3D9.Device( Direct3D, 0, D3D9.DeviceType.Hardware, Handle,
                D3D9.CreateFlags.HardwareVertexProcessing, new D3D9.PresentParameters( ClientSize.Width, ClientSize.Height ) );

            meshes = new List<Data.Mesh>();
            d3dLock = new object();

            sprite = new D3D9.Sprite( Device );
            basicEffect = D3D9.Effect.FromString( Device, Data.Shader.Basic, D3D9.ShaderFlags.None );
        }

        ~FormViewer3D()
        {
            if ( texture != null )
                texture.Dispose();

            sprite.Dispose();
            basicEffect.Dispose();
            Device.Dispose();
            Direct3D.Dispose();
        }

        public new void Show()
        {
            base.Show();
            RenderThread = new Thread( this.RenderFunction );
            RenderThread.Start();
        }

        public void DisplayTexture( byte[] textureData )
        {
            lock ( d3dLock )
            {
                if (texture != null)
                    texture.Dispose();

                D3D9.ImageInformation textureInfo = D3D9.ImageInformation.FromMemory( textureData );
                texture = D3D9.Texture.FromMemory( Device, textureData, D3D9.Usage.None, D3D9.Pool.Managed );

                // center the texture
                float cw = ClientSize.Width / 2.0f;
                float ch = ClientSize.Height / 2.0f;
                Vector2 position = new Vector2( ( ClientSize.Width - textureInfo.Width ) / 2.0f, ( ClientSize.Height - textureInfo.Height  ) / 2.0f );
                float scaling = 1.0f;

                // if bigger than 512, scale down and recenter
                if ( textureInfo.Width > 512 || textureInfo.Height > 512 )
                {
                    float ws = 512 / (float)textureInfo.Width;
                    float hs = 512 / (float)textureInfo.Height;
                    scaling = ws < hs ? ws : hs;
                    position = new Vector2( ( ClientSize.Width - 512 ) / 2.0f, ( ClientSize.Height - 512 ) / 2.0f );
                }
                
                spriteTransform = Matrix.AffineTransformation2D( scaling, 0.0f, position );
                Console.WriteLine( "texture w: {0} h: {1} s: {2}", textureInfo.Width, textureInfo.Height, scaling );
            }
        }

        public void CreateMesh( List<Face> faces, List<Vertex> vertices, List<Vertex> normals, Vertex vMin, Vertex vMax, Vertex origin )
        {
            lock ( d3dLock )
            {
                Data.Mesh mesh = new Viewer.Data.Mesh( Device );
                mesh.CreateIndexBuffer( faces );
                mesh.CreateVertexBuffer( vertices, normals );
                mesh.CreateVertexDeclaration();
                //mesh.CreateBoundingBox( vMin, vMax, origin );
                mesh.Origin = Matrix.Translation( new Vector3( -origin.X, -origin.Y, -origin.Z ) );
                mesh.Ready = true;

                meshes.Add( mesh );
            }
        }

        public void CreateTestCube()
        {
            lock ( d3dLock )
            {
                testCube = new Data.BoundingBoxMesh( Device );
                testCube.Create( new Vector3( -5, -5, -5 ), new Vector3( 5, 5, 5 ), new Vector3( 0, 0, 0 ) );
            }
        }

        public void RemoveMeshes()
        {
            lock( d3dLock )
            {
                meshes.Clear();
            }
        }

        private void RenderFunction()
        {
            RenderLoop.RenderCallback callback = new RenderLoop.RenderCallback( () => 
            {
                Device.Clear( D3D9.ClearFlags.Target | D3D9.ClearFlags.ZBuffer, Color.Black, 1.0f, 0 );
                Device.BeginScene();

                float maxZ = 50.0f;
                /*
                if ( meshes.Count > 0 )
                {
                    float maxZ1 = meshes.Max( m => Math.Abs( m.BoundingBox.VMin.Z ) );
                    float maxZ2 = meshes.Max( m => Math.Abs( m.BoundingBox.VMax.Z ) );
                    maxZ = maxZ1 > maxZ2 ? maxZ1 : maxZ2;
                    maxZ = Math.Max( Math.Min( maxZ * 10.0f, 50.0f ), 2.0f );
                }
                */

                Vector3 EyeVector = new Vector3( 0.0f, 0.0f, -maxZ );
                Vector3 LookAtVector = new Vector3( 0.0f, 0.0f, 0.0f );
                Vector3 UpVector = new Vector3( 0.0f, 1.0f, 0.0f );

                Matrix ViewMatrix = Matrix.LookAtLH( EyeVector, LookAtVector, UpVector );
                Matrix ProjectionMatrix = SharpDX.Matrix.PerspectiveFovLH( (float)Math.PI / 4.0f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 1000.0f );
                Matrix ViewProjMatrix = ViewMatrix * ProjectionMatrix;

                var technique = basicEffect.GetTechnique( 0 );
                basicEffect.Technique = technique;
                basicEffect.Begin();
                basicEffect.BeginPass( 0 );

                float radians = 0.01047197551f * ( (float)Math.PI / 180.0f );

                if ( testCube != null )
                {    
                    testCube.Rotation = testCube.Rotation * Matrix.RotationAxis( Vector3.Up, radians );
                    drawMesh( ViewProjMatrix, testCube );
                }
                else
                {
                    if ( texture != null )
                    {
                        sprite.Transform = spriteTransform;
                        sprite.Begin( D3D9.SpriteFlags.AlphaBlend );
                        sprite.Draw( texture, new RawColorBGRA( 255, 255, 255, 255 ) );
                        sprite.End();
                    }
                    else
                    {
                        foreach ( var mesh in meshes )
                        {
                            if ( mesh.Ready )
                            {
                                mesh.Rotation = Matrix.Multiply( mesh.Rotation, Matrix.RotationAxis( Vector3.Up, radians ) );
                                drawMesh( ViewProjMatrix, mesh );
                            }
                        }
                    }
                }

                basicEffect.EndPass();
                basicEffect.End();

                Device.EndScene();
                Device.Present();
            } );

            using ( var renderLoop = new TRRM.Viewer.RenderLoop( this ) { UseApplicationDoEvents = false } )
            {
                float target = 1.0f / 60.0f;
                DateTime last = DateTime.Now;
                while ( renderLoop.NextFrame() )
                {
                    lock ( d3dLock )
                    {
                        try
                        {
                            callback();
                        } catch(SharpDXException e)
                        {
                            Console.WriteLine( "DX error occurred: {0}", e.Message );
                        }
                    }
                    DateTime now = DateTime.Now;
                    double elapsed = (now -last ).TotalMilliseconds;
                    if ( elapsed < target )
                    {
                        Thread.Sleep( (int)(target - elapsed) );
                    }
                }
            }
        }

        private void drawMesh( Matrix ViewProjMatrix, Data.Mesh mesh )
        {
            Matrix WorldMatrix = mesh.Origin * mesh.Rotation * mesh.Position;
            Matrix WorldViewProjMatrix = WorldMatrix * ViewProjMatrix;

            basicEffect.SetValue( "worldViewProj", WorldViewProjMatrix );

            Device.SetStreamSource( 0, mesh.VertexBuffer, 0, mesh.VertexDeclStride );
            Device.VertexDeclaration = mesh.VertexDecl;
            Device.Indices = mesh.IndexBuffer;

            Int32 primitiveCount = mesh.IndexCount / ( mesh.Primitive == D3D9.PrimitiveType.LineList ? 2 : 3 );
            Device.DrawIndexedPrimitive( mesh.Primitive, 0, 0, mesh.VertexCount, 0, primitiveCount );
        }
    }
}
