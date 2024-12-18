using Application.DTOs;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IBusinessLogic
{
    public interface IAssignmentService
    {
        Task CreateAssignmentAsync(AssignmentRequestDto assignment, IFormFile? file);
        Task<IList<AssignmentDto>> GetAssignmentByCourseIdAsync(string courseId);
        Task<bool> DeleteAssignmentAsync(ObjectId assignmentId);
    }
}
