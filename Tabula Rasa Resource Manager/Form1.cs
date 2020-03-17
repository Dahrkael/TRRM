using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TRRM
{
    public partial class Form1 : Form
    {
        TRData trData = null;
        Viewer.FormViewer3D viewer3D = null;

        public Form1()
        {
            InitializeComponent();
            Icon = TRRM.Properties.Resources.IconTR;

            string dataFolder;
            if ( Program.Arguments.TryGetValue( "data", out dataFolder ) )
            {
                loadTRData( dataFolder );
            }

            viewer3D = new Viewer.FormViewer3D();
            viewer3D.Show();
        }

        private void buttonOpen_Click( object sender, EventArgs e )
        {
            DialogResult ret = chooseFolderDialog.ShowDialog();
            if ( ( ret == DialogResult.OK ) && !String.IsNullOrEmpty( chooseFolderDialog.SelectedPath ) )
            {
                loadTRData( chooseFolderDialog.SelectedPath );
            }
        }

        private void loadTRData( string folder )
        {
            this.trData = new TRData( folder );
            fillList();
        }

        private void filesTV_AfterSelect( object sender, TreeViewEventArgs e )
        {
            TreeNode node = filesTV.SelectedNode;
            if ( node.Tag is PackedFile )
            {
                PackedFile file = node.Tag as PackedFile;
                switch ( file.GetFileType() )
                {
                    case TRFileType.GEO:
                        showGEO( file );
                        break;
                    case TRFileType.DDS:
                    case TRFileType.TGA:
                    case TRFileType.JPG:
                        showImage( file );
                        break;
                    default:
                        Console.WriteLine("No handling available for {0} files",  file.GetFileType() );
                        break;
                }
            }
        }

        private void buttonExtract_Click( object sender, EventArgs e )
        {
            TreeNode node = filesTV.SelectedNode;

            if ( node == null )
                return;

            if ( node.Tag is PackedFile )
            {
                PackedFile file = node.Tag as PackedFile;

                saveDialog.FileName = System.IO.Path.GetFileName( file.Filename );
                DialogResult ret = saveDialog.ShowDialog();
                if ( ( ret == DialogResult.OK ) && ( saveDialog.FileName != "" ) )
                {
                    byte[] buffer = file.GetContents();
                    using ( BinaryWriter writer = new BinaryWriter( File.Open( saveDialog.FileName, FileMode.Create, FileAccess.Write ) ) )
                    {
                        writer.Write( buffer );
                    }
                    MessageBox.Show( "Resource extracted!" );
                }
            }
        }

        private void btnTest_Click( object sender, EventArgs e )
        {
            TreeNode node = filesTV.SelectedNode;
            if ( node.Tag is PackedFile )
            {
                PackedFile file = node.Tag as PackedFile;
                using ( MemoryStream memory = new MemoryStream( file.GetContents() ) )
                {
                    ChunkFile chunkie = new ChunkFile();
                    if ( chunkie.Load( memory ) )
                    {
                        MessageBox.Show( "SUCCESS" );
                    }
                    else
                    {
                        MessageBox.Show( "FAIL" );
                    }
                }
            }
        }

        private void btnSearch_Click( object sender, EventArgs e )
        {
            string term = txtSearch.Text;
            fillList( term );
        }

        private void fillList( string searchTerm = null )
        {
            bool filter = !String.IsNullOrEmpty( searchTerm );
            string regex = WildCardToRegular( searchTerm );

            filesTV.BeginUpdate();
            filesTV.Nodes.Clear();

            foreach ( GLMFile glmFile in trData.GLMFiles )
            {
                TreeNode node = new TreeNode();
                node.Text = glmFile.Filename;
                node.Tag = glmFile;
                foreach ( PackedFile trFile in glmFile.Files )
                {
                    if ( filter && !Regex.IsMatch( trFile.Filename, regex ) )
                    {
                        continue;
                    }
                    TreeNode subnode = new TreeNode();
                    subnode.Text = trFile.Filename;
                    subnode.Tag = trFile;
                    node.Nodes.Add( subnode );
                }

                if ( node.Nodes.Count > 0 )
                {
                    filesTV.Nodes.Add( node );
                }
            }
            filesTV.EndUpdate();
        }

        private static string WildCardToRegular( string value )
        {
            if ( String.IsNullOrEmpty( value ) )
                return "";

            return "^.*" + Regex.Escape( value ).Replace( "\\*", ".*" ) + ".*$";
        }

        private void txtSearch_KeyUp( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Return )
            {
                btnSearch.PerformClick();
            }
        }

        private void btnAll_Click( object sender, EventArgs e )
        {
            viewer3D.CreateTestCube();
        }

        private void btnTestPARM_Click( object sender, EventArgs e )
        {
            HashSet<uint> versions = new HashSet<uint>();
            DateTime start = DateTime.Now;
            foreach( PackedFile file in trData.Filesystem.Values )
            {
                if ( file.GetFileType() == TRFileType.GEO )
                {
                    using ( MemoryStream memory = new MemoryStream( file.GetContents() ) )
                    {
                        ChunkFile chunkie = new ChunkFile();
                        if ( chunkie.Load( memory ) && chunkie.Chunks.Count > 0 )
                        {
                            GBODChunk gbod = chunkie.Chunks[ 0 ] as GBODChunk;
                            foreach ( var child in gbod.Children )
                            {
                                GPCEChunk piece = null;

                                if ( child is GSKNChunk )
                                {
                                    piece = ( child as GSKNChunk ).Geometry;
                                }

                                if ( child is GPCEChunk )
                                {
                                    piece = child as GPCEChunk;
                                }

                                if ( piece != null )
                                {
                                    piece.Effect.parms.ForEach( p => versions.Add( p.Header.Version ) );
                                }
                            }
                        }
                    }
                }
            }
            DateTime end = DateTime.Now;
            Console.WriteLine( "Operation took {0}", (end - start).ToString() );
            versions.ToList().ForEach( v => Console.WriteLine( "used v {0}", v ) );
            MessageBox.Show( "FINISHED" );
        }

        private void showImage( PackedFile imageFile )
        {
            viewer3D.DisplayTexture( imageFile.GetContents() );
        }

        private void showGEO( PackedFile geoFile )
        {
            if ( geoFile.GetFileType() != TRFileType.GEO )
                return;

            viewer3D.ClearDisplay();

            using ( MemoryStream memory = new MemoryStream( geoFile.GetContents() ) )
            {
                ChunkFile chunkie = new ChunkFile();
                if ( chunkie.Load( memory ) )
                {
                    GBODChunk gbod = chunkie.Chunks[ 0 ] as GBODChunk;
                    foreach ( var child in gbod.Children )
                    {
                        GPCEChunk piece = null;

                        if ( child is GSKNChunk )
                        {
                            piece = ( child as GSKNChunk ).Geometry;
                        }

                        if ( child is GPCEChunk )
                        {
                            piece = child as GPCEChunk;
                        }

                        if ( piece != null )
                        {
                            viewer3D.CreateMesh( piece.IndexBuffer.Faces, piece.VertexBuffer.Vertices, piece.VertexBuffer.Normals,
                                piece.BoundingBox.VertexMin, piece.BoundingBox.VertexMax, piece.BoundingBox.Origin );
                        }
                    }
                }
                else
                {
                    MessageBox.Show( "Failed to load geometry file" );
                }
            }
        }
    }
}
