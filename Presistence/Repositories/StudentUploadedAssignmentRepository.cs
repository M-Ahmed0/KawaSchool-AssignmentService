using Application.Interfaces.IPresistence;
using Domain.Entities;
using Presistence.Repositories.BaseRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presistence.Repositories
{
    public class StudentUploadedAssignmentRepository : BaseRepository<StudentUploadedAssignment>, IStudentUploadedAssignmentRepository
    {
        public StudentUploadedAssignmentRepository(BaseDBContext dbContext) : base(dbContext)
        {
        }
    }
}
