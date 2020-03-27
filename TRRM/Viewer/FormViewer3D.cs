using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Mathematics;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;
using D3D9 = SharpDX.Direct3D9;

namespace TRRM.Viewer
{
    class EffectBlock : IDisposable
    {
        public D3D9.Effect Effect { get; set; }

        public EffectBlock( D3D9.Effect effect )
        {
            Effect = effect;

            Effect.Technique = Effect.GetTechnique( 0 );
            Effect.Begin();
            Effect.BeginPass( 0 );
        }

        public void Dispose()
        {
            Effect.EndPass();
            Effect.End();
        }
    }

    class DX
    {
        public D3D9.Direct3D Direct3D { get; private set; }
        public D3D9.Device Device { get; private set; }
        public object GlobalLock { get; private set; }

        public DX( D3D9.Direct3D direct3D, D3D9.Device device )
        {
            Direct3D = direct3D;
            Device = device;
            GlobalLock = new object();

            if ( Device.Capabilities.PixelShaderVersion < new Version( 2, 0 ) ||
                Device.Capabilities.VertexShaderVersion < new Version( 2, 0 ) )
            {
                MessageBox.Show( "This computer doesn't support Vertex and Pixel Shader version 2. Get with the times." );
                Application.Exit();
            }
        }

        public void Dispose()
        {
            lock ( GlobalLock )
            {
                Device.Dispose();
                Direct3D.Dispose();
            }
        }
    }

    class FormViewer3D : RenderForm
    {
        public DX DX { get; private set; }

        private Thread RenderThread;

        // for showing images
        private D3D9.Sprite sprite;
        private Matrix spriteTransform;
        private D3D9.Texture texture;
        // for showing models
        private List<Data.Mesh> meshes;
        // for testing
        private Data.BoundingBoxMesh testCube;

        private D3D9.Effect basicEffect;
        private D3D9.Effect lightEffect;

        public FormViewer3D()
            : base( "TRRM - 3D Viewer" )
        {
            Icon = TRRM.Properties.Resources.trrm;
            ClientSize = new System.Drawing.Size( 1280, 720 );//( 800, 600 );
            AllowUserResizing = false;

            D3D9.Direct3D direct3D = new D3D9.Direct3D();
            DX = new DX(
                direct3D,
                new D3D9.Device( direct3D, 0, D3D9.DeviceType.Hardware, Handle,
                 D3D9.CreateFlags.HardwareVertexProcessing, new D3D9.PresentParameters( ClientSize.Width, ClientSize.Height ) )
            );

            meshes = new List<Data.Mesh>();

            sprite = new D3D9.Sprite( DX.Device );
            basicEffect = D3D9.Effect.FromString( DX.Device, Data.Shader.Basic, D3D9.ShaderFlags.None );
            lightEffect = D3D9.Effect.FromString( DX.Device, Data.Shader.TexturePhong, D3D9.ShaderFlags.None );
        }

        ~FormViewer3D()
        {
            lock ( DX.GlobalLock )
            {
                ClearDisplay();
                sprite.Dispose();
                basicEffect.Dispose();
                lightEffect.Dispose();
                DX.Dispose();
            }
        }

        public new void Show()
        {
            base.Show();
            RenderThread = new Thread( this.RenderFunction );
            RenderThread.Start();
        }

        public void ClearDisplay()
        {
            lock ( DX.GlobalLock )
            {
                if ( testCube != null )
                {
                    testCube.Dispose();
                    testCube = null;
                }

                if ( texture != null )
                {
                    texture.Dispose();
                    texture = null;
                    spriteTransform = Matrix.Identity;
                }

                meshes.ForEach( m => m.Dispose() );
                meshes.Clear();
            }
        }

