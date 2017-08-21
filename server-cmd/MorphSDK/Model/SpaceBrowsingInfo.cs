using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Model
{
    
    public sealed class SpaceBrowsingInfo
    {
        
        public List<SpaceFolderInfo> Folders { get; set; }
        
        public List<SpaceFileInfo> Files { get; set; }

        public SpaceBrowsingInfo()
        {
            Folders = new List<SpaceFolderInfo>();
            Files = new List<SpaceFileInfo>();
        }
    }

    
    public sealed class SpaceFileInfo
    {
        
        public string Name { get; set; }
        
        public string Extension { get; set; }
        
        public long FileSizeBytes { get; set; }
        
        public DateTimeOffset LastModified { get; set; }
    }

    
    public sealed class SpaceFolderInfo
    {
        
        public string Name { get; set; }
        
        public DateTimeOffset LastModified { get; set; }

    }
}
