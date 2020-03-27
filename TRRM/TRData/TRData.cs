using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TRRM
{
    public class TRData
    {
        public string Path;
        public List<GLMFile> GLMFiles;
        public SortedDictionary<string, PackedFile> Filesystem;
        public SortedDictionary<string, byte[]> Shaders;

        public TRData( string path )
        {
            FastConsole.WriteLine("Loading data from " + path );
            Path = path;
            Filesystem = new SortedDictionary<string, PackedFile>();
            Shaders = new SortedDictionary<string, byte[]>();

            // find GLM files in the specified path and subfolders
            string[] foundFiles = System.IO.Directory.GetFiles( path, "*.glm", System.IO.SearchOption.AllDirectories );

            GLMFiles = new List<GLMFile>( foundFiles.Length );
            foreach ( string file in foundFiles )
            {
                GLMFile glmFile = new GLMFile( path, file );
                GLMFiles.Add( glmFile );
            }

            // put all the files into a hash for faster access
            foreach( GLMFile glm in GLMFiles )
            {
                foreach( PackedFile file in glm.Files )
                {
                    PackedFile currentFile = null;
                    Filesystem.TryGetValue( file.Filename, out currentFile );

                    if ( currentFile != null )
                    {
                        FastConsole.WriteLine( String.Format( "Duplicated filename {0} in archives {1} and {2}", file.Filename, currentFile.Parent.Filename, glm.Filename ) );
                        continue;
                    }

                    Filesystem.Add( file.Filename, file );
                }
            }
            FastConsole.WriteLine( "Data loaded" );
            FastConsole.Flush();
        }

        public void PreloadShaders()
        {
            FastConsole.WriteLine( "Preloading packed shaders" );
            
            var compiledEffects = Filesystem.Where( kv => kv.Value.GetFileType() == TRFileType.FXC );

            // load only the latest version of each shader
            //string filenameRegex = "^(.*)SP_(.*).fxc$";
            string suffixRegex = "SP_(.*).fxc$";

            Regex replacer = new Regex( suffixRegex );

            // remove suffix to get true filenames and group them
            var groups = compiledEffects.GroupBy( kv => replacer.Replace( kv.Key, "" ) );

            // retrieve only the latest ones
            List<string> latestShaders = new List<string>();
            foreach ( var group in groups )
            {
                var latest = group.OrderByDescending( kv => kv.Key ).First();
                latestShaders.Add( latest.Key );
            }

            // and now load them and put them into the dictionary
            foreach( var effect in latestShaders )
            {
                PackedFile effectFile = Filesystem[ effect ];
                using ( MemoryStream memory = new MemoryStream( effectFile.GetContents() ) )
                {
                    ChunkFile chunkie = new ChunkFile();
                    if ( chunkie.Load( memory ) )
                    {
                        CPFXChunk cpfx = chunkie.Chunks[ 0 ] as CPFXChunk;

                        string key = replacer.Replace( effect, ".fx" );
                        byte[] value = cpfx.ShaderData.Data;
                        Shaders.Add( key, value );
                    }
                    else
                    {
                        FastConsole.WriteLine( "Failed to preload shader: " + effectFile.Filename );
                    }
                }
            }
            FastConsole.WriteLine( "Preloaded " + Shaders.Count + " shaders" );
            FastConsole.Flush();
        }
    }
}
