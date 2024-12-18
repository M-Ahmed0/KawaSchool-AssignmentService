using Application.DTOs.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateUploadedAssignmentDto : BaseEntityDto, IMapFrom<StudentUploadedAssignment>
    {
        public string UserId { get; set; }
        public string AssignmentId { get; set; } // this assignment id was generated when teacher created the assignment
        public string Status { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<StudentUploadedAssignment, StudentUploadedAssignmentDto>().ReverseMap();
        }
    }
}
