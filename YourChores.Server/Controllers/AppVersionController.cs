using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.DataAccess;
using YourChores.Data.Enums;
using YourChores.Data.Models;
using YourChores.Server.APIModels;

namespace YourChores.Server.Controllers
{
    /// <summary>
    /// Controller in charge of all the operation related to app version
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AppVersionController : ControllerBase
    {
        #region Read Only Feilds

       

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Defautl constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        public AppVersionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #endregion


        /// <summary>
        /// End point to allow the admin to create new version
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<APIResponse<AppVersionAPIModel.Response>>> CreateNewVersion(AppVersionAPIModel.Request requestModel)
        {
            // Initiate the response model
            var responseModel = new APIResponse<AppVersionAPIModel.Response>();

            var appVersion = await _context.AppVersions.FirstOrDefaultAsync(appVersion => appVersion.Version == requestModel.Version);

            if(appVersion!=null)
            {
                appVersion.Message = requestModel.Message;
                appVersion.LowestAllowedVersion = requestModel.LowestAllowedVersion;
                appVersion.DownloadURL = requestModel.DownloadURL;
                
            }
            else
            {
                var createdAppVersion = new AppVersion()
                {
                    Message = requestModel.Message,
                    LowestAllowedVersion = requestModel.LowestAllowedVersion,
                    DownloadURL = requestModel.DownloadURL,
                    Version = requestModel.Version
                };

                await _context.AppVersions.AddAsync(createdAppVersion);
            }

            await _context.SaveChangesAsync();

            responseModel.Response = new AppVersionAPIModel.Response()
            {
                LowestAllowedVersion = requestModel.LowestAllowedVersion,
                DownloadURL = requestModel.DownloadURL,
                Message = requestModel.Message,
                Version = requestModel.Version
            };

            // Return the response
            return Ok(responseModel);

        }

        /// <summary>
        /// End point to get the latest version
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<APIResponse<AppVersionAPIModel.Response>>> GetLatestVersion()
        {

            // Initiate the response model
            var responseModel = new APIResponse<AppVersionAPIModel.Response>();

            var appVersion = await _context.AppVersions.OrderByDescending(appVersion => appVersion.Version).FirstOrDefaultAsync();

            if(appVersion == null)
            {
                var createdAppVersion = new AppVersion()
                {
                    Message = "يوجد اصدار جديد",
                    LowestAllowedVersion = 0,
                    DownloadURL = "https://github.com/NO1225/YourChores.Client/releases",
                    Version = 1
                };

                await _context.AppVersions.AddAsync(createdAppVersion);

                await _context.SaveChangesAsync();

                responseModel.Response = new AppVersionAPIModel.Response()
                {
                    LowestAllowedVersion = createdAppVersion.LowestAllowedVersion,
                    DownloadURL = createdAppVersion.DownloadURL,
                    Message = createdAppVersion.Message,
                    Version = createdAppVersion.Version
                };
            }
            else
            {
                responseModel.Response = new AppVersionAPIModel.Response()
                {
                    LowestAllowedVersion = appVersion.LowestAllowedVersion,
                    DownloadURL = appVersion.DownloadURL,
                    Message = appVersion.Message,
                    Version = appVersion.Version
                };
            }

            // return the rooms
            return Ok(responseModel);
        }

    }
}
