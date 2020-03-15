using SharpDX.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TRRM.Viewer
{
    public class RenderLoop : IDisposable
    {
        private IntPtr controlHandle;
        private Control control;
        private bool isControlAlive;
        private bool switchControl;

        public RenderLoop() { }

        public RenderLoop( Control control )
        {
            Control = control;
        }

        public Control Control
        {
            get
            {
                return control;
            }
            set
            {
                if ( control == value ) return;

                // Remove any previous control
                if ( control != null && !switchControl )
                {
                    isControlAlive = false;
                    control.Invoke( (MethodInvoker)( () => {
                        control.Disposed -= ControlDisposed;
                    } ) );
                    controlHandle = IntPtr.Zero;
                }

                if ( value != null && value.IsDisposed )
                {
                    throw new InvalidOperationException( "Control is already disposed" );
                }

                control = value;
                switchControl = true;
            }
        }

        public bool UseApplicationDoEvents { get; set; }

        public bool NextFrame()
        {
            // Setup new control
            // TODO this is not completely thread-safe. We should use a lock to handle this correctly
            if ( switchControl && control != null )
            {
                control.Invoke( (MethodInvoker)( () => {
                    controlHandle = control.Handle;
                    control.Disposed += ControlDisposed;
                } ) );
                isControlAlive = true;
                switchControl = false;
            }

            if ( isControlAlive )
            {
                if ( UseApplicationDoEvents )
                {
                    // Revert back to Application.DoEvents in order to support Application.AddMessageFilter
                    // Seems that DoEvents is compatible with Mono unlike Application.Run that was not running
                    // correctly.
                    Application.DoEvents();
                }
                else
                {
                    var localHandle = controlHandle;
                    if ( localHandle != IntPtr.Zero )
                    {
                        // Previous code not compatible with Application.AddMessageFilter but faster then DoEvents
                        NativeMessage msg;
                        while ( Win32Native.PeekMessage( out msg, IntPtr.Zero, 0, 0, 0 ) != 0 )
                        {
                            if ( Win32Native.GetMessage( out msg, IntPtr.Zero, 0, 0 ) == -1 )
                            {
                                throw new InvalidOperationException( String.Format( CultureInfo.InvariantCulture,
                                    "An error happened in rendering loop while processing windows messages. Error: {0}",
                                    Marshal.GetLastWin32Error() ) );
                            }

                            // NCDESTROY event?
                            if ( msg.msg == 130 )
                            {
                                isControlAlive = false;
                            }

                            var message = new Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
                            if ( !Application.FilterMessage( ref message ) )
                            {
                                Win32Native.TranslateMessage( ref msg );
                                Win32Native.DispatchMessage( ref msg );
                            }
                        }
                    }
                }
            }

            return isControlAlive || switchControl;
        }

        private void ControlDisposed( object sender, EventArgs e )
        {
            isControlAlive = false;
        }

        public void Dispose()
        {
            Control = null;
        }

        public delegate void RenderCallback();

        public static void Run( ApplicationContext context, RenderCallback renderCallback )
        {
            Run( context.MainForm, renderCallback );
        }

        public static void Run( Control form, RenderCallback renderCallback, bool useApplicationDoEvents = false )
        {
            if ( form == null ) throw new ArgumentNullException( "form" );
            if ( renderCallback == null ) throw new ArgumentNullException( "renderCallback" );

            form.Show();
            using ( var renderLoop = new RenderLoop( form ) { UseApplicationDoEvents = useApplicationDoEvents } )
            {
                while ( renderLoop.NextFrame() )
                {
                    renderCallback();
                }
            }
        }

        public static bool IsIdle
        {
            get
            {
                NativeMessage msg;
                return (bool)( Win32Native.PeekMessage( out msg, IntPtr.Zero, 0, 0, 0 ) == 0 );
            }
        }
    }
}
