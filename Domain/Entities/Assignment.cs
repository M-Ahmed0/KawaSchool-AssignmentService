using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Assignment : BaseEntity
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string AssignmentDescription { get; set; }
        public DateTime Created { get; set; }
        public DateTime Deadline { get; set; }
        public string FileName { get; set; }

    }
}
