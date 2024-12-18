using Application.BusinessLogic;
using Application.DTOs;
using Application.Interfaces.IBusinessLogic;
using Application.Interfaces.IInfrastructure.IAzureServices;
using Application.Interfaces.IPresistence;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentService.UnitTests
{
    public class StudentUploadedAssignmentTests
    {
        private Mock<IMapper> _mockMapper;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IStudentUploadedAssignmentRepository> _mockStudentUploadedAssignmentRepository;
        private Mock<IAzureBlobService> _mockAzureBlobService;
        private IStudentUploadedAssignmentService _studentUploadedAssignmentService;
        private StudentUploadAssignmentRequestDto assignmentRequestDto;
        private Mock<IFormFile> fileMock;

        [SetUp]
        public void Setup()
        {
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAzureBlobService = new Mock<IAzureBlobService>();
            _mockStudentUploadedAssignmentRepository = new Mock<IStudentUploadedAssignmentRepository>();
            

            _studentUploadedAssignmentService = new StudentUploadedAssignmentService(
                _mockMapper.Object,
                _mockConfiguration.Object,
                _mockAzureBlobService.Object,
            _mockStudentUploadedAssignmentRepository.Object);


            // Set up StudentUploadAssignmentRequestDto
            assignmentRequestDto = new StudentUploadAssignmentRequestDto
            {
                UserId = Guid.NewGuid().ToString(), // Generate a new GUID for UserId
                AssignmentId = ObjectId.GenerateNewId().ToString() // Generate a new ObjectId for AssignmentId
            };

            // Set up fileMock
            fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("test.txt");
            fileMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream());
            fileMock.Setup(_ => _.Length).Returns(1024); // Non-zero file size

            // Set up mapper
            _mockMapper.Setup(m => m.Map<StudentUploadedAssignment>(assignmentRequestDto))
                       .Returns(new StudentUploadedAssignment
                       {
                           UserId = assignmentRequestDto.UserId,
                           AssignmentId = assignmentRequestDto.AssignmentId,
                       });

        }

        [Test]
        public async Task UploadAssignmentAsync_WithValidData_ShouldUploadFile()
        {
            // Arrange
            var mockUploadUrl = "https://example.com/mockurl";
            _mockAzureBlobService.Setup(service => service.Upload(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                                 .ReturnsAsync(mockUploadUrl);

            // Act
            await _studentUploadedAssignmentService.UploadAssignmentAsync(assignmentRequestDto, fileMock.Object);

            // Assert
            _mockAzureBlobService.Verify(service => service.Upload(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }


        [Test]
        public async Task UpdateUploadedAssignmentByIdAsync_WithNonExistingAssignment_ShouldNotUpdate()
        {
            // Arrange
            var updateDto = new UpdateUploadedAssignmentDto
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = "New"
            };

            ObjectId objectId = ObjectId.Parse(updateDto.Id);
            _mockStudentUploadedAssignmentRepository.Setup(repo => repo.GetSingleAsync(objectId))
                                                    .ReturnsAsync((StudentUploadedAssignment)null);

            // Act
            await _studentUploadedAssignmentService.UpdateUploadedAssignmentByIdAsync(updateDto);

            // Assert
            _mockStudentUploadedAssignmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<StudentUploadedAssignment>()), Times.Never);
        }


    }
}
