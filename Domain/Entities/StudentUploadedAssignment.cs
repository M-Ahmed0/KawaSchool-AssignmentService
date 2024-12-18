using Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class StudentUploadedAssignment : BaseEntity
    {
        public string UserId { get; set; }
        public string AssignmentId { get; set; }
        public StudentUploadedAssignmentStatus Status { get; set; } = StudentUploadedAssignmentStatus.New;
        public string FileName { get; set; }
    }
}
