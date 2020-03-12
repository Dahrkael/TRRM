using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRRM
{
    public class TRData
    {
        public string Path;
        public List<GLMFile> GLMFiles;

        public TRData( string path )
        {
            Path = path;

            // find GLM files in the specified path and subfolders
            string[] foundFiles = System.IO.Directory.GetFiles( path, "*.glm", System.IO.SearchOption.AllDirectories );

            GLMFiles = new List<GLMFile>( foundFiles.Length );
            foreach ( string file in foundFiles )
            {
                GLMFile glmFile = new GLMFile( path, file );
                GLMFiles.Add( glmFile );
            }
        }
    }
}
