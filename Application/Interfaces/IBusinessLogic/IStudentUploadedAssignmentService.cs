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
    public interface IStudentUploadedAssignmentService
    {
        Task UploadAssignmentAsync(StudentUploadAssignmentRequestDto assignment, IFormFile file);
        Task<IList<StudentUploadedAssignmentDto>> GetUploadedAssignmentByAssignmentIdAsync(string assingmnetId, string? userId = null);
        Task UpdateUploadedAssignmentByIdAsync(UpdateUploadedAssignmentDto uploadedAssignment);
        Task<bool> DeleteUploadedAssignmentAsync(ObjectId uploadedAssignmentId); 
    }
}
