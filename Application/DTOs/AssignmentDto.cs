using Application.DTOs.Mapping;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AssignmentDto : BaseEntityDto, IMapFrom<Assignment>
    {
        
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string AssignmentDescription { get; set; }
        public DateTime Created { get; set; }
        public DateTime Deadline { get; set; }
        public string AssignmentFileSasUrl { get; set; }
        public string FileName { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Assignment, AssignmentDto>().ReverseMap();
        }
    }
}
