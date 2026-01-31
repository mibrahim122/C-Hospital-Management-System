using System;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize Admin if not exists (Ensures you can always log in as Admin)
            var adminCheck = Database.Instance.Users.Find(u => u.Role == "Admin").FirstOrDefault();
            if (adminCheck == null)
            {
                var admin = new Admin { Name = "System Admin", Username = "admin", Password = "admin", Role = "Admin" };
                Database.Instance.Users.InsertOne(admin);
            }

            // 2. Main Loop
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== HOSPITAL MANAGEMENT SYSTEM ===");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register as Patient"); // <--- NEW OPTION
                Console.WriteLine("3. Exit");
                Console.Write("Select Option: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    AuthController.Login();
                }
                else if (choice == "2")
                {
                    RegisterPatient(); // <--- CALL NEW METHOD
                }
                else if (choice == "3")
                {
                    break;
                }
            }
        }

        // === NEW METHOD: PATIENT REGISTRATION ===
        static void RegisterPatient()
        {
            Console.Clear();
            Console.WriteLine("=== PATIENT REGISTRATION ===");

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Username: ");
            string username = Console.ReadLine();

            // Check if username taken
            var existing = Database.Instance.Users.Find(u => u.Username == username).FirstOrDefault();
            if (existing != null)
            {
                Console.WriteLine("Error: Username already exists!");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter Password: ");
            string password = Console.ReadLine();

            // Create Patient Object
            var newPatient = new Patient
            {
                Name = name,
                Username = username,
                Password = password,
                Role = "Patient"
            };

            // Save to Database
            Database.Instance.Users.InsertOne(newPatient);

            Console.WriteLine("Success! Account created. You can now Login.");
            Console.ReadKey();
        }
    }
}