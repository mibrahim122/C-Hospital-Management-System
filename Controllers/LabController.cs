using System;
using System.Linq; // Required for ToList()
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    public class LabController
    {
        private MainUI _view = new MainUI();
        private User _labTech;

        public LabController(User user)
        {
            _labTech = user;
        }

        public void Run()
        {
            while (true)
            {
                _view.ShowTitle($"LABORATORY DASHBOARD - {_labTech.Name}");
                _view.ShowMessage("1. View Pending Tests");
                _view.ShowMessage("2. Update Test Results");
                _view.ShowMessage("3. Logout");

                string choice = _view.GetInput("Select Option");

                if (choice == "1") ViewPendingTests();
                else if (choice == "2") UpdateTestResults();
                else if (choice == "3") return;
            }
        }

        private void ViewPendingTests()
        {
            _view.ShowTitle("PENDING LAB TESTS");

            // Fix 1: Use Database.Instance.LabTests
            // Fix 2: Add .ToList() to fix the "method group" error
            var pending = Database.Instance.LabTests.Find(t => t.Status == "Pending").ToList();

            if (pending.Count == 0) // Now .Count works because it is a List
            {
                _view.ShowMessage("No pending tests.");
            }
            else
            {
                foreach (var test in pending)
                {
                    _view.ShowMessage($"- {test.TestName} for {test.PatientName} (ID: {test.Id})");
                }
            }
            _view.PressAnyKey();
        }

        private void UpdateTestResults()
        {
            _view.ShowTitle("UPDATE TEST RESULTS");

            string testId = _view.GetInput("Enter Test ID");
            string result = _view.GetInput("Enter Result/Notes");

            // Fix: Use Database.Instance
            var updateDef = Builders<LabTest>.Update
                .Set(t => t.Result, result)
                .Set(t => t.Status, "Completed");

            var resultDb = Database.Instance.LabTests.UpdateOne(t => t.Id == testId, updateDef);

            if (resultDb.ModifiedCount > 0)
                _view.ShowMessage("Test Updated Successfully.");
            else
                _view.ShowError("Test ID not found.");

            _view.PressAnyKey();
        }
    }
}