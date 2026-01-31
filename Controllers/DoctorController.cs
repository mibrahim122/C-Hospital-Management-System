using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    public class DoctorController
    {
        private MainUI _view = new MainUI();
        private Doctor _currentDoc;

        public DoctorController(User doc) { _currentDoc = (Doctor)doc; }

        public void Run()
        {
            while (true)
            {
                _view.ShowTitle($"DR. {_currentDoc.Name} - DASHBOARD");
                _view.ShowMessage("1. View/Approve Pending Requests");
                _view.ShowMessage("2. View Booked Appointments");
                _view.ShowMessage("3. View Patient Medical & Billing History");
                _view.ShowMessage("4. Process Appointment (Prescribe & Bill)");
                _view.ShowMessage("5. Logout");

                string choice = _view.GetInput("Select Option");

                if (choice == "1") ApproveRequests();
                else if (choice == "2") ViewBookedAppointments();
                else if (choice == "3") ViewFullHistory();
                else if (choice == "4") ProcessVisit();
                else if (choice == "5") break;
            }
        }

        private void ApproveRequests()
        {
            _view.ShowTitle("PENDING APPROVALS");
            var pending = Database.Instance.Appointments.Find(a => a.DoctorId == _currentDoc.Id && a.Status == "Pending").ToList();

            if (pending.Count == 0) { _view.ShowMessage("No pending requests."); _view.PressAnyKey(); return; }

            for (int i = 0; i < pending.Count; i++)
                _view.ShowMessage($"{i + 1}. {pending[i].PatientName} (ID: {pending[i].PatientId}) - Slot: {pending[i].TimeSlot}");

            string input = _view.GetInput("Select Patient # to Approve (0 to cancel)");
            if (int.TryParse(input, out int idx) && idx > 0 && idx <= pending.Count)
            {
                Database.Instance.Appointments.UpdateOne(a => a.Id == pending[idx - 1].Id,
                    Builders<Appointment>.Update.Set(a => a.Status, "Approved"));
                _view.ShowMessage("Appointment Approved!");
            }
            _view.PressAnyKey();
        }

        private void ViewBookedAppointments()
        {
            _view.ShowTitle("MY BOOKED APPOINTMENTS");
            var booked = Database.Instance.Appointments.Find(a => a.DoctorId == _currentDoc.Id && a.Status == "Approved").ToList();

            if (booked.Count == 0)
            {
                _view.ShowMessage("No upcoming booked appointments.");
            }
            else
            {
                foreach (var app in booked)
                {
                    _view.ShowMessage($"- {app.PatientName} (ID: {app.PatientId}) | Time: {app.TimeSlot}");
                }
            }
            _view.PressAnyKey();
        }

        private void ViewFullHistory()
        {
            _view.ShowTitle("PATIENT RECORD SEARCH");

            // FIX: Search by Unique Patient ID (or we could use Username if we had it stored in appointments)
            // But since Appointment stores PatientId, that is the safest unique key.
            // We can first ask for the exact Username to find the ID.

            string username = _view.GetInput("Enter Patient Username (Unique ID)");

            // 1. Find the User first to get their ID
            var patientUser = Database.Instance.Users.Find(u => u.Username == username && u.Role == "Patient").FirstOrDefault();

            if (patientUser == null)
            {
                _view.ShowError("Patient not found with that username.");
                _view.PressAnyKey();
                return;
            }

            string targetId = patientUser.Id;
            string targetName = patientUser.Name;

            _view.ShowMessage($"\nFound Patient: {targetName} (ID: {targetId})");

            // 2. Search records using the Unique ID
            var medical = Database.Instance.Appointments.Find(a => a.PatientId == targetId && a.Status == "Completed").ToList();

            _view.ShowMessage("\n--- MEDICAL RECORDS ---");
            if (!medical.Any())
            {
                _view.ShowMessage("No clinical history found.");
            }
            else
            {
                foreach (var m in medical)
                {
                    string testDisplay = (m.AssignedTests != null && m.AssignedTests.Count > 0)
                                         ? string.Join(", ", m.AssignedTests)
                                         : "No test records";

                    _view.ShowMessage($"[{m.Date.ToShortDateString()}] Rx: {m.Prescription} | Tests: {testDisplay}");
                }
            }

            var bills = Database.Instance.Invoices.Find(i => i.PatientId == targetId).SortByDescending(i => i.BillingDateTime).ToList();
            _view.ShowMessage("\n--- BILLING HISTORY ---");
            if (!bills.Any()) _view.ShowMessage("No payment history found.");
            foreach (var b in bills)
                _view.ShowMessage($"${b.Amount:F2} paid on {b.BillingDateTime:dddd, dd MMMM yyyy}");

            _view.PressAnyKey();
        }

        private void ProcessVisit()
        {
            _view.ShowTitle("TREATMENT & BILLING MODULE");
            var approved = Database.Instance.Appointments.Find(a => a.DoctorId == _currentDoc.Id && a.Status == "Approved").ToList();

            if (approved.Count == 0) { _view.ShowMessage("No active patients to process."); _view.PressAnyKey(); return; }

            for (int i = 0; i < approved.Count; i++) _view.ShowMessage($"{i + 1}. {approved[i].PatientName} (Username: {GetUsernameById(approved[i].PatientId)})");

            if (int.TryParse(_view.GetInput("Select Patient #"), out int idx) && idx > 0 && idx <= approved.Count)
            {
                var app = approved[idx - 1];

                string rx = _view.GetInput("Enter Prescription");
                List<string> testList = new List<string>();
                string askTest = _view.GetInput("Order Lab Tests? (y/n)").ToLower();

                if (askTest == "y" || askTest == "yes")
                {
                    string tests = _view.GetInput("Enter Lab Tests (comma separated)");
                    testList = tests.Split(',').Select(t => t.Trim()).ToList();

                    foreach (var tName in testList)
                    {
                        var labReq = new LabTest
                        {
                            PatientId = app.PatientId,
                            PatientName = app.PatientName,
                            TestName = tName,
                            Status = "Pending"
                        };
                        Database.Instance.LabTests.InsertOne(labReq);
                    }
                }

                double charge = double.Parse(_view.GetInput($"Enter Charge Amount (Default Fee: {_currentDoc.ConsultationFee})"));

                Database.Instance.Appointments.UpdateOne(a => a.Id == app.Id,
                    Builders<Appointment>.Update
                    .Set(a => a.Prescription, rx)
                    .Set(a => a.AssignedTests, testList)
                    .Set(a => a.Status, "Completed"));

                var invoice = new Invoice
                {
                    PatientId = app.PatientId,
                    PatientName = app.PatientName,
                    DoctorId = _currentDoc.Id,
                    DoctorName = _currentDoc.Name,
                    Amount = charge,
                    BillingDateTime = DateTime.Now
                };
                Database.Instance.Invoices.InsertOne(invoice);

                _view.ShowMessage("\n[SUCCESS] Medical records updated and Billing generated.");
            }
            _view.PressAnyKey();
        }

        // Helper to get username for display
        private string GetUsernameById(string id)
        {
            var u = Database.Instance.Users.Find(x => x.Id == id).FirstOrDefault();
            return u != null ? u.Username : "Unknown";
        }
    }
}