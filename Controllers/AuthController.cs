using System;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    public static class AuthController
    {
        public static void Login()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            // FIX: Use Database.Instance.Users
            var user = Database.Instance.Users.Find(u => u.Username == username && u.Password == password).FirstOrDefault();

            if (user != null)
            {
                if (user.Role == "Admin") new AdminController(user).Run();
                else if (user.Role == "Doctor") new DoctorController(user).Run();
                else if (user.Role == "Patient") new PatientController(user).Run();
                else if (user.Role == "LabTech") new LabController(user).Run();
            }
            else
            {
                Console.WriteLine("Invalid Credentials!");
                Console.ReadKey();
            }
        }
    }
}