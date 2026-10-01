using AA.Pango.Model;
using LiteDB;
using log4net;
using System.Collections.Generic;
using System.Linq;

namespace AA.Pango.ServiceLayer
{
    /// <summary>
    /// </summary>
    public class ParkingService
    {
        private static string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LiteDB"].ConnectionString;

        //private static volatile ILiteDatabase _database = new LiteDatabase(_connectionString);

        private static ILog Log = LogManager.GetLogger(typeof(ParkingService));

        public static void Seed(string name = "Parking 1")
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkingDb = db.GetCollection<Parking>("parkings");

                if (parkingDb.Count() == 0)
                {
                    var park = new Parking
                    {
                        Name = name,
                        MaximumCapacity = 100,
                        PlcIpAddress = "192.168.0.3",
                        ActualCapacity = 0
                    };

                    parkingDb.Insert(park);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="badgeId"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="isActive"></param>
        /// <param name="nationalId"></param>
        /// <returns></returns>
        public static int InsertParking(string name, int maximumCapacity)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkingsDb = db.GetCollection<Parking>("parkings");

                var parking = new Parking
                {
                    MaximumCapacity = maximumCapacity,
                    ActualCapacity = 0,
                    Name = name,
                };

                var result = parkingsDb.Insert(parking);
                return result.AsInt32;
            }
        }

        public static bool UpdateParking(int _id, string name, int maximumCapacity, string ip)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkingsDb = db.GetCollection<Parking>("parkings");

                var parking = new Parking
                {
                    _id = _id,
                    MaximumCapacity = maximumCapacity,
                    Name = name,
                    PlcIpAddress = ip
                };

                return parkingsDb.Update(parking);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parkingId"></param>
        /// <param name="difference"></param>
        /// <returns></returns>
        public static bool AdjustParkingCapacity(int parkingId, int difference)
        {
            var result = false;

            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkings = db.GetCollection<Parking>("parkings");

                // Use Linq to query documents
                var parking = parkings.FindOne(x => x._id == parkingId);

                if (parking != null)
                {
                    parking.ActualCapacity += difference;
                    result = parkings.Update(parking);
                }

                return result;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="parkingId"></param>
        /// <returns></returns>
        public static Parking GetParking(int parkingId)
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkings = db.GetCollection<Parking>("parkings");

                // Use Linq to query parking
                return parkings.FindOne(x => x._id == parkingId);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Parking> GetParkings()
        {
            using (var db = new LiteDatabase(_connectionString))
            {
                // Get customer collection
                var parkings = db.GetCollection<Parking>("parkings");

                if (parkings.Count() == 0)
                {
                    //seed parking repo with default parking
                    var pk = new Parking
                    {
                        Name = "Parking 1",
                        ActualCapacity = 0,
                        MaximumCapacity = 0,
                        PlcIpAddress = "192.168.0.1",
                    };
                    parkings.Insert(pk);
                }

                // Use Linq to query parking
                var pkList = parkings.FindAll().ToList();

                return pkList;
            }
        }

        public static int ValidateEntry(int parkingId, string customerId)
        {
            //var isAuthorized = CustomerService.IsAuthorized(customerId);

            //if (isAuthorized)
            //{
            //    Log.Info("Empleado Existente y Activo en la base de datos: " + customerId);

            // var parking = ParkingService.GetParking(parkingId); var statusResponse = GetStatus(parkingId);

            // Log.Info("Respuesta Status: " + statusResponse != null && statusResponse.Length > 0 ?
            // statusResponse[0] : "vacio");

            // if (statusResponse != null && statusResponse.Length > 1 //&& //(statusResponse[0] ==
            // ((int)Parking.State.ACCESS_REQUESTED).ToString() || // statusResponse[0] ==
            // ((int)Parking.State.STAND_BY).ToString()) ) { int diff = 1;
            // AdjustParkingCapacity(parking._id, diff);

            // Log.Info("Status Procesado");

            // // if the state we got is access requested, then we have to pass // the state to
            // access validated (3) so the state machine // can continue moving return
            // plcFacade.SetStatusVariable(parking.PlcIpAddress,
            // (int)Parking.State.ACCESS_VALIDATED) ? 1 : 0; } else { return -1; }

            //}else
            //{
            //    return -2;
            //}
            return -1;
        }
    }
}