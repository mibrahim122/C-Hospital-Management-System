using System;
using HospitalManagementSystem;

namespace HospitalManagementSystem
{
    // PATTERN: FACTORY
    // This class handles the logic of creating different user types (Doctor, Patient, Admin).
    public static class UserFactory
    {
        public static User CreateUser(string role, string name, string username, string password, string specialization = "", double fee = 0)
        {
            switch (role.ToLower())
            {
                case "doctor":
                    return new Doctor
                    {
                        Name = name,
                        Username = username,
                        Password = password,
                        Role = "Doctor",
                        Specialization = specialization,
                        ConsultationFee = fee,
                        AvailableSlots = new System.Collections.Generic.List<string>() // Initialize empty list
                    };

                case "patient":
                    return new Patient
                    {
                        Name = name,
                        Username = username,
                        Password = password,
                        Role = "Patient"
                    };

                case "admin":
                    return new Admin
                    {
                        Name = name,
                        Username = username,
                        Password = password,
                        Role = "Admin"
                    };

                default:
                    throw new ArgumentException("Invalid Role Type");
            }
        }
    }
}