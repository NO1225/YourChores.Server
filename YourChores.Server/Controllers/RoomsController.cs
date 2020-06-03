using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.DataAccess;
using YourChores.Data.Enums;
using YourChores.Data.Models;
using YourChores.Server.APIModels;

namespace YourChores.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        /// <summary>
        /// The maximum number of allowed rooms for a user
        /// </summary>
        private const int MAX_USER_ROOMS = 20;

        /// <summary>
        /// The maximum number of user in one room
        /// </summary>
        private const int MAX_ROOM_USERS = 50;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// End point to allow the user to create a new room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<APIResponse<CreateRoomAPIModel.Response>>> CreateRoom(CreateRoomAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<CreateRoomAPIModel.Response>();

            // Check for the number of rooms the current is in
            if (_context.RoomUsers.Include(roomUser => roomUser.User).Where(roomUser => roomUser.User.Id == user.Id).Count() >= MAX_USER_ROOMS)
            {
                responseModel.AddError($"Maximum room number reached {MAX_USER_ROOMS}, leave a room to be able to create a new one");

                return responseModel;
            }


            // Check for the duplication in room name
            if (await _context.Rooms.FirstOrDefaultAsync(room => room.RoomName == requestModel.RoomName) != null)
            {
                responseModel.AddError("The name is already in use, please select another name");

                return responseModel;
            }

            // Create new room
            var room = new Room()
            {
                RoomName = requestModel.RoomName,
                AllowMembersToPost = requestModel.AllowMembersToPost
            };

            // Assign the user as the owner of the room
            var roomUser = new RoomUser()
            {
                Owener = true,
                Room = room,
                User = user
            };

            // Add the room the database and save changes
            await _context.RoomUsers.AddAsync(roomUser);
            await _context.SaveChangesAsync();

            // Fill the response
            responseModel.Response = new CreateRoomAPIModel.Response()
            {
                RoomName = requestModel.RoomName,
                AllowMembersToPost = requestModel.AllowMembersToPost
            };

            // Return the response
            return Ok(responseModel);

        }

        /// <summary>
        /// End point to get all the rooms that the current user joined
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<APIResponse<List<RoomAPIModel.Response>>>> GetRooms()
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<List<RoomAPIModel.Response>>();

            // Getting the room users
            responseModel.Response = await _context.RoomUsers
                // Include the user (join)
                .Include(roomUser => roomUser.User)
                // Include the room (join)
                .Include(roomUser => roomUser.Room)
                // Select all records related to the current user
                .Where(roomUser => roomUser.User.Id == user.Id)
                // Select just the required feilds and assign them to our response
                .Select(roomUser => new RoomAPIModel.Response()
                {
                    RoomName = roomUser.Room.RoomName,
                    AllowMembersToPost = roomUser.Room.AllowMembersToPost
                })
                // Run the query and get the data
                .ToListAsync();

            // return the rooms
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to get the details of a room by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getRoomById/{id}")]
        public async Task<ActionResult<APIResponse<RoomAPIModel.DetailedResponse>>> GetRoomById(int id)
        {
            // Initiate the response model
            var responseModel = new APIResponse<RoomAPIModel.DetailedResponse>();

            // Get all the rooms
            responseModel.Response = await _context.Rooms
                // Include the room users (join)
                .Include(room => room.RoomUsers)
                // Include the user of room user (join)
                .ThenInclude(roomUser => roomUser.User)
                // Select the required room
                .Where(room => room.Id == id)
                // Assign it to our response
                .Select(room =>
                // Our response
                    new RoomAPIModel.DetailedResponse()
                    {
                        RoomName = room.RoomName,
                        AllowMembersToPost = room.AllowMembersToPost,
                        // Getting the members and assigning them to our member response
                        RoomMembers = room.RoomUsers.Select(roomUser =>
                            new RoomAPIModel.RoomMember()
                            {
                                FirstName = roomUser.User.Firstname,
                                LastName = roomUser.User.Lastname
                            }
                        ).ToList()
                    }
                    // Return the room
                ).FirstOrDefaultAsync();

            // Returning the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to get the details of a room by Name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("getRoomByName/{name}")]
        public async Task<ActionResult<APIResponse<RoomAPIModel.DetailedResponse>>> GetRoomByName(string name)
        {
            // Initiate the response model
            var responseModel = new APIResponse<RoomAPIModel.DetailedResponse>();

            // Get all the rooms
            responseModel.Response = await _context.Rooms
                // Include the room users (join)
                .Include(room => room.RoomUsers)
                // Include the user of room user (join)
                .ThenInclude(roomUser => roomUser.User)
                // Select the required room
                .Where(room => room.RoomName == name)
                // Assign it to our response
                .Select(room =>
                // Our response
                    new RoomAPIModel.DetailedResponse()
                    {
                        RoomName = room.RoomName,
                        AllowMembersToPost = room.AllowMembersToPost,
                        // Getting the members and assigning them to our member response
                        RoomMembers = room.RoomUsers.Select(roomUser =>
                            new RoomAPIModel.RoomMember()
                            {
                                FirstName = roomUser.User.Firstname,
                                LastName = roomUser.User.Lastname
                            }
                        ).ToList()
                    }
                    // Return the room
                ).FirstOrDefaultAsync();

            // Returning the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point so that the user can send a join request to a room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Join")]
        public async Task<ActionResult<APIResponse>> JoinRoom(JoinRoomRequestAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Get the room
            var room = await _context.Rooms.FirstOrDefaultAsync(room => room.Id == requestModel.RoomId);

            // Initiate the response model
            var responseModel = new APIResponse();

            // Check if the room id is valid
            if(room==null)
            {
                responseModel.AddError("Room not found");

                return responseModel;
            }

            // Check if the user is already in this room
            if (_context.RoomUsers
                // Include the user (join)
                .Include(roomUser => roomUser.User)
                // Include the room (join)
                .Include(roomUser => roomUser.Room)
                // Check if the user has already joined this room
                .Any(roomUser => roomUser.Room.Id == requestModel.RoomId && roomUser.User.Id == user.Id))
            {
                responseModel.AddError("You already joined this room");

                return responseModel;
            }

            // Check if the user has already sent a request to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == user.Id && roomJoinRequest.JoinRequestType== JoinRequestType.Join))
            {
                responseModel.AddError("You already sent a request to this room");

                return responseModel;
            }

            // Check if the user has already receveid an invitation to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == user.Id && roomJoinRequest.JoinRequestType == JoinRequestType.Invite))
            {
                // TODO: redirect to the accept room request endpoint / accept

                return responseModel;
            }

            // Check for the number of rooms the current is in
            if (_context.RoomUsers.Include(roomUser => roomUser.User).Where(roomUser => roomUser.User.Id == user.Id).Count() >= MAX_USER_ROOMS)
            {
                responseModel.AddError($"Maximum room number reached {MAX_USER_ROOMS}, leave a room to be able to create a new one");

                return responseModel;
            }

            // Check for the number of member in the passed room
            if (_context.RoomUsers.Include(roomUser => roomUser.Room).Where(roomUser => roomUser.Room.Id == requestModel.RoomId).Count() >= MAX_ROOM_USERS)
            {
                responseModel.AddError($"Maximum room members reached {MAX_ROOM_USERS}, wait until a member leaves so that you can enter");

                return responseModel;
            }

            // Create the join request
            var roomJoinRequest = new RoomJoinRequest()
            {
                Room = room,
                User = user,
                JoinRequestType = JoinRequestType.Join
            };

            // Add the join request to the database and save changes
            await _context.RoomJoinRequests.AddAsync(roomJoinRequest);
            await _context.SaveChangesAsync();

            // Return success
            return Ok(responseModel);
        }


        /// <summary>
        /// End point so that the room room owner can invite users to the room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Invite")]
        public async Task<ActionResult<APIResponse>> InviteUser(InviteUserRequestAPIModel.Request requestModel)
        {

            // Initiate the response model
            var responseModel = new APIResponse();

            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var invitedUser = await _userManager.FindByIdAsync(requestModel.UserId);

            // Check if the room id is valid
            if (invitedUser == null)
            {
                responseModel.AddError("Invalid user id");

                return responseModel;
            }

            // Get the room
            var roomUser = await _context.RoomUsers
                .Include(roomUser=>roomUser.Room)
                .Include(roomUser => roomUser.User)
                .FirstOrDefaultAsync(roomUser => roomUser.Owener && roomUser.Room.Id == requestModel.RoomId && roomUser.User.Id == user.Id);


            // Check if the room id is valid
            if (roomUser == null)
            {
                responseModel.AddError("Invalid room id");

                return responseModel;
            }


            // Check if the user is already in this room
            if (_context.RoomUsers
                // Include the user (join)
                .Include(roomUser => roomUser.User)
                // Include the room (join)
                .Include(roomUser => roomUser.Room)
                // Check if the user has already joined this room
                .Any(roomUser => roomUser.Room.Id == requestModel.RoomId && roomUser.User.Id == requestModel.UserId))
            {
                responseModel.AddError("The user is already a member of this room");

                return responseModel;
            }

            // Check if the user has already sent a request to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == requestModel.UserId && roomJoinRequest.JoinRequestType == JoinRequestType.Join))
            {
                // TODO: redirect to the accept room request endpoint / accept


                return responseModel;
            }

            // Check if the user has already receveid an invitation to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == requestModel.UserId && roomJoinRequest.JoinRequestType == JoinRequestType.Invite))
            {
                responseModel.AddError("You already sent an invitation to this user");

                return responseModel;
            }

            // Check for the number of rooms the current is in
            if (_context.RoomUsers.Include(roomUser => roomUser.User).Where(roomUser => roomUser.User.Id == requestModel.UserId).Count() >= MAX_USER_ROOMS)
            {
                responseModel.AddError($"Maximum room number reached {MAX_USER_ROOMS} for the user!");

                return responseModel;
            }

            // Check for the number of member in the passed room
            if (_context.RoomUsers.Include(roomUser => roomUser.Room).Where(roomUser => roomUser.Room.Id == requestModel.RoomId).Count() >= MAX_ROOM_USERS)
            {
                responseModel.AddError($"Maximum room members reached {MAX_ROOM_USERS}, wait until a member leaves so that you can invite");

                return responseModel;
            }

            // Create the join request
            var roomJoinRequest = new RoomJoinRequest()
            {
                Room = roomUser.Room,
                User = invitedUser,
                JoinRequestType = JoinRequestType.Invite
            };

            // Add the join request to the database and save changes
            await _context.RoomJoinRequests.AddAsync(roomJoinRequest);
            await _context.SaveChangesAsync();

            // Return success
            return Ok(responseModel);
        }

    }
}
