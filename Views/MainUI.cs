using System;

namespace HospitalManagementSystem
{
    public class MainUI
    {
        public void ShowTitle(string title)
        {
            Console.Clear();
            Console.WriteLine($"=== {title.ToUpper()} ===");
            Console.WriteLine("-----------------------------------------");
        }
        public string GetInput(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine();
        }
        public void ShowMessage(string msg) => Console.WriteLine(msg);
        public void ShowError(string err)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {err}");
            Console.ResetColor();
        }
        public void PressAnyKey()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}