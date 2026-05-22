// using System;
// using System.Diagnostics;
// using System.Runtime.InteropServices;

// class Program
// {
//     [DllImport("kernel32.dll")]
//     public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

//     [DllImport("kernel32.dll", SetLastError = true)]
//     public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

//     [DllImport("kernel32.dll")]
//     public static extern bool CloseHandle(IntPtr hObject);

//     const int PROCESS_VM_READ = 0x0010;

//     static void Main()
//     {
//         string processName = "Kena-Win64-Shipping_original"; // Replace with the actual process name (without .exe)
//         IntPtr ammoAddress = (IntPtr)0x12345678; // Replace with the actual memory address of ammo

//         // Find the game process
//         Process[] processes = Process.GetProcessesByName(processName);
//         if (processes.Length == 0)
//         {
//             Console.WriteLine("Game process not found!");
//             return;
//         }

//         Process gameProcess = processes[0];
//         IntPtr processHandle = OpenProcess(PROCESS_VM_READ, false, gameProcess.Id);

//         if (processHandle == IntPtr.Zero)
//         {
//             Console.WriteLine("Failed to open process!");
//             return;
//         }

//         while (true)
//         {
//             byte[] buffer = new byte[4]; // Assuming ammo is a 4-byte integer
//             ReadProcessMemory(processHandle, ammoAddress, buffer, buffer.Length, out int bytesRead);

//             if (bytesRead > 0)
//             {
//                 int ammoValue = BitConverter.ToInt32(buffer, 0);
//                 Console.WriteLine($"Ammo: {ammoValue}");

//                 if (ammoValue < 1)
//                 {
//                     Console.WriteLine("Out of ammo! Take action.");
//                 }
//             }
//             else
//             {
//                 Console.WriteLine("Failed to read memory!");
//             }

//             System.Threading.Thread.Sleep(500); // Delay to reduce CPU usage
//         }

//         CloseHandle(processHandle);
//     }
// }