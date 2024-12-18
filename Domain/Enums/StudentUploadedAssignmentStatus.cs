using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    [Flags]
    public enum StudentUploadedAssignmentStatus
    {
        New,
        Graded
    }
}
