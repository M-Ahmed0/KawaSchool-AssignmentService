using Application.DTOs;
using Application.Interfaces.IBusinessLogic;
using Application.Interfaces.IInfrastructure.IAzureServices;
using Application.Interfaces.IPresistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.BusinessLogic
{
    public class StudentUploadedAssignmentService : IStudentUploadedAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IStudentUploadedAssignmentRepository _uploadedAssignmentRepository;

        public StudentUploadedAssignmentService(IMapper mapper,
            IConfiguration configuration,
            IAzureBlobService azureBlobService,
            IStudentUploadedAssignmentRepository uploadedAssignmentRepository)
        {
            _mapper = mapper;
            _configuration = configuration;
            _azureBlobService = azureBlobService;
            _uploadedAssignmentRepository = uploadedAssignmentRepository;
        }

        public async Task UploadAssignmentAsync(StudentUploadAssignmentRequestDto assignment, IFormFile file)
        {
            var assign = _mapper.Map<StudentUploadedAssignment>(assignment);
            var fileName = $"{assign.AssignmentId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            assign.FileName = fileName;
            await _uploadedAssignmentRepository.AddAsync(assign);

            var tags = new Dictionary<string, string>
                {
                    { "userid", assign.UserId.ToString() },
                    { "assignmnetId", assign.AssignmentId.ToString() },
                    { "uploadedassignmnetId", assign.Id.ToString() },
                };

            await using (var stream = file.OpenReadStream())
            {
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                await _azureBlobService.Upload(memoryStream, _configuration["AzureStorage:UploadedAssignmentContainer"], fileName, tags);
            }

        }
        public async Task<IList<StudentUploadedAssignmentDto>> GetUploadedAssignmentByAssignmentIdAsync(string assignmentId, string? userId = null)
        {
            ObjectId objectId = ObjectId.Parse(assignmentId);
            IEnumerable<StudentUploadedAssignment> results;
            if (userId is not null)
                results = await _uploadedAssignmentRepository.FindBy(x => x.AssignmentId == objectId.ToString() && x.UserId == userId);
            else
                results = await _uploadedAssignmentRepository.FindBy(x => x.AssignmentId == objectId.ToString());

            var assignmentsDtos = _mapper.Map<IList<StudentUploadedAssignmentDto>>(results);

            foreach (var assignmentsDto in assignmentsDtos)
            {
                var sasUrl = await _azureBlobService.GetServiceSasUriForBlob(_configuration["AzureStorage:UploadedAssignmentContainer"], $"{assignmentsDto.FileName}");
                if (sasUrl is not null)
                    assignmentsDto.UploadedAssignmentSasUrl = sasUrl.ToString();
            }
            return assignmentsDtos;
        }

        public async Task UpdateUploadedAssignmentByIdAsync(UpdateUploadedAssignmentDto uploadedAssignment)
        {
            ObjectId objectId = ObjectId.Parse(uploadedAssignment.Id);

            var results = await _uploadedAssignmentRepository.GetSingleAsync(objectId);
            if (results is not null)
            {
                results.Status = (StudentUploadedAssignmentStatus)Enum.Parse(typeof(StudentUploadedAssignmentStatus), uploadedAssignment.Status);
                await _uploadedAssignmentRepository.UpdateAsync(results);
            }    
        }

        public async Task<bool> DeleteUploadedAssignmentAsync(ObjectId uploadedAssignmentId)
        {
            try
            {
                var assignment = await _uploadedAssignmentRepository.GetSingleAsync(uploadedAssignmentId);
                if (!string.IsNullOrEmpty(assignment.FileName))
                    await _azureBlobService.DeleteBlob(_configuration["AzureStorage:UploadedAssignmentContainer"], $"{assignment.FileName}");

                await _uploadedAssignmentRepository.DeleteAsync(uploadedAssignmentId);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
