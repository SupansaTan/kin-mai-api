using KinMai.AWS.DynamoDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.AWS.DynamoDB.Interfaces
{
    public interface IOfficeHoursService
    {
        Task<List<OfficeHours>> GetAllAsync();
        Task<OfficeHours> GetSingleAsync(string officeHoursId);
        Task AddAsync(OfficeHours officeHours);
        Task RemoveAsync(string officeHoursId);
        Task UpdateAsync(OfficeHours officeHours);
    }
}
