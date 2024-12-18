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
using System.Linq.Expressions;

namespace AssignmentService.UnitTests
{
    public class AssignmentServiceTests
    {
        private Mock<IMapper> _mockMapper;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IAssignmentRepository> _mockAssignmentRepository;
        private Mock<IAzureBlobService> _mockAzureBlobService;
        private IAssignmentService _assignmentService;
        private AssignmentRequestDto assignmentDto;
        private Mock<IFormFile> fileMock;

        [SetUp]
        public void Setup()
        {
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockAssignmentRepository = new Mock<IAssignmentRepository>();
            _mockAzureBlobService = new Mock<IAzureBlobService>();

            _assignmentService = new Application.BusinessLogic.AssignmentService(
                _mockMapper.Object,
                _mockConfiguration.Object,
                _mockAssignmentRepository.Object,
                _mockAzureBlobService.Object);


            // Arrange common setup for all tests
            assignmentDto = new AssignmentRequestDto
            {
                UserId = Guid.NewGuid().ToString(),
                CourseId = Guid.NewGuid().ToString(),
                AssignmentDescription = "Sample Assignment Description",
                Created = DateTime.UtcNow,
                Deadline = DateTime.UtcNow.AddDays(7)
            };

            _mockMapper.Setup(m => m.Map<Assignment>(It.IsAny<AssignmentRequestDto>()))
            .Returns((AssignmentRequestDto dto) => new Assignment
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                AssignmentDescription = dto.AssignmentDescription,
                Created = dto.Created,
                Deadline = dto.Deadline,
                FileName = null
            });

            fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("test.txt");
            fileMock.Setup(_ => _.Length).Returns(1024); // Non-empty file size
            fileMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream());
        }

        #region Create Assignment Tests
        [Test]
        public async Task CreateAssignmentAsync_WithValidData_ShouldWorkAsExpected()
        {

            _mockAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<Assignment>())).Returns(Task.CompletedTask);

            fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.FileName).Returns("test.txt");
            fileMock.Setup(_ => _.Length).Returns(1024);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream());

            // Act
            await _assignmentService.CreateAssignmentAsync(assignmentDto, fileMock.Object);

            // Assert
            _mockAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<Assignment>()), Times.Once);
            _mockAzureBlobService.Verify(b => b.Upload(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [Test]
        public async Task CreateAssignmentAsync_WithEmptyFile_ShouldAddAssignmentWithoutUploadingFile()
        {
            // Arrange
            // Setup a file mock with zero length
            var emptyFileMock = new Mock<IFormFile>();
            emptyFileMock.Setup(f => f.Length).Returns(0);

            // Act
            await _assignmentService.CreateAssignmentAsync(assignmentDto, emptyFileMock.Object);

            // Assert
            _mockAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<Assignment>()), Times.Once);
            // Verify that the Upload method on the Azure Blob Service is never called, as the file is empty.
            _mockAzureBlobService.Verify(b => b.Upload(It.IsAny<MemoryStream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Never);
        }

        [Test]
        public void CreateAssignmentAsync_WhenExceptionOccurs_ShouldThrow()
        {
            // Simulate an exception when adding to the repository
            _mockAssignmentRepository.Setup(r => r.AddAsync(It.IsAny<Assignment>())).ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _assignmentService.CreateAssignmentAsync(assignmentDto, fileMock.Object));
        }
        #endregion

        #region Get Assignment Tests
        [Test]
        public async Task Get_WithValidCourseIdAndNoAssignments_ShouldReturnNotFound()
        {
            // Arrange
            var courseId = Guid.NewGuid().ToString();
            _mockAssignmentRepository.Setup(repo => repo.FindBy(It.IsAny<Expression<Func<Assignment, bool>>>()))
                                     .ReturnsAsync(new List<Assignment>());

            _mockMapper.Setup(m => m.Map<IList<AssignmentDto>>(It.IsAny<List<Assignment>>()))
                       .Returns(new List<AssignmentDto>());

            // Act
            var result = await _assignmentService.GetAssignmentByCourseIdAsync(courseId);

            // Assert
            Assert.IsEmpty(result); // Check that the result is an empty list
        }

        [Test]
        public async Task GetAssignmentByCourseIdAsync_ShouldGenerateSasUrlsForAssignments()
        {
            // Arrange
            var courseId = Guid.NewGuid().ToString();
            var assignments = new List<Assignment>
            {
                new Assignment { FileName = "file1.txt" },
                new Assignment { FileName = "file2.txt" }  
            };

            _mockAssignmentRepository.Setup(repo => repo.FindBy(It.IsAny<Expression<Func<Assignment, bool>>>()))
                                     .ReturnsAsync(assignments);

            _mockMapper.Setup(m => m.Map<IList<AssignmentDto>>(assignments))
                       .Returns(assignments.Select(a => new AssignmentDto { FileName = a.FileName }).ToList());

            // Set up the mock AzureBlobService to return a SAS URL for each file name
            _mockAzureBlobService.Setup(service => service.GetServiceSasUriForBlob(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<int>()))
                                 .ReturnsAsync((string container, string fileName, string policy, int minutes) =>
                                     new Uri($"http://example.com/sasurl/{fileName}"));

            // Act
            var result = await _assignmentService.GetAssignmentByCourseIdAsync(courseId);

            // Assert
            Assert.IsNotEmpty(result);
            foreach (var assignmentDto in result)
            {
                Assert.That(assignmentDto.AssignmentFileSasUrl, Is.Not.Null.Or.Empty);
                Assert.That(assignmentDto.AssignmentFileSasUrl, Does.StartWith("http://example.com/sasurl/"));
                Assert.That(assignmentDto.AssignmentFileSasUrl, Does.Contain(assignmentDto.FileName));
            }
        }
        #endregion

        [Test]
        public async Task DeleteAssignmentAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var assignmentId = ObjectId.GenerateNewId();

            _mockAssignmentRepository.Setup(repo => repo.GetSingleAsync(assignmentId))
                                     .ReturnsAsync((Assignment)null); // Simulate no assignment found

            // Act
            var result = await _assignmentService.DeleteAssignmentAsync(assignmentId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteAssignmentAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var assignmentId = ObjectId.GenerateNewId();
           // var assignment = new Assignment {FileName = "existingFile.txt"}; // since file name is optional 
            var assignment = new Assignment();

            _mockAssignmentRepository.Setup(repo => repo.GetSingleAsync(assignmentId))
                                     .ReturnsAsync(assignment);
            _mockAssignmentRepository.Setup(repo => repo.DeleteAsync(assignmentId))
                                     .Returns(Task.CompletedTask);

            // Act
            var result = await _assignmentService.DeleteAssignmentAsync(assignmentId);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
