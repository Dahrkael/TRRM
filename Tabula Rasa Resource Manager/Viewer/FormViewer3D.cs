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
        public D3D9.Device Device;
        private Thread RenderThread;

        private object d3dLock;
        private List<Data.Mesh> meshes;

        public FormViewer3D()
            : base( "TRRM - 3D Viewer" )
        {
            Icon = TRRM.Properties.Resources.IconTR;
            Size = new System.Drawing.Size( 720, 480 );
            ClientSize = Size;

            Device = new D3D9.Device( new D3D9.Direct3D(), 0, D3D9.DeviceType.Hardware, Handle,
                D3D9.CreateFlags.HardwareVertexProcessing, new D3D9.PresentParameters( ClientSize.Width, ClientSize.Height ) );

            meshes = new List<Data.Mesh>();
            d3dLock = new object();
        }

        public new void Show()
        {
            base.Show();
            RenderThread = new Thread( this.RenderFunction );
            RenderThread.Start();
        }


        public void CreateMesh( List<Face> faces, List<Vertex> vertices, List<Vertex> normals, Vertex vMin, Vertex vMax, Vertex origin )
        {
            lock ( d3dLock )
            {
                Data.Mesh mesh = new Viewer.Data.Mesh( Device );
                mesh.CreateIndexBuffer( faces );
                mesh.CreateVertexBuffer( vertices, normals );
                mesh.CreateVertexDeclaration();
                mesh.Ready = true;

                mesh.CreateBoundingBox( vMin, vMax, origin );

                meshes.Add( mesh );
            }
        }

        private void RenderFunction()
        {
            RenderLoop.RenderCallback callback = new RenderLoop.RenderCallback( () => {
                Device.Clear( D3D9.ClearFlags.Target | D3D9.ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0 );
                Device.BeginScene();

                Device.SetRenderState( D3D9.RenderState.Lighting, true );
                Device.SetRenderState( D3D9.RenderState.Ambient, new Color3( 1.0f, 1.0f, 1.0f ).ToRgba() );

                D3D9.Light dirLight = new D3D9.Light
                {
                    Type = D3D9.LightType.Directional,
                    Diffuse = new RawColor4( 0.5f, 0.5f, 0.5f, 1.0f ),
                    Direction = new Vector3( -1.0f, -0.3f, 1.0f )
                };

                Device.SetLight( 0, ref dirLight );
                Device.EnableLight( 0, true );

                Vector3 EyeVector = new Vector3( 0.0f, 0.0f, -50.0f );
                Vector3 LookAtVector = new Vector3( 0.0f, 0.0f, 0.0f );
                Vector3 UpVector = new Vector3( 0.0f, 1.0f, 0.0f );

                Matrix ViewMatrix = Matrix.LookAtLH( EyeVector, LookAtVector, UpVector );
                Device.SetTransform( D3D9.TransformState.View, ViewMatrix );
                
                Matrix ProjectionMatrix = SharpDX.Matrix.PerspectiveFovLH( (float)Math.PI / 4.0f, 720 / 480, 1.0f, 100.0f );
                Device.SetTransform( D3D9.TransformState.Projection, ProjectionMatrix );

                Matrix WorldMatrix = Matrix.Identity;
                Device.SetTransform( D3D9.TransformState.World, WorldMatrix );

                //Int64 timestamp = Stopwatch.GetTimestamp();
                foreach ( var mesh in meshes )
                {
                    if ( mesh.Ready )
                    {
                        float radians = 0.01047197551f * ( (float)Math.PI / 180.0f );
                        mesh.Rotation = Matrix.Multiply( mesh.Rotation, Matrix.RotationAxis( Vector3.Up, radians ) );
                        mesh.Draw();
                    }
                }

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
                        callback();
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
    }
}
