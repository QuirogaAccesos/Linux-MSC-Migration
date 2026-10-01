using System.Collections.Generic;

namespace AA.Pango.Model
{
    public class Parking
    {
        /// <summary>
        /// State Machine to Use for the parking
        /// </summary>
        public enum State
        {
            STAND_BY = 1,
            ACCESS_REQUESTED = 2,
            ACCESS_VALIDATED = 3,
            VEHICLE_ENTERING = 4,
            VEHICLE_EXITING = 5,
            EXIT_GATE_OPENED = 6,
            PARKING_FULL = 7,

            //ERRORS
            CONNECTION_LOST = -1
        }

        public int _id { get; set; }
        public string Name { get; set; }
        public int MaximumCapacity { get; set; }
        public int ActualCapacity { get; set; }
        public string PlcIpAddress { get; set; }

        public List<Terminal> Terminals { get; set; }
    }
}