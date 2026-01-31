using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    public class PatientController
    {
        private MainUI _view = new MainUI();
        private User _patient;

        public PatientController(User user)
        {
            _patient = user;
        }

        public void Run()
        {
            while (true)
            {
                _view.ShowTitle($"PATIENT DASHBOARD - {_patient.Name}");
                _view.ShowMessage("1. Book Appointment");
                _view.ShowMessage("2. View Upcoming Appointments");
                _view.ShowMessage("3. Medical History & Prescriptions");
                _view.ShowMessage("4. View Billing & Invoices");
                _view.ShowMessage("5. View Lab Test Results");
                _view.ShowMessage("6. Logout");

                string choice = _view.GetInput("Select Option");

                if (choice == "1") BookAppointment();
                else if (choice == "2") ViewUpcomingAppointments();
                else if (choice == "3") ViewMedicalHistory();
                else if (choice == "4") ViewBillingHistory();
                else if (choice == "5") ViewLabResults();
                else if (choice == "6") return;
            }
        }

        private void BookAppointment()
        {
            _view.ShowTitle("BOOK APPOINTMENT");

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

            string docInput = _view.GetInput("Select Doctor #");

            if (int.TryParse(docInput, out int docIndex) && docIndex > 0 && docIndex <= doctors.Count)
            {
                var selectedDoc = doctors[docIndex - 1];

                if (selectedDoc.AvailableSlots == null || selectedDoc.AvailableSlots.Count == 0)
                {
                    _view.ShowError("This doctor has no available slots.");
                    _view.PressAnyKey();
                    return;
                }

                _view.ShowTitle($"AVAILABLE SLOTS - Dr. {selectedDoc.Name}");
                for (int i = 0; i < selectedDoc.AvailableSlots.Count; i++)
                {
                    _view.ShowMessage($"{i + 1}. {selectedDoc.AvailableSlots[i]}");
                }

                string slotInput = _view.GetInput("Select Slot #");
                if (int.TryParse(slotInput, out int slotIndex) && slotIndex > 0 && slotIndex <= selectedDoc.AvailableSlots.Count)
                {
                    string selectedSlot = selectedDoc.AvailableSlots[slotIndex - 1];

                    var facade = new AppointmentFacade();
                    string result = facade.BookAppointment(_patient, selectedDoc, selectedSlot);

                    if (result == "Success")
                        _view.ShowMessage("Appointment Request Sent Successfully!");
                    else
                        _view.ShowError(result);
                }
                else _view.ShowError("Invalid Slot Selection.");
            }
            else _view.ShowError("Invalid Doctor Selection.");

            _view.PressAnyKey();
        }

        private void ViewUpcomingAppointments()
        {
            _view.ShowTitle("UPCOMING APPOINTMENTS");
            var myApps = Database.Instance.Appointments
                .Find(a => a.PatientId == _patient.Id && (a.Status == "Pending" || a.Status == "Approved"))
                .ToList();

            if (myApps.Count == 0) _view.ShowMessage("No upcoming appointments.");
            else
            {
                foreach (var app in myApps)
                {
                    _view.ShowMessage($"Date: {app.Date.ToShortDateString()} | Dr. {app.DoctorName} | Slot: {app.TimeSlot} | Status: {app.Status}");
                }
            }
            _view.PressAnyKey();
        }

        private void ViewMedicalHistory()
        {
            _view.ShowTitle("MEDICAL HISTORY & PRESCRIPTIONS");
            // Only show completed appointments where treatment was given
            var history = Database.Instance.Appointments
                .Find(a => a.PatientId == _patient.Id && a.Status == "Completed")
                .ToList();

            if (history.Count == 0) _view.ShowMessage("No medical history found.");
            else
            {
                foreach (var record in history)
                {
                    _view.ShowMessage("------------------------------------------------");
                    _view.ShowMessage($"Date: {record.Date.ToShortDateString()}");
                    _view.ShowMessage($"Doctor: Dr. {record.DoctorName}");
                    _view.ShowMessage($"Prescription: {record.Prescription}");

                    if (record.AssignedTests != null && record.AssignedTests.Count > 0)
                    {
                        string tests = string.Join(", ", record.AssignedTests);
                        _view.ShowMessage($"Tests Ordered: {tests}");
                    }
                }
                _view.ShowMessage("------------------------------------------------");
            }
            _view.PressAnyKey();
        }

        private void ViewBillingHistory()
        {
            _view.ShowTitle("BILLING HISTORY");
            var bills = Database.Instance.Invoices
                .Find(i => i.PatientId == _patient.Id)
                .ToList();

            if (bills.Count == 0) _view.ShowMessage("No invoices found.");
            else
            {
                foreach (var bill in bills)
                {
                    _view.ShowMessage($"Date: {bill.BillingDateTime} | Amount: ${bill.Amount} | Status: {bill.Status} | Dr. {bill.DoctorName}");
                }
            }
            _view.PressAnyKey();
        }

        private void ViewLabResults()
        {
            _view.ShowTitle("LAB TEST RESULTS");
            var labs = Database.Instance.LabTests
                .Find(t => t.PatientId == _patient.Id)
                .ToList();

            if (labs.Count == 0) _view.ShowMessage("No lab tests found.");
            else
            {
                foreach (var test in labs)
                {
                    string resultDisplay = string.IsNullOrEmpty(test.Result) ? "Waiting for Lab Tech" : test.Result;
                    _view.ShowMessage($"Test: {test.TestName} | Status: {test.Status} | Result: {resultDisplay}");
                }
            }
            _view.PressAnyKey();
        }
    }
}