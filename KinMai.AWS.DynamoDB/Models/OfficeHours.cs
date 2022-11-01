using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.AWS.DynamoDB.Models
{
    [DynamoDBTable("OfficeHours")]
    public class OfficeHours
    {
        [DynamoDBHashKey("OfficeHoursId")]
        public string OfficeHoursId { get; set; }
    }
}
