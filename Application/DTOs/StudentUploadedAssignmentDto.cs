using Application.DTOs.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class StudentUploadedAssignmentDto : BaseEntityDto, IMapFrom<StudentUploadedAssignment>
    {
        public string UserId { get; set; }
        public string AssignmentId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StudentUploadedAssignmentStatus Status { get; set; }
        public string UploadedAssignmentSasUrl { get; set; }
        public string FileName { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<StudentUploadedAssignment, StudentUploadedAssignmentDto>().ReverseMap();
        }
    }
}
