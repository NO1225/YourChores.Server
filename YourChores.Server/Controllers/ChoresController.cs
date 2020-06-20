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
    /// Controller in charge of all the operation related to chores
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChoresController : ControllerBase
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
        public ChoresController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #endregion


        /// <summary>
        /// End point to allow the user to create a new chore
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<APIResponse<CreateChoreAPIModel.Response>>> CreateChore(CreateChoreAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<CreateChoreAPIModel.Response>();

            var room = await _context.Rooms
                .Include(room => room.RoomUsers)
                .ThenInclude(roomUser => roomUser.User)
                .FirstOrDefaultAsync(room =>
                room.Id == requestModel.RoomId
                && room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id)
                && (room.AllowMembersToPost || room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner)
                );

            if(room == null)
            {
                responseModel.AddError("You don't have premession to post here ");

                return responseModel;
            }

            var chore = new ToDoItem()
            {
                Description = requestModel.Description,
                Room = room,
                Urgency = requestModel.Urgency
            };

            await _context.ToDoItems.AddAsync(chore);

            await _context.SaveChangesAsync();

            responseModel.Response = new CreateChoreAPIModel.Response()
            {
                ChoreId = chore.Id,
                Description = chore.Description,
                Urgency = chore.Urgency
            };

            // Return the response
            return Ok(responseModel);

        }

        /// <summary>
        /// End point to get all the chores for this user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<APIResponse<List<ChoreAPIModel.Response>>>> GetChores()
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<List<ChoreAPIModel.Response>>();

            // Getting the room users
            responseModel.Response = await _context.ToDoItems
                // Join on room
                .Include(toDoItem => toDoItem.Room)
                // Join on room users
                .ThenInclude(room => room.RoomUsers)
                // Join on users
                .ThenInclude(roomUser => roomUser.User)
                // Select the rooms this user is member of
                .Where(toDoItem => toDoItem.Room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id))
                // Convert the rooms to the response format
                .Select(toDoItem =>
                new ChoreAPIModel.Response()
                    {
                        ChoreId = toDoItem.Id,
                        CreatedOn = toDoItem.CreatedOn,
                        Description = toDoItem.Description,
                        Urgency = toDoItem.Urgency,
                        Done = toDoItem.Done,
                        RoomId = toDoItem.Room.Id,
                        RoomName = toDoItem.Room.RoomName
                    }
                ).ToListAsync();

            // return the rooms
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to update the chore
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Update")]
        public async Task<ActionResult<APIResponse>> UpdateChore(UpdateChoreAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse<UpdateRoomAPIModel.Response>();

            var room = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id));

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("Invlid room Id");

                // Return the response
                return responseModel;
            }
            // Get the chore we are trying to update
            var chore = await _context.ToDoItems
                .Include(toDoItem => toDoItem.Room)
                .FirstOrDefaultAsync(toDoItem => toDoItem.Id == requestModel.ChoreId && toDoItem.Room.Id == requestModel.RoomId);
            
            // Checck if the user is a member of this room and the room exist
            if (chore == null)
            {
                responseModel.AddError("Invlid chore Id");

                // Return the response
                return responseModel;
            }
            // Update the chore
            chore.Done = true;
            chore.DoingTime = DateTime.UtcNow;
            chore.Doer = user;

            // Save the changes
            await _context.SaveChangesAsync();

            return Ok(responseModel);
        }

    }
}
