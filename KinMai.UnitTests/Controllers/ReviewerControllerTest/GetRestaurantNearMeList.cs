using KinMai.Api.Controllers;
using KinMai.Api.Models;
using KinMai.Api.Models.Reviewer;
using KinMai.Authentication.Model;
using KinMai.Authentication.UnitOfWork;
using KinMai.Dapper.Interface;
using KinMai.EntityFramework.Models;
using KinMai.EntityFramework.UnitOfWork.Implement;
using KinMai.EntityFramework.UnitOfWork.Interface;
using KinMai.Logic.Models;
using KinMai.Logic.UnitOfWork.Implement;
using KinMai.Logic.UnitOfWork.Interface;
using KinMai.Mail.UnitOfWork;
using KinMai.S3.UnitOfWork.Interface;
using KinMai.UnitTests.Shared;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using Xunit.Abstractions;

namespace KinMai.UnitTests.Controllers.ReviewerControllerTest
{
	public class GetRestaurantNearMeList
	{
        private readonly ITestOutputHelper output;
        private readonly InitConfiguration initConfiguration;
        public GetRestaurantNearMeList(ITestOutputHelper output)
		{
			this.output = output;
			var initConfiguration = new InitConfiguration();
		}

		[Fact]
		public async Task GetRestaurantNearMeList_RetuenStatus200_WhenRestaurantShowUp()
		{
            // mock unit of work
            var mockDapperUnitOfWork = new Mock<IDapperUnitOfWork>();
            var mockS3UnitOfWork = new Mock<IS3UnitOfWork>();
            var mockMailUnitOfWork = new Mock<IMailUnitOfWork>();
            IAuthenticationUnitOfWork mockAuthenticationUnitOfWork = new AuthenticationUnitOfWork();

            //arrange
            var mockRequest = new GetRestaurantNearMeRequestModel()
            {
                Id = userId,
                Latitude = latitude,
                Longtitude = longitude,
                Skip = skip,
                Take = take

            };

            using (var context = new KinMaiContext(NewDbContextService.CreateNewContextOptions()))
            {
                // init controller
                IEntityUnitOfWork mockEntityUnitOfWork = new EntityUnitOfWork(context);
                ILogicUnitOfWork logicUnitOfWork = new LogicUnitOfWork(
                    mockEntityUnitOfWork,
                    mockDapperUnitOfWork.Object,
                    mockAuthenticationUnitOfWork,
                    mockS3UnitOfWork.Object,
                    mockMailUnitOfWork.Object
                );
                var reviewerController = new ReviewerController(logicUnitOfWork);

                // act
                var actualOutput = await reviewerController.GetRestaurantNearMeList(mockRequest);
                var expectOutput = new ResponseModel<RestaurantInfoListModel>
                {
                    Data = true,
                    Message = "success",
                    Status = 200
                };

                // assert
                Assert.Equal(actualOutput.Data, expectOutput.Data);
                Assert.Equal(actualOutput.Message, expectOutput.Message);
                Assert.Equal(actualOutput.Status, expectOutput.Status);
            }


        }

    }
}

