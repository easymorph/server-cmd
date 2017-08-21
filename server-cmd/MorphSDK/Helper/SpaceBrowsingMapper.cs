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
        public static SpaceBrowsingInfo MapFromDto (SpaceBrowsingResponseDto dto)
        {
            return new SpaceBrowsingInfo()
            {
                Files = dto.Files?.Select(Map).ToList(),
                Folders = dto.Folders?.Select(Map).ToList()
            };
        }

        private static SpaceFileInfo Map(SpaceFileItemDto dto)
        {
            return new SpaceFileInfo
            {
                Extension = dto.Extension,
                FileSizeBytes = dto.FileSizeBytes,
                LastModified = DateTimeOffset.Parse(dto.LastModified),
                Name = dto.Name
            };
        }
        private static SpaceFolderInfo Map(SpaceFolderItemDto  dto)
        {
            return new SpaceFolderInfo
            {              
                LastModified = DateTimeOffset.Parse(dto.LastModified),
                Name = dto.Name
            };
        }
    }
}
