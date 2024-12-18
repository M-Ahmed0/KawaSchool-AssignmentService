using Application.BusinessLogic;
using Application.Interfaces.IBusinessLogic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class DependencyInjectionApplication
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IAssignmentService, AssignmentService>();
            services.AddTransient<IStudentUploadedAssignmentService, StudentUploadedAssignmentService>();

            return services;
        }
    }
}
