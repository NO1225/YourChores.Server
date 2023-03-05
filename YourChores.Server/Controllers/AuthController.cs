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
    /// <summary>
    /// The controller in charge of all the operation of the authentication
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Private Feilds

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constrcutor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="signInManager"></param>
        /// <param name="userManager"></param>
        public AuthController(IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        #endregion

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<APIResponse<RegisterAPIModel.Response>>> Register(RegisterAPIModel.Request requestModel)
        {
            // Initiating the respoonse model
            var responseModel = new APIResponse<RegisterAPIModel.Response>();

            // Defining a new user
            var user = new ApplicationUser()
            {
                UserName = requestModel.UserName,
                Email = requestModel.Email
            };

            // Try to register the new user
            var result = await _userManager.CreateAsync(user, requestModel.Password);

            // If ther was an error
            if (!result.Succeeded)
            {
                // Adding the errors to the response
                responseModel.Errors = (result.Errors.Select(error => error.Description)).ToList();

                // Return the response
                return responseModel;

            }

            // if success, fill the response 
            responseModel.Response = new RegisterAPIModel.Response()
            {
                Email = requestModel.Email,
                UserName = requestModel.UserName
            };

            // Return the response
            return Ok(responseModel);
            

        }

        /// <summary>
        /// End point for loggin in using userName/Email with password
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
                responseModel.AddError("كلمة المرور لا تطابق الحساب");
                return responseModel;
            }

            // if there is a user, attempt to sign in 
            var result = await _signInManager.PasswordSignInAsync(user, requestModel.Password, false, false);

            // If the sign in was successfull
            if (result.Succeeded)
            {

                // Return the response with the generated token
                responseModel.Response = new LoginAPIModel.Response()
                {
                    Token = await GenerateJSONWebToken(user),
                    UserId = user.Id
                };

                return Ok(responseModel);
            }

            // If the attempt was a failure, return an error
            responseModel.AddError("كلمة المرور لا تطابق الحساب");

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
                Token = await GenerateJSONWebToken(user),
                UserId = user.Id

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
            // Get the user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            
            // Change the user first and last name
            if (!string.IsNullOrWhiteSpace(requestModel.Firstname))
            {
                user.Firstname = requestModel.Firstname;
            }
            
            // Change the user first and last name
            if (!string.IsNullOrWhiteSpace(requestModel.Lastname))
            {
                user.Lastname = requestModel.Lastname;
            }

            // Update
            await _userManager.UpdateAsync(user);

            var responseModel = new APIResponse();

            // return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to change the password of the current user
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("ChangePassword")]
        public async Task<ActionResult<APIResponse>> ChangePassword(ChangePasswordAPIModel.Request requestModel)
        {
            // Get the user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Try to change the password
            var result = await _userManager.ChangePasswordAsync(user, requestModel.OldPassword, requestModel.NewPassword);

            // Initiate the response
            var responseModel = new APIResponse();

            // if there is an error
            if(!result.Succeeded)
            {
                // Add the errors to the response
                responseModel.Errors = result.Errors.Select(error => error.Description).ToList();

                // Return the response
                return responseModel;
            }

            // Return the response
            return Ok(responseModel);
            
        }

        /// <summary>
        /// End point to get the info of the logged in user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("GetMyInfo")]
        public async Task<ActionResult<APIResponse<UserAPIModel.Response>>> GetMyInfo ()
        {
            // Get the logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<UserAPIModel.Response>();

            // Fil the response model
            responseModel.Response = new UserAPIModel.Response()
            {
                Id = user.Id,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                UserName = user.UserName,
                Email = user.Email
            };

            // return the response model
            return Ok(responseModel);

        }

        #region Helper Methods

        /// <summary>
        /// A method to generate a web token for the user
        /// </summary>
        /// <param name="user">The user to generate the web token for</param>
        /// <returns></returns>
        private async Task< string> GenerateJSONWebToken(ApplicationUser user)
        {
            // Getting the security key
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Getting the encryption type
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Getting the role of the user
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            // Creating the token
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Audience"],
              new[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, role??"user"),
                },
              expires: DateTime.UtcNow.AddMinutes(120),
              signingCredentials: credentials);

            // Returning the created token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}
