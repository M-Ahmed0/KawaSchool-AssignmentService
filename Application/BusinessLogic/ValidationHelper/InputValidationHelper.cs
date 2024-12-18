using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BusinessLogic.ValidationHelper
{
    public static class InputValidationHelper
    {
        public static bool ValidateAssignmentIdAndUserIdInput(string assignmentId, string? userId)
        {
            if (!ObjectId.TryParse(assignmentId, out ObjectId assignmentIdParsed))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(userId))
            {
                if (!Guid.TryParse(userId, out Guid userIdParsed))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
