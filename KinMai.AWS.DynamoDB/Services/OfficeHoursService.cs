using Amazon.DynamoDBv2.DataModel;
using KinMai.AWS.DynamoDB.Interfaces;
using KinMai.AWS.DynamoDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.AWS.DynamoDB.Services
{
    public class OfficeHoursService : IOfficeHoursService
    {
        private readonly IDynamoDBContext _dbContext;

        public OfficeHoursService(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OfficeHours>> GetAllAsync()
        {
            return await _dbContext.ScanAsync<OfficeHours>(default).GetRemainingAsync();
        }

        public async Task<OfficeHours> GetSingleAsync(string officeHoursId)
        {
            return await _dbContext.LoadAsync<OfficeHours>(officeHoursId);
        }

        public async Task AddAsync(OfficeHours officeHours)
        {
            await _dbContext.SaveAsync(officeHours);
        }

        public async Task RemoveAsync(string officeHoursId)
        {
            await _dbContext.DeleteAsync(officeHoursId);
        }

        public async Task UpdateAsync(OfficeHours officeHours)
        {
            await _dbContext.SaveAsync(officeHours);
        }
    }
}
