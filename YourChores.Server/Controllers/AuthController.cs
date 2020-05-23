using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YourChores.Data.Models;
using YourChores.Server.APIModels;

namespace YourChores.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<RegisterAPIModel.Response>> Register(RegisterAPIModel.Request requestModel)
        {
            var user = new ApplicationUser()
            {
                UserName = requestModel.UserName,
                Email = requestModel.Email
            };

            var result = await _userManager.CreateAsync(user, requestModel.Passward);

            if (result.Succeeded)
            {
                var responseModel = new RegisterAPIModel.Response()
                {
                    Email = requestModel.Email,
                    UserName = requestModel.UserName
                };

                return Ok(responseModel);
            }
            else
            {
                var responseModel = new RegisterAPIModel.Response()
                {
                    Errors= result.Errors
                };

                return responseModel;
            }
        }
    }
}
