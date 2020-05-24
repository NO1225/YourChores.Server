using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
        public async Task<ActionResult<APIResponse<RegisterAPIModel.Response>>> Register(RegisterAPIModel.Request requestModel)
        {
            var responseModel = new APIResponse<RegisterAPIModel.Response>();

            var user = new ApplicationUser()
            {
                UserName = requestModel.UserName,
                Email = requestModel.Email
            };

            var result = await _userManager.CreateAsync(user, requestModel.Passward);

            if (result.Succeeded)
            {

                responseModel.Response = new RegisterAPIModel.Response()
                {
                    Email = requestModel.Email,
                    UserName = requestModel.UserName
                };


                return Ok(responseModel);
            }

            responseModel.Errors.AddRange(result.Errors.Select(error => error.Description));

            return responseModel;

        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<APIResponse<LoginAPIModel.Response>>> Login(LoginAPIModel.Request requestModel)
        {
            var responseModel = new APIResponse<LoginAPIModel.Response>();

            ApplicationUser user;

            if (requestModel.UserNameOrEmail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(requestModel.UserNameOrEmail);
            }
            else
            {
                user = await _userManager.FindByNameAsync(requestModel.UserNameOrEmail);
            }

            if (user == null)
            {
                responseModel.Errors.Add("Passward Doesn't Match");
                return responseModel;
            }

            var result = await _signInManager.PasswordSignInAsync(user, requestModel.Passward, false, false);

            if (result.Succeeded)
            {
                responseModel.Response.Token = "dfasdfasdfasdfasd";
                return Ok(responseModel);
            }

            responseModel.Errors.Add("Passward Doesn't Match");

            return responseModel;
        }
    }
}
