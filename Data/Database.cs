// using MongoDB.Driver;

// namespace HospitalManagementSystem
// {
//     public static class Database
//     {
//         // FIXED: Added '= null!;' to suppress the "must contain non-null value" warning.
//         private static IMongoDatabase _db = null!;

//         public static void Connect()
//         {
//             var client = new MongoClient("mongodb://localhost:27017");
//             _db = client.GetDatabase("HospitalManagementDB");
//         }

//         public static IMongoCollection<User> Users => _db.GetCollection<User>("Users");
//         public static IMongoCollection<Appointment> Appointments => _db.GetCollection<Appointment>("Appointments");
//         public static IMongoCollection<Invoice> Invoices => _db.GetCollection<Invoice>("Invoices");
//         public static IMongoCollection<LabTest> LabTests => _db.GetCollection<LabTest>("LabTests");
//     }
// }
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    // PATTERN: SINGLETON
    public sealed class Database
    {
        private static Database _instance = null;
        private static readonly object _padlock = new object();

        // Collections
        public IMongoCollection<User> Users { get; private set; }
        public IMongoCollection<Appointment> Appointments { get; private set; }
        public IMongoCollection<Invoice> Invoices { get; private set; }
        public IMongoCollection<LabTest> LabTests { get; private set; } // <--- ADDED THIS

        private Database()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("HospitalManagementDB");

            Users = db.GetCollection<User>("Users");
            Appointments = db.GetCollection<Appointment>("Appointments");
            Invoices = db.GetCollection<Invoice>("Invoices");
            LabTests = db.GetCollection<LabTest>("LabTests"); // <--- ADDED THIS
        }

        public static Database Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new Database();
                    }
                    return _instance;
                }
            }
        }
    }
}