using MorphSDK.Dto;
using MorphSDK.Dto.Commands;
using MorphSDK.Model;
using MorphSDK.Model.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphSDK.Mappers
{
    internal static class ValidateTasksErrorMapper
    {
        public static ValidateTasksError MapFromDto(ValidateTasksErrorDto dto)
        {
            return new ValidateTasksError()
            {
                FailedTasks = dto.FailedTasks?.Select(Map).ToList()
            };
        }

        private static FailedTaskInfo Map(FailedTaskInfoDto dto)
        {
            return new FailedTaskInfo
            {
                MissingParameters = dto.MissingParameters,
                TaskId = dto.TaskId,
                TaskLocation = dto.TaskLocation,
                TaskSiteUrl = dto.TaskSiteUrl,
                Text = dto.Text
            };
        }
       
    }
}
