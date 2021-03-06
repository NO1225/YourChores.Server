﻿using Microsoft.AspNetCore.Authorization;
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
    /// <summary>
    /// Controller in charge of all the operation related to rooms
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        #region Read Only Feilds

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

        #endregion

        #region Constructor

        /// <summary>
        /// Defautl constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        public RoomsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #endregion


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
                responseModel.AddError($"تم الوصول ل اقصى عدد من الغرف للمستخدم {MAX_USER_ROOMS}, يرجى مغادرة غرفة لاتاحة المجال لعمل غرفة جديدة");

                return responseModel;
            }

            var normalizedRoomName = requestModel.RoomName.ToLower();
            // Check for the duplication in room name
            if (await _context.Rooms.FirstOrDefaultAsync(room => room.NormalizedRoomName == normalizedRoomName) != null)
            {
                responseModel.AddError("اسم الغرفة مستعمل مسبقا، يرجى المحالة باسم اخر");

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
                Owner = true,
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
                // Include the chores of this room
                .ThenInclude(room => room.ToDoItems)
                // Select all records related to the current user
                .Where(roomUser => roomUser.User.Id == user.Id)
                // Select just the required feilds and assign them to our response
                .Select(roomUser => new RoomAPIModel.Response()
                {
                    // Assign the id
                    RoomId = roomUser.Room.Id,
                    // Assign the room name
                    RoomName = roomUser.Room.RoomName,
                    // Get if the members are allowed to post 
                    AllowMembersToPost = roomUser.Room.AllowMembersToPost,
                    // Get the number of pending chores
                    NumberOfPendingChores = roomUser.Room.ToDoItems.Where(toDoItem => toDoItem.Done == false).Count(),
                    // If there is a pending chores, get the highest urgency from them
                    HighestUrgency = roomUser.Room.ToDoItems.Where(toDoItem => toDoItem.Done == false).Count() == 0 ? Urgency.Low : (Urgency)roomUser.Room.ToDoItems.Max(toDoItem => (int)toDoItem.Urgency)
                })
                // Run the query and get the data
                .ToListAsync();

            // return the rooms
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to get the details of a room by Id
        /// </summary>
        /// <param name="id">The id of the room</param>
        /// <returns></returns>
        [HttpGet("getRoomById/{id}")]
        public async Task<ActionResult<APIResponse<RoomAPIModel.DetailedResponse>>> GetRoomById(int id)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<RoomAPIModel.DetailedResponse>();

            var isOwner = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Select the required room and make sure that this user is a member of it
               .AnyAsync(room => room.Id == id &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            if(isOwner)
            {
                // Get all the rooms
                responseModel.Response = await _context.Rooms
                    // Include the room chores (join)
                    .Include(room => room.ToDoItems)
                    // Include the room users (join)
                    .Include(room => room.RoomUsers)
                    // Include the user of room user (join)
                    .ThenInclude(roomUser => roomUser.User)
                    // Include the join requests (join)
                    .Include(room => room.RoomJoinRequests)
                    // Include the user of join requests (join)
                    .ThenInclude(roomJoinRequests => roomJoinRequests.User)
                    // Select the required room and make sure that this user is a member of it
                    .Where(room => room.Id == id && room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id))
                    // Assign it to our response
                    .Select(room =>
                        // Our response
                        new RoomAPIModel.DetailedResponse()
                        {
                        // Assign the id
                        RoomId = room.Id,
                            RoomName = room.RoomName,
                            AllowMembersToPost = room.AllowMembersToPost,
                        // If the current user is the owner of this room
                        IsOwner = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner,
                        // Getting the members and assigning them to our member response
                        RoomMembers = room.RoomUsers.Select(roomUser =>
                                new RoomAPIModel.RoomMember()
                                {
                                    UserId = roomUser.User.Id,
                                    FirstName = roomUser.User.Firstname,
                                    LastName = roomUser.User.Lastname,
                                    IsOwner = roomUser.Owner,
                                }
                            ).ToList(),
                        // Getting the join request for this room
                        JoinRequests = room.RoomJoinRequests.Select(roomJoinRequest =>
                                new RoomAPIModel.JoinRequest()
                                {
                                    JoinRequestId = roomJoinRequest.Id,
                                    UserId = roomJoinRequest.User.Id,
                                    FirstName = roomJoinRequest.User.Firstname,
                                    LastName = roomJoinRequest.User.Lastname,
                                    JoinRequestType = roomJoinRequest.JoinRequestType
                                }
                            ).ToList(),
                        // Gettig the chores in this room
                        Chores = room.ToDoItems.Select(toDoItem => new RoomAPIModel.Chore()
                            {
                                ChoreId = toDoItem.Id,
                                Description = toDoItem.Description,
                                Done = toDoItem.Done,
                                Urgency = toDoItem.Urgency
                            }).ToList()
                        }
                    // Return the room
                    ).FirstOrDefaultAsync();
            }
            else
            {
                // Get all the rooms
                responseModel.Response = await _context.Rooms
                    // Include the room chores (join)
                    .Include(room => room.ToDoItems)
                    // Include the room users (join)
                    .Include(room => room.RoomUsers)
                    // Include the user of room user (join)
                    .ThenInclude(roomUser => roomUser.User)
                    // Select the required room and make sure that this user is a member of it
                    .Where(room => room.Id == id && room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id))
                    // Assign it to our response
                    .Select(room =>
                        // Our response
                        new RoomAPIModel.DetailedResponse()
                        {
                        // Assign the id
                        RoomId = room.Id,
                            RoomName = room.RoomName,
                            AllowMembersToPost = room.AllowMembersToPost,
                        // If the current user is the owner of this room
                        IsOwner = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner,
                        // Getting the members and assigning them to our member response
                        RoomMembers = room.RoomUsers.Select(roomUser =>
                                new RoomAPIModel.RoomMember()
                                {
                                    UserId = roomUser.User.Id,
                                    FirstName = roomUser.User.Firstname,
                                    LastName = roomUser.User.Lastname,
                                    IsOwner = roomUser.Owner,
                                }
                            ).ToList(),
                        // Gettig the chores in this room
                        Chores = room.ToDoItems.Select(toDoItem => new RoomAPIModel.Chore()
                            {
                                ChoreId = toDoItem.Id,
                                Description = toDoItem.Description,
                                Done = toDoItem.Done,
                                Urgency = toDoItem.Urgency
                            }).ToList()
                        }
                    // Return the room
                    ).FirstOrDefaultAsync();
            }
            

            // Returning the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to get the details of a room by Name
        /// </summary>
        /// <param name="name">The name of the room</param>
        /// <returns></returns>
        [HttpGet("getRoomsByName/{name}")]
        public async Task<ActionResult<APIResponse<List<RoomAPIModel.SearchRoomResponse>>>> GetRoomsByName(string name)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Initiate the response model
            var responseModel = new APIResponse<List<RoomAPIModel.SearchRoomResponse>>();

            var normalizedName = name.ToLower();

            // Get all the rooms
            responseModel.Response = await _context.Rooms
                // Include the join requests
                .Include(room => room.RoomJoinRequests)
                // Include the user of the join request (join)
                .ThenInclude(roomJoinRequest => roomJoinRequest.User)
                // Include the room users (join)
                .Include(room => room.RoomUsers)
                // Include the user of room user (join)
                .ThenInclude(roomUser => roomUser.User)
                // Select the required room
                .Where(room => room.NormalizedRoomName.Contains(normalizedName))
                // Assign it to our response
                .Select(room =>
                // Our response
                    new RoomAPIModel.SearchRoomResponse()
                    {
                        // Assign the id
                        RoomId = room.Id,
                        RoomName = room.RoomName,
                        MaxAllowedMembers = MAX_ROOM_USERS,
                        NumberOfMembers = room.RoomUsers.Count,
                        IsMember = room.RoomUsers.Any(roomUser => roomUser.User.Id == user.Id),
                        JoinRequestSent = room.RoomJoinRequests.Any(roomJoinRequest => roomJoinRequest.User.Id == user.Id && roomJoinRequest.JoinRequestType == JoinRequestType.Join),
                        IsInvited = room.RoomJoinRequests.Any(roomJoinRequest => roomJoinRequest.User.Id == user.Id && roomJoinRequest.JoinRequestType == JoinRequestType.Invite),
                    }
                    // Return the room
                ).ToListAsync();

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
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

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
                responseModel.AddError("انت بالفعل عضو في هذه الغرفة");

                return responseModel;
            }

            // Check if the user has already sent a request to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == user.Id && roomJoinRequest.JoinRequestType == JoinRequestType.Join))
            {
                responseModel.AddError("لقد قمت مسبقا بارسال طلب لهذه الغرفة");

                return responseModel;
            }

            // Check if the user has already receveid an invitation to this room
            if (_context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .Any(roomJoinRequest => 
                    roomJoinRequest.Room.Id == requestModel.RoomId 
                    && roomJoinRequest.User.Id == user.Id 
                    && roomJoinRequest.JoinRequestType == JoinRequestType.Invite))
            {
                // TODO: redirect to the accept room request endpoint / accept
                // TODO: Test this if it works
                var requestId = (await _context.RoomJoinRequests
               // Include the user (join)
               .Include(roomJoinRequest => roomJoinRequest.User)
               // Include the room (join)
               .Include(roomJoinRequest => roomJoinRequest.Room)
               // Check if the user has a;ready sent a join request to this room
               .FirstOrDefaultAsync(roomJoinRequest => 
                   roomJoinRequest.Room.Id == requestModel.RoomId 
                   && roomJoinRequest.User.Id == user.Id
                   && roomJoinRequest.JoinRequestType == JoinRequestType.Invite)).Id;


                var passingModel = new AcceptRequestAPIModel.Request()
                {
                    JoinRequestId = requestId,
                    RoomId = requestModel.RoomId
                };

                // Redirecting to the accept method
                return RedirectToAction(nameof(AcceptInvitation), passingModel);
            }

            // Check for the number of rooms the current is in
            if (_context.RoomUsers.Include(roomUser => roomUser.User).Where(roomUser => roomUser.User.Id == user.Id).Count() >= MAX_USER_ROOMS)
            {
                responseModel.AddError($"تم الوصول ل اقصى عدد من الغرف للمستخدم {MAX_USER_ROOMS}, يرجى مغادرة غرفة لاتاحة المجال لعمل غرفة جديدة");

                return responseModel;
            }

            // Check for the number of member in the passed room
            if (_context.RoomUsers.Include(roomUser => roomUser.Room).Where(roomUser => roomUser.Room.Id == requestModel.RoomId).Count() >= MAX_ROOM_USERS)
            {
                responseModel.AddError($"تم الوصول ل اقصى عدد من الاعضاء بالغرفة {MAX_ROOM_USERS}, انتظر حتى خروج عضو لاتاحة المجال لك");

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
                responseModel.AddError("رقم المستخدم غير صحيح");

                return responseModel;
            }

            // Get the room
            var roomUser = await _context.RoomUsers
                .Include(roomUser => roomUser.Room)
                .Include(roomUser => roomUser.User)
                .FirstOrDefaultAsync(roomUser => roomUser.Owner && roomUser.Room.Id == requestModel.RoomId && roomUser.User.Id == user.Id);


            // Check if the room id is valid
            if (roomUser == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

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
                responseModel.AddError("المستخدم عضو في هذه الغرفة");

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
                var requestId = (await _context.RoomJoinRequests
                // Include the user (join)
                .Include(roomJoinRequest => roomJoinRequest.User)
                // Include the room (join)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                // Check if the user has a;ready sent a join request to this room
                .FirstOrDefaultAsync(roomJoinRequest => roomJoinRequest.Room.Id == requestModel.RoomId && roomJoinRequest.User.Id == requestModel.UserId && roomJoinRequest.JoinRequestType == JoinRequestType.Join)).Id;


                // TODO: Test this if it works

                var passingModel = new AcceptRequestAPIModel.Request()
                {
                    JoinRequestId = requestId,
                    RoomId = requestModel.RoomId
                };

                // Redirecting to the accept method
                return RedirectToAction(nameof(AcceptRequest), passingModel);
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
                responseModel.AddError("لقد قمت بارسال دعوة مسبقا لهذا المستخدم");

                return responseModel;
            }

            // Check for the number of rooms the current is in
            if (_context.RoomUsers.Include(roomUser => roomUser.User).Where(roomUser => roomUser.User.Id == requestModel.UserId).Count() >= MAX_USER_ROOMS)
            {
                responseModel.AddError($"تم الوصول لاقصى عدد من الغرف {MAX_USER_ROOMS} لهذا المستخدم!");

                return responseModel;
            }

            // Check for the number of member in the passed room
            if (_context.RoomUsers.Include(roomUser => roomUser.Room).Where(roomUser => roomUser.Room.Id == requestModel.RoomId).Count() >= MAX_ROOM_USERS)
            {
                responseModel.AddError($"تم الوصول ل اقصى عدد من المستخدمين لهذه الغرفة {MAX_ROOM_USERS}, انتظر حتى خروج اي عضو لاتاحة المجال");

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

        /// <summary>
        /// End point to allow the members to leave a room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Leave")]
        public async Task<ActionResult<APIResponse>> LeaveRoom(LeaveRoomAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            var room = await _context.Rooms
                // Include the room users (join)
                .Include(room => room.RoomUsers)
                // Include the user of room user (join)
                .ThenInclude(roomUser => roomUser.User)
                // Select the required room and make sure that this user is a member of it
                .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId && room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id));

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                return responseModel;
            }

            // get the record of the current user
            var roomUser = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id);

            // Check if this user is the last owner of this room
            if (room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner
                && room.RoomUsers.Where(roomUser => roomUser.Owner).Count() < 2
                && room.RoomUsers.Count() > 1)
            {

                var alternativeRoomUser = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == requestModel.AlternativeId);

                if (alternativeRoomUser == null)
                {
                    responseModel.AddError("انت اخر مالك لهذه الغرفة، يجب تحديد عضو بديل لتتمكن من المغادرة");

                    return responseModel;
                }

                // check the alternative id, assign him as owner
                alternativeRoomUser.Owner = true;

                // reomve it from the database and save chagnes
                _context.RoomUsers.Remove(roomUser);
                await _context.SaveChangesAsync();

                // Return the response
                return Ok(responseModel);
            }

            // If this user is the last user in the room, delete it
            if (room.RoomUsers.Count() < 2)
            {
                // reomve it from the database and save chagnes
                _context.RoomUsers.Remove(roomUser);
                _context.Rooms.Remove(room);

                await _context.SaveChangesAsync();

                // Return the response
                return Ok(responseModel);
            }

            // reomve it from the database and save chagnes
            _context.RoomUsers.Remove(roomUser);

            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to allow the owner of a room to kick other users
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Kick")]
        public async Task<ActionResult<APIResponse>> KickUser(KickUserAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            if (user.Id == requestModel.UserId)
            {
                responseModel.AddError("لا يمكنك طرد نفسك");

                // Return the response
                return responseModel;
            }

            var room = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var roomUser = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == requestModel.UserId);

            if (roomUser == null)
            {
                responseModel.AddError("هذا المستخدم ليس عضوا في هذه الغرفة");

                // Return the response
                return responseModel;
            }

            // Remove the record of the user to this room & save changes
            _context.RoomUsers.Remove(roomUser);
            await _context.SaveChangesAsync();

            // Return the response
            return responseModel;

        }

        /// <summary>
        /// End point to update the settings of the room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Update")]
        public async Task<ActionResult<APIResponse<UpdateRoomAPIModel.Response>>> UpdateRoom(UpdateRoomAPIModel.Request requestModel)
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
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            room.AllowMembersToPost = requestModel.AllowMembersToPost;
            await _context.SaveChangesAsync();

            responseModel.Response = new UpdateRoomAPIModel.Response()
            {
                RoomName = room.RoomName,
                AllowMembersToPost = room.AllowMembersToPost,
            };

            return Ok(responseModel);
        }

        /// <summary>
        /// End point to allow the owners of the room to promote other members
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Promote")]
        public async Task<ActionResult<APIResponse>> PromoteMember(PromoteUserAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            if (user.Id == requestModel.UserId)
            {
                responseModel.AddError("لا تستطيع ترفيع نفسك");

                // Return the response
                return responseModel;
            }

            var room = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var roomUser = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == requestModel.UserId && !roomUser.Owner);

            if (roomUser == null)
            {
                responseModel.AddError("هذا المستخدم ليس عضوا في هذه الغرفة");

                // Return the response
                return responseModel;
            }

            // Promote the user
            roomUser.Owner = true;

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to allow the owners of the room to demote other owners
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("Demote")]
        public async Task<ActionResult<APIResponse>> DemoteOwner(DemoteOwnerAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            var room = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var roomUser = room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == requestModel.UserId && roomUser.Owner);

            if (roomUser == null)
            {
                responseModel.AddError("هذا العضو ليس مالكا للغرفة");

                // Return the response
                return responseModel;
            }

            // Check if the user we are going to demote is the last owner of this room (by default, it's the logged in user)
            if(roomUser.User.Id == user.Id && room.RoomUsers.Where(roomUser=>roomUser.Owner).Count()==1)
            {
                responseModel.AddError("انت اخر مالك بهذه الغرفة، لا يمكنك تهبيط نفسك");

                // Return the response
                return responseModel;
            }

            // Promote the user
            roomUser.Owner = false;

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to search for a members for this room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("FindMember")]
        public async Task<ActionResult<APIResponse<List<SearchMemberAPIModel.Response>>>> GetMemberByUserName(SearchMemberAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse<List<SearchMemberAPIModel.Response>>();

            var room = await _context.Rooms
               // Include the room users (join)
               .Include(room => room.RoomUsers)
               // Include the user of room user (join)
               .ThenInclude(roomUser => roomUser.User)
               // Include the room users (join)
               .Include(room => room.RoomJoinRequests)
               // Include the user of room user (join)
               .ThenInclude(roomJoinRequest => roomJoinRequest.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            var userName = requestModel.userName.ToUpper();

            responseModel.Response = (await _context.Users.Where(user => user.NormalizedUserName.Contains(userName))
                .ToListAsync())
                .Select(user => new SearchMemberAPIModel.Response()
                {
                    UserId = user.Id,
                    FirstName = user.Firstname,
                    LastName = user.Lastname,
                    UserName = user.UserName,
                    IsMember = room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id),
                    IsInvited = room.RoomJoinRequests.Where(roomJoinRequest=> roomJoinRequest.JoinRequestType == JoinRequestType.Invite).Select(roomUser => roomUser.User.Id).Contains(user.Id),
                    IsRequestingJoin = room.RoomJoinRequests.Where(roomJoinRequest => roomJoinRequest.JoinRequestType == JoinRequestType.Join).Select(roomUser => roomUser.User.Id).Contains(user.Id),
                }).ToList();


            // Returning the response
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to allow the owners of the room accept the join request 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("AcceptRequest")]
        public async Task<ActionResult<APIResponse>> AcceptRequest(AcceptRequestAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            var room = await _context.Rooms
               // Include the jion requests (join)
               .Include(room => room.RoomJoinRequests)
               // Include the user of the join request (join)
               .ThenInclude(roomJoinRequests => roomJoinRequests.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var joinRequest = room.RoomJoinRequests.FirstOrDefault(roomJoinRequest=>roomJoinRequest.Id==requestModel.JoinRequestId && roomJoinRequest.JoinRequestType== JoinRequestType.Join);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            var roomUser = new RoomUser()
            {
                User = joinRequest.User,
                Room = joinRequest.Room
            };

            await _context.RoomUsers.AddAsync(roomUser);

            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        
        /// <summary>
        /// End point to allow the owners of the room to cancel sent requests
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("CancelInvitaion")]
        public async Task<ActionResult<APIResponse>> CancelInvitaion(CancelInvitaionAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            var room = await _context.Rooms
               // Include the jion requests (join)
               .Include(room => room.RoomJoinRequests)
               // Include the user of the join request (join)
               .ThenInclude(roomJoinRequests => roomJoinRequests.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var joinRequest = room.RoomJoinRequests
                .FirstOrDefault(roomJoinRequest=>
                roomJoinRequest.Id==requestModel.JoinRequestId 
                && roomJoinRequest.JoinRequestType== JoinRequestType.Invite);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            // Remove the request
            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to allow the owners of the room to decline room join request
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("DeclineRequest")]
        public async Task<ActionResult<APIResponse>> DeclineRequest(DeclineRequestAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            var room = await _context.Rooms
               // Include the jion requests (join)
               .Include(room => room.RoomJoinRequests)
               // Include the user of the join request (join)
               .ThenInclude(roomJoinRequests => roomJoinRequests.User)
               // Select the required room and make sure that this user is a member of it
               .FirstOrDefaultAsync(room => room.Id == requestModel.RoomId &&
               room.RoomUsers.Select(roomUser => roomUser.User.Id).Contains(user.Id) &&
               room.RoomUsers.FirstOrDefault(roomUser => roomUser.User.Id == user.Id).Owner);

            // Checck if the user is a member of this room and the room exist
            if (room == null)
            {
                responseModel.AddError("رقم الغرفة غير صحيح");

                // Return the response
                return responseModel;
            }

            // get the record of the current user
            var joinRequest = room.RoomJoinRequests.FirstOrDefault(roomJoinRequest => roomJoinRequest.Id == requestModel.JoinRequestId && roomJoinRequest.JoinRequestType == JoinRequestType.Join);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            // Remove the join request
            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to allow the users to accept an invitation to join a room
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("AcceptInvitation")]
        public async Task<ActionResult<APIResponse>> AcceptInvitation(AcceptInvitationAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            // Getting the request
            var joinRequest = await _context.RoomJoinRequests
                .Include(roomJoinRequest => roomJoinRequest.User)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                .FirstOrDefaultAsync(roomJoinRequest => roomJoinRequest.Id == requestModel.JoinRequestId
                    && roomJoinRequest.User.Id == user.Id
                    && roomJoinRequest.JoinRequestType == JoinRequestType.Invite);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            var roomUser = new RoomUser()
            {
                User = joinRequest.User,
                Room = joinRequest.Room
            };

            await _context.RoomUsers.AddAsync(roomUser);

            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }


        /// <summary>
        /// End point to allow the owners of the room to cancel sent requests
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("CancelRequest")]
        public async Task<ActionResult<APIResponse>> CancelRequest(CancelRequestAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            // Getting the request
            var joinRequest = await _context.RoomJoinRequests
                .Include(roomJoinRequest => roomJoinRequest.User)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                .FirstOrDefaultAsync(roomJoinRequest => roomJoinRequest.Id == requestModel.JoinRequestId 
                    && roomJoinRequest.User.Id == user.Id
                    && roomJoinRequest.JoinRequestType == JoinRequestType.Join);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            // Remove the request
            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to allow the owners of the room to decline room join request
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost("DeclineInvitation")]
        public async Task<ActionResult<APIResponse>> DeclineInvitation(DeclineInvitationAPIModel.Request requestModel)
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse();

            // Getting the request
            var joinRequest = await _context.RoomJoinRequests
                .Include(roomJoinRequest => roomJoinRequest.User)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                .FirstOrDefaultAsync(roomJoinRequest => roomJoinRequest.Id == requestModel.JoinRequestId
                    && roomJoinRequest.User.Id == user.Id
                    && roomJoinRequest.JoinRequestType == JoinRequestType.Invite);

            if (joinRequest == null)
            {
                responseModel.AddError("هذا ليس طلب انضمام");

                // Return the response
                return responseModel;
            }

            // Remove the request
            _context.RoomJoinRequests.Remove(joinRequest);

            // save the changes
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(responseModel);
        }

        /// <summary>
        /// End point to allow the owners of the room to decline room join request
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpGet("GetMyRequests")]
        public async Task<ActionResult<APIResponse<List<RoomJoinRequestAPIModel.Response>>>> GetMyRequests()
        {
            // Get the current logged in user
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var responseModel = new APIResponse<List<RoomJoinRequestAPIModel.Response>>();

            // Getting the request
            responseModel.Response = await _context.RoomJoinRequests
                .Include(roomJoinRequest => roomJoinRequest.User)
                .Include(roomJoinRequest => roomJoinRequest.Room)
                .Where(roomJoinRequest => roomJoinRequest.User.Id == user.Id)
                .Select(roomJoinRequest=> new RoomJoinRequestAPIModel.Response() 
                {
                    FirstName = roomJoinRequest.User.Firstname,
                    LastName = roomJoinRequest.User.Lastname,
                    JoinRequestId = roomJoinRequest.Id,
                    JoinRequestType = roomJoinRequest.JoinRequestType,
                    UserId = roomJoinRequest.User.Id,
                    RoomId = roomJoinRequest.Room.Id,
                    RoomName = roomJoinRequest.Room.RoomName
                }
                ).ToListAsync();

            // Return the response
            return Ok(responseModel);
        }

    }
}
