using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KUtility;
using Paloma;
using System.Text.RegularExpressions;

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
                    case TRFileType.DDS:
                        try
                        {
                            DDSImage dds = new DDSImage( file.GetContents() );
                            pbPreview.Image = dds.images.First();
                        }
                        catch ( Exception )
                        {
                            pbPreview.Image = null;
                        }
                        break;
                    case TRFileType.TGA:
                        using ( MemoryStream memory = new MemoryStream( file.GetContents() ) )
                        {
                            TargaImage tga = new TargaImage( memory );
                            pbPreview.Image = tga.Image;
                        }
                        break;
                    case TRFileType.JPG:
                        using ( MemoryStream stream = new MemoryStream( file.GetContents() ) )
                        {
                            pbPreview.Image = new Bitmap( stream );
                        }
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
                if ( file.GetFileType() == TRFileType.GEO )
                {
                    using ( MemoryStream memory = new MemoryStream( file.GetContents() ) )
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
                                    piece = (child as GSKNChunk).Geometry;
                                }

                                if ( child is GPCEChunk )
                                {
                                    piece = child as GPCEChunk;
                                }

                                if ( piece != null )
                                {
                                    viewer3D.CreateMesh( piece.IndexBuffer.Faces, piece.VertexBuffer.Vertices, piece.VertexBuffer.Normals, 
                                        piece.BoundingBox.VertexMin, piece.BoundingBox.VertexMax , piece.BoundingBox.Origin );
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show( "FAIL" );
                        }
                    }
                }
                else
                {
                    MessageBox.Show( "nope, not geo." );
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
            UInt32 good = 0;
            UInt32 bad = 0;

            foreach ( GLMFile glmFile in trData.GLMFiles )
            {
                foreach ( PackedFile trFile in glmFile.Files )
                {
                    if ( trFile.GetFileType() == TRFileType.GEO )
                    {
                        using ( MemoryStream memory = new MemoryStream( trFile.GetContents() ) )
                        {
                            Console.WriteLine( "### {0}", trFile.Filename );
                            ChunkFile chunkie = new ChunkFile();
                            if ( chunkie.Load( memory ) )
                            {
                                good++;
                            }
                            else
                            {
                                bad++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine( "good: {0} bad: {1}", good, bad );
        }
    }
}
