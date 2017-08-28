using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Dto
{
    [DataContract]
    internal sealed class SpaceBrowsingResponseDto
    {
        [DataMember(Name = "folders")]
        public List<SpaceFolderItemDto> Folders { get; set; }
        [DataMember(Name = "files")]
        public List<SpaceFileItemDto> Files { get; set; }
        [DataMember(Name = "navigationChain")]
        public List<SpaceNavigationItemDto> NavigationChain { get; set; }
        [DataMember(Name = "freeSpaceBytes")]
        public ulong FreeSpaceBytes { get; set; }
        [DataMember(Name = "spaceAccess")]
        public string SpaceAccess { get; set; }
        [DataMember(Name = "spaceName")]
        public string SpaceName { get; set; }        


        public SpaceBrowsingResponseDto()
        {
            Folders = new List<SpaceFolderItemDto>();
            Files = new List<SpaceFileItemDto>();
            NavigationChain = new List<SpaceNavigationItemDto>();
        }
    }

    [DataContract]
    internal sealed class SpaceFileItemDto
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "extension")]
        public string Extension { get; set; }
        [DataMember(Name = "fileSizeBytes")]
        public long FileSizeBytes { get; set; }
        [DataMember(Name = "lastModified")]
        public string LastModified { get; set; }
    }

    [DataContract]
    internal sealed class SpaceFolderItemDto
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "lastModified")]
        public string LastModified { get; set; }

    }

    [DataContract]
    internal sealed class  SpaceNavigationItemDto
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "path")]
        public string Path { get; set; }
        public SpaceNavigationItemDto()
        {
           
        }
    }
}
