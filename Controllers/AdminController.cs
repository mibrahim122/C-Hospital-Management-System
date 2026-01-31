using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    public class AdminController
    {
        private MainUI _view = new MainUI();
        private User _admin;

        public AdminController(User admin)
        {
            _admin = admin;
        }

        public void Run()
        {
            while (true)
            {
                _view.ShowTitle($"ADMIN DASHBOARD - {_admin.Name}");
                _view.ShowMessage("1. Provider Management (Add/Remove Doctor)");
                _view.ShowMessage("2. Staff Analytics & Revenue");
                _view.ShowMessage("3. Appointment Oversight (Approve/Cancel)");
                _view.ShowMessage("4. Set Doctor Scheduling");
                _view.ShowMessage("5. Logout");

                string choice = _view.GetInput("Select Option");

                if (choice == "1") ProviderManagement();
                else if (choice == "2") StaffAnalytics();
                else if (choice == "3") Oversight();
                else if (choice == "4") ScheduleManagement();
                else if (choice == "5") return;
            }
        }

        // === FIXED METHOD with FACTORY PATTERN ===
        private void ProviderManagement()
        {
            _view.ShowTitle("PROVIDER MANAGEMENT");
            _view.ShowMessage("1. Add Doctor");
            _view.ShowMessage("2. Remove Doctor");

            string op = _view.GetInput("Choice");

            if (op == "1")
            {
                string name = _view.GetInput("Name");
                string spec = _view.GetInput("Specialization");
                string feeStr = _view.GetInput("Consultation Fee");
                string username = _view.GetInput("Create Username");
                string password = _view.GetInput("Create Password");

                if (double.TryParse(feeStr, out double fee))
                {
                    try
                    {
                        // USE FACTORY
                        User newDoc = UserFactory.CreateUser("Doctor", name, username, password, spec, fee);

                        // USE SINGLETON
                        Database.Instance.Users.InsertOne(newDoc);

                        _view.ShowMessage("Doctor Created via Factory!");
                    }
                    catch (Exception ex)
                    {
                        _view.ShowError($"Error: {ex.Message}");
                    }
                }
                else
                {
                    _view.ShowError("Invalid Fee Amount.");
                }
            }
            else if (op == "2")
            {
                string username = _view.GetInput("Enter Username to Remove");
                // USE SINGLETON
                var result = Database.Instance.Users.DeleteOne(u => u.Username == username && u.Role == "Doctor");

                if (result.DeletedCount > 0) _view.ShowMessage("Doctor Removed.");
                else _view.ShowError("Doctor not found.");
            }
            _view.PressAnyKey();
        }

        private void StaffAnalytics()
        {
            _view.ShowTitle("ANALYTICS");
            // Use Singleton Database.Instance
            var doctors = Database.Instance.Users.OfType<Doctor>().Find(_ => true).ToList();
            var totalRevenue = Database.Instance.Invoices.AsQueryable().Sum(i => i.Amount);

            _view.ShowMessage($"Total Hospital Revenue: ${totalRevenue}");
            _view.ShowMessage($"Total Doctors: {doctors.Count}");
            _view.PressAnyKey();
        }

        private void Oversight()
        {
            _view.ShowTitle("APPOINTMENT OVERSIGHT");
            var pending = Database.Instance.Appointments.Find(a => a.Status == "Pending").ToList();

            if (pending.Count == 0)
            {
                _view.ShowMessage("No pending appointments.");
                _view.PressAnyKey();
                return;
            }

            for (int i = 0; i < pending.Count; i++)
            {
                _view.ShowMessage($"{i + 1}. {pending[i].PatientName} -> Dr. {pending[i].DoctorName}");
            }

            string input = _view.GetInput("Select # to Approve (0 to Go Back)");

            if (int.TryParse(input, out int selection) && selection > 0 && selection <= pending.Count)
            {
                var app = pending[selection - 1];
                var updateDef = Builders<Appointment>.Update.Set(a => a.Status, "Approved");
                Database.Instance.Appointments.UpdateOne(a => a.Id == app.Id, updateDef);
                _view.ShowMessage($"Success! Appointment for {app.PatientName} approved.");
            }
            _view.PressAnyKey();
        }

        private void ScheduleManagement()
        {
            _view.ShowTitle("SCHEDULE MANAGEMENT");

            var doctors = Database.Instance.Users.OfType<Doctor>().Find(_ => true).ToList();
            if (doctors.Count == 0)
            {
                _view.ShowError("No doctors found.");
                _view.PressAnyKey();
                return;
            }

            for (int i = 0; i < doctors.Count; i++)
            {
                _view.ShowMessage($"{i + 1}. Dr. {doctors[i].Name} ({doctors[i].Specialization})");
            }

            string input = _view.GetInput("Select Doctor to Add Slot");
            if (int.TryParse(input, out int index) && index > 0 && index <= doctors.Count)
            {
                var doc = doctors[index - 1];
                string newSlot = _view.GetInput("Enter Slot (e.g., 09:00 AM - 10:00 AM)");

                if (!string.IsNullOrWhiteSpace(newSlot))
                {
                    var update = Builders<Doctor>.Update.Push(d => d.AvailableSlots, newSlot);
                    Database.Instance.Users.OfType<Doctor>().UpdateOne(d => d.Id == doc.Id, update);
                    _view.ShowMessage("Slot Added Successfully!");
                }
            }
            _view.PressAnyKey();
        }
    }
}