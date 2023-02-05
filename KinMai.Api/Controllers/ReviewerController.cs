using KinMai.Logic.UnitOfWork.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KinMai.Api.Controllers
{
    public class ReviewerController : ControllerBase
    {
        private readonly ILogicUnitOfWork _logicUnitOfWork;
        public ReviewerController(ILogicUnitOfWork logicUnitOfWork)
        {
            _logicUnitOfWork = logicUnitOfWork;
        }


    }
}
