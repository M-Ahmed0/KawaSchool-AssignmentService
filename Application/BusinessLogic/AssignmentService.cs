using Application.DTOs;
using Application.Interfaces.IBusinessLogic;
using Application.Interfaces.IInfrastructure.IAzureServices;
using Application.Interfaces.IPresistence;
using AutoMapper;
using Domain.Entities;
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
    public class AssignmentService : IAssignmentService
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IAzureBlobService _azureBlobService;
        public AssignmentService(IMapper mapper,
            IConfiguration configuration,
            IAssignmentRepository assignmentRepository,
            IAzureBlobService azureBlobService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _assignmentRepository = assignmentRepository;
            _azureBlobService = azureBlobService;
        }

        public async Task CreateAssignmentAsync(AssignmentRequestDto assignment, IFormFile? file)
        {
            
            var assign = _mapper.Map<Assignment>(assignment);

            string? fileName = null;
            if (file != null)
            {
                fileName = $"{assign.CourseId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                assign.FileName = fileName;
            }

            await _assignmentRepository.AddAsync(assign);

            // Check if file is not null and has content
            if (file is not null && file.Length is not 0)
            {
                var tags = new Dictionary<string, string>
                {
                    { "userid", assign.UserId.ToString() },
                    { "courseid", assign.CourseId.ToString() },
                    { "assignmentId", assign.Id.ToString() },
                };

                // Upload the file to Azure Blob Storage
                await using (var stream = file.OpenReadStream())
                {
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    await _azureBlobService.Upload(memoryStream, _configuration["AzureStorage:CreatedAssignmentContainer"], fileName, tags);
                }
            }
        }

        public async Task<IList<AssignmentDto>> GetAssignmentByCourseIdAsync(string courseId)
        {
            var results = await _assignmentRepository.FindBy(x=>x.CourseId == courseId);
            var assignmentsDtos = _mapper.Map<IList<AssignmentDto>>(results);

            foreach (var assignmentsDto in assignmentsDtos)
            {
                var sasUrl = await _azureBlobService.GetServiceSasUriForBlob(_configuration["AzureStorage:CreatedAssignmentContainer"], $"{assignmentsDto.FileName}");
                if (sasUrl is not null)
                    assignmentsDto.AssignmentFileSasUrl = sasUrl.ToString();
            }
            return assignmentsDtos;
        }
        public async Task<bool> DeleteAssignmentAsync(ObjectId assignmentId)
        {
            try
            {
                var assignment = await _assignmentRepository.GetSingleAsync(assignmentId);

                if (!string.IsNullOrEmpty(assignment.FileName))
                    await _azureBlobService.DeleteBlob(_configuration["AzureStorage:CreatedAssignmentContainer"], $"{assignment.FileName}");

                await _assignmentRepository.DeleteAsync(assignmentId);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
