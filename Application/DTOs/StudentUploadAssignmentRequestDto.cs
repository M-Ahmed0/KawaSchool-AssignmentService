using Application.DTOs.Mapping;
using AutoMapper;
using Domain.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class StudentUploadAssignmentRequestDto : IMapFrom<StudentUploadedAssignment>
    {
        public string UserId { get; set; }
        public string AssignmentId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StudentUploadedAssignment, StudentUploadAssignmentRequestDto>().ReverseMap();
        }
    }
}
