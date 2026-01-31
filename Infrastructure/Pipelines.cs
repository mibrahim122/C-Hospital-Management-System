using System;

namespace HospitalManagementSystem
{
    public static class EventSystem
    {
        public static void TriggerBooking(Appointment app)
        {
            Console.WriteLine($"[EVENT] Appointment Booked: {app.PatientName} with Dr. {app.DoctorName}");
        }
    }
}