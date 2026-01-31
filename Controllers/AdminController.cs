using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace HospitalManagementSystem
{
    public class AdminController
    {
        private MainUI _view = new MainUI();
        private User _admin; // Store the admin user

        // === FIXED: Restore the Constructor ===
        public AdminController(User admin)
        {
            _admin = admin;
        }

        public void Run()
        {
            while (true)
            {
                _view.ShowTitle($"ADMIN DASHBOARD");
                _view.ShowMessage("1. Add New Doctor");
                _view.ShowMessage("2. View All Doctors");
                _view.ShowMessage("3. View All Patients");
                _view.ShowMessage("4. Logout");

                string choice = _view.GetInput("Select Option");

                if (choice == "1") AddDoctor();
                else if (choice == "2") ViewAllDoctors();
                else if (choice == "3") ViewAllPatients();
                else if (choice == "4") return;
            }
        }

        private void AddDoctor()
        {
            _view.ShowTitle("ADD NEW DOCTOR");

            string name = _view.GetInput("Enter Doctor Name");
            string username = _view.GetInput("Enter Username");

            // CHECK FOR DUPLICATES
            var existingUser = Database.Instance.Users.Find(u => u.Username == username).FirstOrDefault();
            if (existingUser != null)
            {
                _view.ShowError($"Error: The username '{username}' is already taken.");
                _view.PressAnyKey();
                return;
            }

            string password = _view.GetInput("Enter Password");
            string spec = _view.GetInput("Enter Specialization");

            if (!double.TryParse(_view.GetInput("Enter Consultation Fee"), out double fee))
            {
                _view.ShowError("Invalid Fee Amount.");
                return;
            }

            string slotsInput = _view.GetInput("Enter Available Slots (comma separated, e.g. 10:00 AM, 02:00 PM)");
            List<string> slots = new List<string>(slotsInput.Split(','));
            for (int i = 0; i < slots.Count; i++) slots[i] = slots[i].Trim();

            var newDoctor = new Doctor
            {
                Name = name,
                Username = username,
                Password = password,
                Role = "Doctor",
                Specialization = spec,
                ConsultationFee = fee,
                AvailableSlots = slots
            };

            Database.Instance.Users.InsertOne(newDoctor);
            _view.ShowMessage("Success! Doctor Added.");
            _view.PressAnyKey();
        }

        private void ViewAllDoctors()
        {
            _view.ShowTitle("ALL DOCTORS");
            var doctors = Database.Instance.Users.OfType<Doctor>().Find(_ => true).ToList();

            foreach (var d in doctors)
            {
                _view.ShowMessage($"Dr. {d.Name} ({d.Specialization}) - User: {d.Username}");
            }
            _view.PressAnyKey();
        }

        private void ViewAllPatients()
        {
            _view.ShowTitle("ALL PATIENTS");
            var patients = Database.Instance.Users.OfType<Patient>().Find(_ => true).ToList();

            foreach (var p in patients)
            {
                _view.ShowMessage($"{p.Name} (User: {p.Username})");
            }
            _view.PressAnyKey();
        }
    }
}