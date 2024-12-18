using Application.BusinessLogic.ValidationHelper;
using Application.DTOs;
using Application.Interfaces.IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;

namespace KawaAssignment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly ILogger<AssignmentController> _logger;
        private readonly IAssignmentService _assignmentService;
        private readonly IStudentUploadedAssignmentService _uploadedAssignmentService;


        public AssignmentController(ILogger<AssignmentController> logger,
            IAssignmentService assignmentService,
            IStudentUploadedAssignmentService uploadedAssignmentService)
        {
            _logger = logger;
            _assignmentService = assignmentService;
            _uploadedAssignmentService = uploadedAssignmentService;

        }
        #region  POST
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Teacher")]
        [HttpPost(Name = "assignments")]
        public async Task<ActionResult> CreateAssignment([FromForm] AssignmentRequestDto assignmentInfo, IFormFile? file)
        {
            try
            {
                // Logic to handle the creation of a new assignment using assignmentInfo and file
                // Example implementation:
                if (assignmentInfo != null)
                {
                    // Process the assignmentInfo and file data (e.g., save to the database)
                    await _assignmentService.CreateAssignmentAsync(assignmentInfo, file);
                    // Return a response indicating success
                    return StatusCode(201, "Assignment created");
                }
                else
                {
                    // Handle invalid input or missing data
                    return BadRequest("Invalid input or missing data");
                }
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while processing the request.");
            }
            
        }
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Student")]
        [HttpPost("upload", Name = "UploadAssignment")]
        public async Task<ActionResult> UploadAssignment([FromForm] StudentUploadAssignmentRequestDto assignmentInfo, IFormFile file)
        {
            try
            {
                if (assignmentInfo is not null && file is not null)
                {
                    // Process the assignmentInfo and file data (e.g., save to the database)
                    await _uploadedAssignmentService.UploadAssignmentAsync(assignmentInfo, file);
                    // Return a response indicating success
                    return StatusCode(201, "Assignment Uploaded");
                }
                else
                {
                    // Handle invalid input or missing data
                    return BadRequest("Invalid input or missing data");
                }
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while processing the request.");
            }

        }
        #endregion


        #region  GET
        /// <summary>
        /// GetAssignmentByCourseId
        /// </summary>
        /// <param name="courseId"> for. Example: 7bc11c32-0dc9-4cd8-b4ad-f99c35d95093</param>
        /// <returns>A list of assignments for the given course.</returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Student")]
        [HttpGet("{courseId}", Name = "GetAssignmentsByCourse")]
        public async Task<ActionResult<IEnumerable<AssignmentDto>>> Get(string courseId)
        {
            try
            {
                if (courseId is not null)
                {
                    var assignments = await _assignmentService.GetAssignmentByCourseIdAsync(courseId);

                    if (assignments.Any())
                    {
                        return Ok(assignments);
                    }
                    else
                    {
                        return NotFound("No assignments found");
                    }                    
                }
                return BadRequest("Invalid course Id");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }

        }

        /// <summary>
        /// GetUploadedAssignment
        /// </summary>
        /// <param name="assignmentId"> for. Example: 659c63052f2cf7fe2f8a00bf </param>
        /// <returns>A list of assignments for the given id.</returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Student, Teacher")]
        [HttpGet("uploaded/{assignmentId}", Name = "GetUploadedAssignmentById")]
        public async Task<ActionResult<IEnumerable<StudentUploadedAssignmentDto>>> GetUploadedAssignment(string assignmentId)
        {
           
            try
            {
                // Check if the user has the "Student" role
                bool isStudent = User.IsInRole("Student");
                
                // Check if the user has the "Teacher" role
                bool isTeacher = User.IsInRole("Teacher");

                string userId = null;
                if (isStudent)
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim is not null)
                    {
                        userId = userIdClaim.Value;
                    }
                }
                var isValid = InputValidationHelper.ValidateAssignmentIdAndUserIdInput(assignmentId, userId);
                if (!isValid) return BadRequest("Invalid assignmentId Id");
                var assignments = await _uploadedAssignmentService.GetUploadedAssignmentByAssignmentIdAsync(assignmentId, userId);

                if (assignments.Any())
                {
                    return Ok(assignments);
                }
                else
                {
                    return NotFound("No assignments found");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        #endregion 

        #region  PUT
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Student")]
        [HttpPut("uploaded", Name = "UpdateUploadedAssignment")]
        public async Task<ActionResult> UpdateUploadedAssignment(UpdateUploadedAssignmentDto uploadedAssignment)
        {
            try
            {
                if (uploadedAssignment is not null)
                {
                    await _uploadedAssignmentService.UpdateUploadedAssignmentByIdAsync(uploadedAssignment);

                    return Ok("Updated");
                }
                return BadRequest("Invalid Id");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        #endregion

        #region  DELETE
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Student")]
        [HttpDelete("uploaded/{uploadedAssignmentId}", Name = "DeleteUploadedAssignment")]
        public async Task<ActionResult> DeleteUploadedAssignment(string uploadedAssignmentId)
        {
            try
            {
                try
                {
                    ObjectId objectId = ObjectId.Parse(uploadedAssignmentId);
                    var result = await _uploadedAssignmentService.DeleteUploadedAssignmentAsync(objectId);

                    if (result)
                    {
                        return Ok("Operation completed successfully.");
                    }
                    else
                    {
                        return NotFound("Operation could not be completed to an error.");
                    }
                }
                catch (Exception)
                {

                    return BadRequest("uploadedAssignmentId is not valid.");
                }
            }
            catch (Exception)
            {
                // Consider logging the exception
                return StatusCode(500, "An error occurred while deleting the assignment.");
            }
        }

        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Teacher")]
        [HttpDelete("{assignmentId}", Name = "DeleteAssignmentById")]
        public async Task<ActionResult> DeleteAssignment(string assignmentId)
        {
            try
            {
                try
                {
                   ObjectId objectId = ObjectId.Parse(assignmentId);
                    var result = await _assignmentService.DeleteAssignmentAsync(objectId);

                    if (result)
                    {
                        return Ok("Operation completed successfully.");
                    }
                    else
                    {
                        return NotFound("Operation could not be completed to an error.");
                    }
                }
                catch (Exception)
                {

                    return BadRequest("uploadedAssignmentId is not valid.");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the assignment.");
            }
        }
        #endregion 



    }
}
