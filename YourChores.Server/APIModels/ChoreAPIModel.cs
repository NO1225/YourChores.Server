using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.Enums;
using YourChores.Data.Models;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model for the chores 
    /// </summary>
    public class ChoreAPIModel
    {
        /// <summary>
        /// The response model
        /// </summary>
        public class Response
        {
            /// <summary>
            /// The id of the requested chore
            /// </summary>
            public int ChoreId { get; set; }

            /// <summary>
            /// The description of the chore
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The urgency of the chore
            /// </summary>
            public Urgency Urgency { get; set; }

            /// <summary>
            /// The date this chore was created
            /// </summary>
            public DateTime CreatedOn { get; set; }

            /// <summary>
            /// If this chore was already done
            /// </summary>
            public bool Done { get; set; }

            /// <summary>
            /// The id of the room this chore posted in
            /// </summary>
            public int RoomId { get; set; }

            /// <summary>
            /// The name of the room this chore posted in
            /// </summary>
            public string RoomName { get; set; }

        }

    }
}
