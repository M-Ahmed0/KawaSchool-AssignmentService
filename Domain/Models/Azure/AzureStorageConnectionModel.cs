using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Azure
{
    public class AzureStorageConnectionModel
    {
        public string ConnectionString { get; }

        public string Description { get; set; }

        public AzureStorageConnectionModel(string connectionString, string description)
        {
            this.ConnectionString = connectionString;
            this.Description = description;
        }
    }
}
