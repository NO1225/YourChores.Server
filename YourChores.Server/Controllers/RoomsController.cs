using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourChores.Data.DataAccess;
using YourChores.Data.Models;
using YourChores.Server.APIModels;

namespace YourChores.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// End point to allow users to create rooms
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/")]
        public async Task<ActionResult<APIResponse<CreateRoomAPIModel.Response>>> CreateRoom(CreateRoomAPIModel.Request requestModel)
        {
            // Get the current logged user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiating the response model
            var responseModel = new APIResponse<CreateRoomAPIModel.Response>();

            // Checking for the dublication on room name
            if(await _context.Rooms.FirstOrDefaultAsync(room => room.RoomName == requestModel.RoomName) != null)
            {
                responseModel.AddError("The Room name is already in use, try another one");

                return responseModel;
            }

            // Create the new room
            var room = new Room()
            {
                RoomName = requestModel.RoomName,
                AllowMembersToPost = requestModel.AllowMembersToPost
            };

            // Assign the current user as the owner of this room
            var roomUser = new RoomUser()
            {
                Owener = true,
                Room = room,
                User = user
            };

            // Add the room to the database
            await _context.RoomUsers.AddAsync(roomUser);
            await _context.SaveChangesAsync();

            // Assigning the new room to the response
            responseModel.Response = new CreateRoomAPIModel.Response()
            {
                RoomName = requestModel.RoomName,
                AllowMembersToPost = requestModel.AllowMembersToPost
            };

            // Return the response
            return Ok(responseModel);
        }
    }
}
