using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalManagementSystem
{
    public class LabTest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string TestName { get; set; }
        public string Status { get; set; } = "Pending";
        public string Result { get; set; } = "";
    }
}