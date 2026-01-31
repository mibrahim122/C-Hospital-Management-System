using System;
using MongoDB.Driver;

namespace HospitalManagementSystem
{
    // PATTERN: FACADE
    // Provides a simplified interface to a complex subsystem (Booking Logic).
    public class AppointmentFacade
    {
        public string BookAppointment(User patient, Doctor doctor, string timeSlot)
        {
            // 1. Validation Logic
            if (doctor.AvailableSlots.Count == 0 || !doctor.AvailableSlots.Contains(timeSlot))
            {
                return "Error: Slot not available.";
            }

            // 2. Create Appointment Object
            var app = new Appointment
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.Name,
                PatientId = patient.Id,
                PatientName = patient.Name,
                Date = DateTime.Now,
                TimeSlot = timeSlot,
                Status = "Pending"
            };

            // 3. Save to Database (Using Singleton)
            Database.Instance.Appointments.InsertOne(app);

            // 4. Trigger Notification (Observer Pattern)
            EventSystem.TriggerBooking(app);

            return "Success";
        }
    }
}