        public void DisplayTexture( byte[] textureData )
        {
            ClearDisplay();

            D3D9.ImageInformation textureInfo;
            lock ( DX.GlobalLock )
            {
                textureInfo = D3D9.ImageInformation.FromMemory( textureData );
                texture = D3D9.Texture.FromMemory( DX.Device, textureData );
            }
            // center the texture
            float cw = ClientSize.Width / 2.0f;
            float ch = ClientSize.Height / 2.0f;
            Vector2 position = new Vector2( ( ClientSize.Width - textureInfo.Width ) / 2.0f, ( ClientSize.Height - textureInfo.Height ) / 2.0f );
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

        public void DisplayMeshes( List<Data.Mesh> meshList )
        {
            ClearDisplay();
            meshList.ForEach( m => DisplayMesh( m, false ) );
        }

        public void DisplayMesh( Data.Mesh mesh, bool clear = true )
        { 
            if ( clear )
                ClearDisplay();

            meshes.Add( mesh );
        }

        public void CreateTestCube()
        {
            ClearDisplay();

            testCube = new Data.BoundingBoxMesh( DX );
            testCube.Create( new Vector3( -5, -5, -5 ), new Vector3( 5, 5, 5 ), new Vector3( 0, 0, 0 ) );
        }

        private void RenderFunction()
        {
            RenderLoop.RenderCallback callback = new RenderLoop.RenderCallback( RenderCallback );

            using ( var renderLoop = new TRRM.Viewer.RenderLoop( this ) { UseApplicationDoEvents = false } )
            {
                float target = 1.0f / 60.0f;
                DateTime last = DateTime.Now;
                while ( renderLoop.NextFrame() )
                {
                    lock ( DX.GlobalLock )
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

        private void RenderCallback()
        {
            DX.Device.Clear( D3D9.ClearFlags.Target | D3D9.ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0 );
            DX.Device.BeginScene();

            // disable culling because rotation
            DX.Device.SetRenderState( D3D9.RenderState.CullMode, D3D9.Cull.None );
            // wireframe is cool
            DX.Device.SetRenderState( D3D9.RenderState.FillMode, D3D9.FillMode.Wireframe );

            float fov = (float)Math.PI / 4.0f;
            float maxZ = 25.0f;
            
            if ( meshes.Count > 0 )
            {
                float radius = meshes.Max( m => m.BoundingBox.SphereRadius() );
                
                //float cameraView = (float)Math.Tan( fov * 0.5 * ( Math.PI / 180.0 ) );
                //float distance = ( radius * 0.5f ) / cameraView;
                //distance += 0.5f * radius;
                //maxZ = distance;

                maxZ = radius * 1.75f;
            }

            Vector3 eyeVector = new Vector3( 0.0f, 0.0f, -maxZ );
            Vector3 lookAtVector = new Vector3( 0.0f, 0.0f, 0.0f );
            Vector3 upVector = new Vector3( 0.0f, 1.0f, 0.0f );

            Matrix viewMatrix = Matrix.LookAtLH( eyeVector, lookAtVector, upVector );
            Matrix projectionMatrix = SharpDX.Matrix.PerspectiveFovLH( fov, ClientSize.Width / (float)ClientSize.Height, 1.0f, 1000.0f );
            Matrix viewProjMatrix = viewMatrix * projectionMatrix;

            float radians = 0.01047197551f * ( (float)Math.PI / 180.0f );

            drawSprite();
            drawTestCube( viewProjMatrix, radians );
            drawMeshes( eyeVector, viewMatrix, projectionMatrix, radians );

            DX.Device.EndScene();
            DX.Device.Present();
        }

        private void drawTestCube( Matrix viewProjMatrix, float radians )
        {
            if ( testCube == null )
                return;

            using ( EffectBlock effect = new EffectBlock( basicEffect ) )
            {
                testCube.Rotation = testCube.Rotation * Matrix.RotationAxis( Vector3.Up, radians );
                drawMeshTest( Matrix.Identity, viewProjMatrix, testCube, effect.Effect );
            }
        }

        private void drawSprite()
        {
            if ( texture == null )
                return;

            sprite.Transform = spriteTransform;
            sprite.Begin( D3D9.SpriteFlags.AlphaBlend );
            sprite.Draw( texture, new RawColorBGRA( 255, 255, 255, 255 ) );
            sprite.End();
        }


        private void drawMeshes( Vector3 eyeVector, Matrix viewMatrix, Matrix projectionMatrix, float radians )
        {
            if ( meshes.Count == 0 )
                return;

            Data.BoundingBox fullBox = new Data.BoundingBox( meshes.Select( m => m.BoundingBox ).ToList() );
            Matrix worldMatrix = Matrix.Invert( Matrix.Translation( fullBox.Origin ) );
            foreach ( var mesh in meshes )
            {
                if ( mesh.Ready )
                {
                    mesh.Rotation = Matrix.Multiply( mesh.Rotation, Matrix.RotationAxis( Vector3.Up, radians ) );
                    drawMesh( eyeVector, viewMatrix, projectionMatrix, worldMatrix, mesh );
                }
            }
        }

        private void drawMesh( Vector3 eyeVector, Matrix viewMatrix, Matrix projectionMatrix, Matrix worldMatrix, Data.Mesh mesh )
        {
            Matrix viewProjMatrix = viewMatrix * projectionMatrix;
            Matrix worldMatrix2 = worldMatrix * mesh.Rotation * mesh.Position;
            Matrix worldViewProjMatrix = worldMatrix2 * viewProjMatrix;

            Matrix worldInverseTranspose = Matrix.Invert( worldMatrix2 );
            worldInverseTranspose = Matrix.Transpose( worldInverseTranspose );

            using ( EffectBlock effect = new EffectBlock( mesh.Effect ) )
            {
                effect.Effect.SetValue( "gWorldMatrix", worldMatrix2 );
                effect.Effect.SetValue( "gViewMatrix", viewMatrix );
                effect.Effect.SetValue( "gProjectionMatrix", projectionMatrix );
                effect.Effect.SetValue( "gViewProjectionMatrix", worldViewProjMatrix );
                effect.Effect.SetValue( "gInvViewProjectionMatrix", worldInverseTranspose );
                effect.Effect.SetValue( "gWorldEyePos", eyeVector );
                effect.Effect.SetValue( "gFogEnable", false );
                
                //effect.Effect.SetValue( "gMaxWeightsPerVertex", );
                //effect.Effect.SetValue( "gGlobalEnvTexture", );
                //effect.Effect.SetValue( "gLogTexture", );
                //effect.Effect.SetValue( "gShadowDisplacement", );
                //effect.Effect.SetValue( "gModulateScale", );

                effect.Effect.ApplyParameterBlock( mesh.ParameterBlock );
                effect.Effect.CommitChanges();
            }

            DX.Device.SetStreamSource( 0, mesh.VertexBuffer, 0, mesh.VertexDeclStride );
            DX.Device.VertexDeclaration = mesh.VertexDecl;
            DX.Device.Indices = mesh.IndexBuffer;

            Int32 primitiveCount = mesh.IndexCount / ( mesh.Primitive == D3D9.PrimitiveType.LineList ? 2 : 3 );
            DX.Device.DrawIndexedPrimitive( mesh.Primitive, 0, 0, mesh.VertexCount, 0, primitiveCount );
        }

        private void drawMeshesTest( Matrix viewProjMatrix, float radians )
        {
            if ( meshes.Count == 0 )
                return;

            using ( EffectBlock effect = new EffectBlock( lightEffect ) )
            {
                effect.Effect.SetValue( "gAmbientLight", new Color4( 0.5f, 0.5f, 0.5f, 1.0f ) );
                effect.Effect.SetValue( "gDiffuseLight", new Color4( 0.9f, 0.9f, 0.9f, 1.9f ) );
                effect.Effect.SetValue( "gDiffuseVecW", new Vector3( 1.0f, -0.5f, -1.0f ) );

                Data.BoundingBox fullBox = new Data.BoundingBox( meshes.Select( m => m.BoundingBox ).ToList() );
                Matrix worldMatrix = Matrix.Invert( Matrix.Translation( fullBox.Origin ) );
                foreach ( var mesh in meshes )
                {
                    if ( mesh.Ready )
                    {
                        mesh.Rotation = Matrix.Multiply( mesh.Rotation, Matrix.RotationAxis( Vector3.Up, radians ) );
                        drawMeshTest( worldMatrix, viewProjMatrix, mesh, effect.Effect );
                    }
                }
            }
        }

        private void drawMeshTest( Matrix worldMatrix, Matrix viewProjMatrix, Data.Mesh mesh, D3D9.Effect effect )
        {
            Matrix worldMatrix2 = worldMatrix * mesh.Rotation * mesh.Position;
            Matrix worldViewProjMatrix = worldMatrix2 * viewProjMatrix;

            Matrix worldInverseTranspose = Matrix.Invert( worldMatrix2 );
            worldInverseTranspose = Matrix.Transpose( worldInverseTranspose );

            effect.SetValue( "gWorldViewProj", worldViewProjMatrix );
            effect.SetValue( "gWorldInvTrans", worldInverseTranspose );
            effect.SetTexture( "gDiffuseTexture", mesh.DiffuseTexture );
            effect.CommitChanges();

            DX.Device.SetStreamSource( 0, mesh.VertexBuffer, 0, mesh.VertexDeclStride );
            DX.Device.VertexDeclaration = mesh.VertexDecl;
            DX.Device.Indices = mesh.IndexBuffer;

            Int32 primitiveCount = mesh.IndexCount / ( mesh.Primitive == D3D9.PrimitiveType.LineList ? 2 : 3 );
            DX.Device.DrawIndexedPrimitive( mesh.Primitive, 0, 0, mesh.VertexCount, 0, primitiveCount );
        }
    }
}
