using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HospitalManagementSystem
{
    // === USER MODELS ===
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(Doctor), typeof(Patient), typeof(Admin))]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class Doctor : User
    {
        public string Specialization { get; set; } = string.Empty;
        public double ConsultationFee { get; set; }
        public List<string> AvailableSlots { get; set; } = new List<string>();
    }

    public class Patient : User { }

    public class Admin : User { }

    // === CLINICAL MODELS ===
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string Prescription { get; set; } = "None";
        public List<string> AssignedTests { get; set; } = new List<string>();
    }

    // === FINANCIAL MODELS ===
    public class Invoice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string PatientId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public DateTime BillingDateTime { get; set; }
        public string Status { get; set; } = "Paid";
    }

    // Note: 'LabTest' has been moved to Models/LabTest.cs to fix the duplicate error.
}