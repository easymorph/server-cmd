using MorphSDK.Dto;
using MorphSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Helper
{
    internal static class SpaceBrowsingMapper
    {
        public static SpaceBrowsingInfo MapFromDto(SpaceBrowsingResponseDto dto)
        {
            return new SpaceBrowsingInfo()
            {
                Files = dto.Files?.Select(Map).ToList(),
                Folders = dto.Folders?.Select(Map).ToList(),
                NavigationChain = dto.NavigationChain?.Select(Map).ToList(),
                FreeSpaceBytes = dto.FreeSpaceBytes,
                SpaceName = dto.SpaceName

            };
        }

        private static SpaceFileInfo Map(SpaceFileItemDto dto)
        {
            return new SpaceFileInfo
            {
                Extension = dto.Extension,
                FileSizeBytes = dto.FileSizeBytes,
                LastModified = DateTime.Parse(dto.LastModified),
                Name = dto.Name
            };
        }
        private static SpaceFolderInfo Map(SpaceFolderItemDto dto)
        {
            return new SpaceFolderInfo
            {
                LastModified =  DateTime.Parse(dto.LastModified),
                Name = dto.Name
            };
        }
        private static SpaceNavigation Map(SpaceNavigationItemDto dto)
        {
            return new SpaceNavigation
            {
                Path = dto.Path,
                Name = dto.Name
            };
        }
    }
}
