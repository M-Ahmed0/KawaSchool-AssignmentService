using Application.Interfaces.IPresistence;
using Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presistence.Repositories;
using Presistence.Repositories.BaseRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presistence
{
    public static class DependencyInjectionPresistence
    {
        public static IServiceCollection AddPresistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<BaseDBContext>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddTransient<IAssignmentRepository, AssignmentRepository>();
            services.AddTransient<IStudentUploadedAssignmentRepository, StudentUploadedAssignmentRepository>();

            return services;
        }
    }
}
