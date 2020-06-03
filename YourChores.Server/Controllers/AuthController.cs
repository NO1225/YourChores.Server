using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
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

            responseModel.Errors = (result.Errors.Select(error => error.Description)).ToList();

            return responseModel;

        }

        /// <summary>
        /// End point for loggin in using userName/Email with passward
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<APIResponse<LoginAPIModel.Response>>> Login(LoginAPIModel.Request requestModel)
        {
            // Defining empty response
            var responseModel = new APIResponse<LoginAPIModel.Response>();

            // Defining a user
            ApplicationUser user;

            // Check if the passed parameter is an email or a user name
            if (requestModel.UserNameOrEmail.Contains('@'))
            {
                // if email, search for the user using the email
                user = await _userManager.FindByEmailAsync(requestModel.UserNameOrEmail);
            }
            else
            {
                // if username, search for the user using userName
                user = await _userManager.FindByNameAsync(requestModel.UserNameOrEmail);
            }

            // If no user found
            if (user == null)
            {
                // Assigning the error and return
                responseModel.AddError("Passward Doesn't Match");
                return responseModel;
            }

            // if there is a user, attempt to sign in 
            var result = await _signInManager.PasswordSignInAsync(user, requestModel.Passward, false, false);

            // If the sign in was successfull
            if (result.Succeeded)
            {

                // Return the response with the generated token
                responseModel.Response = new LoginAPIModel.Response()
                {
                    Token = GenerateJSONWebToken(user)
                };

                return Ok(responseModel);
            }

            // If the attempt was a failure, return an error
            responseModel.AddError("Passward Doesn't Match");

            return responseModel;
        }

        /// <summary>
        /// End point to check the token and generate new one if valid, and let the user in 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("TokenLogin")]
        public async Task<ActionResult<APIResponse<LoginAPIModel.Response>>> TokenLogin()
        {
            // Defining empty response
            var responseModel = new APIResponse<LoginAPIModel.Response>();

            // Defining a user
            ApplicationUser user;

            user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Return the response with the generated token
            responseModel.Response = new LoginAPIModel.Response()
            {
                Token = GenerateJSONWebToken(user)
            };

            return Ok(responseModel);
        }


        /// <summary>
        /// End point to change or assign the first and last name for the user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("ChangeName")]
        public async Task<ActionResult<APIResponse>> ChangeName(ChangeNameAPIModel.Request requestModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            user.Firstname = requestModel.Firstname;
            user.Lastname = requestModel.Lastname;

            await _userManager.UpdateAsync(user);

            var responseModel = new APIResponse();

            return Ok(responseModel);
        }

        /// <summary>
        /// End point to change the passward of the current user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("ChangePassward")]
        public async Task<ActionResult<APIResponse>> ChangePassward(ChangePasswardAPIModel.Request requestModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var result = await _userManager.ChangePasswordAsync(user, requestModel.OldPassward, requestModel.NewPassward);

            var responseModel = new APIResponse();

            if(result.Succeeded)
            {
                return Ok(responseModel);
            }

            responseModel.Errors = result.Errors.Select(error => error.Description).ToList();

            return responseModel;
        }

        [HttpGet]
        [Authorize]
        [Route("GetMyInfo")]
        public async Task<ActionResult<APIResponse<UserAPIModel.Response>>> GetMyInfo ()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var responseModel = new APIResponse<UserAPIModel.Response>();

            responseModel.Response = new UserAPIModel.Response()
            {
                Id = user.Id,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                UserName = user.UserName,
                Email = user.Email
            };

            return Ok(responseModel);

        }

        #region Helper Methods

        /// <summary>
        /// A method to generate a web token for the user
        /// </summary>
        /// <param name="user">The user to generate the web token for</param>
        /// <returns></returns>
        private string GenerateJSONWebToken(ApplicationUser user)
        {
            // Getting the security key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Getting the encryption type
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Creating the token
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Audience"],
              new[] {
                    new Claim(ClaimTypes.Name, user.UserName)
                },
              expires: DateTime.UtcNow.AddMinutes(120),
              signingCredentials: credentials);

            // Returning the created token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}
