// using Newtonsoft.Json;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// // using System.Security.Cryptography;
// using System.Text;
// using Memory;
// // using HidSharp;

// namespace DualSenseRE4R
// {
//     internal class Program
//     {
//         static UdpClient? client;
//         static IPEndPoint? endPoint;

//         static DateTime TimeSent;

//         static Process[] gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");


//         static void Connect()
//         {
//             client = new UdpClient();
//             var portNumber = File.ReadAllText(@"C:\Temp\DualSenseX\DualSenseX_PortNumber.txt");
//             endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(portNumber));
//             Console.WriteLine($"Port number found is: {portNumber}\n");
//         }

//         static void Send(Packet data)
//         {
//             var RequestData = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
//             client!.Send(RequestData, RequestData.Length, endPoint);
//             TimeSent = DateTime.Now;
//         }

//         static bool IsValidMemory(string ammoPointer, string healthPointer, Mem mem)
//         {
//             try
//             {
//                 Console.WriteLine("Addresses are invalid or zero. Waiting...\n");

//                 // IntPtr ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
//                 // ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
//                 // IntPtr pointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
//                 // healthPointer = mem.ResolvePointer(pointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);
//                 // int ammo = mem.ReadInt(ammoPointer);
//                 // float health = mem.SafeReadFloat(healthPointer);
//                  int ammo = mem.ReadInt(ammoPointer);
//                 float health = mem.ReadFloat(healthPointer);
//                 return ammo != 0 || health != 0f;
//             }
//             catch
//             {
//                 // Likely invalid pointer format or unreadable memory
//                 return false;
//             }
//         }

//         public static void WriteLog(string message)
//         {
//             try
//             {
//                 string currentDir = AppDomain.CurrentDomain.BaseDirectory;
//                 string root = Directory.GetParent(currentDir)?.Parent?.FullName ?? currentDir;
//                 string logPath = Path.Combine(root, "DualSenseTR.log");

//                 if (!Directory.Exists(root))
//                 {
//                     Console.WriteLine("Log directory not found.");
//                     return;
//                 }

//                 File.AppendAllText(logPath, message + Environment.NewLine);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Failed to write log: {ex.Message}");
//                 Console.WriteLine("Press any key to exit...");
//                 Console.ReadKey();
//                 Environment.Exit(1);
//             }
//         }

//         static void ControllerLogic()
//         {
//             Connect();

//             // Process[] gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");
//             gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");

//             Console.WriteLine("Monitoring game process...\n");

//             Mem mem = new();

//             // Open the game process
//             mem.OpenProcess("Kena-Win64-Shipping"); // Replace with your game's executable name (no ".exe")

//             // Get the base address from game process
//             // IntPtr baseAddress = mem.GetModuleBase("Kena-Win64-Shipping.exe");

//             // Pointer paths (replace with your actual addresses from Cheat Engine)
//             string ammoPointer = "Kena-Win64-Shipping.exe+060D4AC0,30,128,40,390,20,50,4B0";   // Example: base+offset1,offset2
//             // IntPtr ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
//             // IntPtr ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
//             string healthPointer = "Kena-Win64-Shipping.exe+060ECC20,F0,20,A8,CF0,508,110,10"; // Another example for float health
//             // IntPtr healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
//             // IntPtr healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);

//             var dualsense = WujLibPad.Dualsense_Create(); // Find first available controller

//             Stopwatch flashTimer = new();
//             flashTimer.Start();
//             string currentState = "";
//             string previousState = "";

//             // bool flashing = false;
//             // bool redOn = false;

//             Packet p = new()
//             {
//                 // Set how many instructions you want to send at one time
//                 instructions = new Instruction[7]
//             };

//             int controllerIndex = 0;

//             while (!IsValidMemory(ammoPointer, healthPointer, mem))
//             {
//                 gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");

//                 if (gameProcessName.Length == 0)
//                 {
//                     Console.WriteLine("Game not found. Exiting...\n");
//                     Environment.Exit(1); // Stop the mod or script from continuing
//                 }

//                 p.instructions[4].type = InstructionType.RGBUpdate;
//                 p.instructions[4].parameters = [controllerIndex, 0, 0, 0]; // Off // Black

//                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];

//                 Send(p);

//                 Thread.Sleep(1000);
//             }
//             try
//             {
//                 while (gameProcessName.Length != 0)
//                 {
//                     gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");

//                     // Read Ammo (int)
//                     // ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
//                     // ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
//                     int ammo = mem.ReadInt(ammoPointer);
//                     // Console.WriteLine("Ammo: " + ammo);

//                     // Ammo status check
//                     // if (ammo < 1)
//                     //     Console.WriteLine("No ammo");

//                     // Read Health (float)
//                     // healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
//                     // healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);
//                     float health = mem.ReadFloat(healthPointer);
//                     // Console.WriteLine("Health: " + health);

//                     WujLibPad.Dualsense_Read(dualsense);
//                     WujLibPad.ButtonState btnState = new();
//                     WujLibPad.Dualsense_GetButtonState(dualsense, ref btnState);

//                     // Health status check
//                     // if (health < 1f)
//                     //     Console.WriteLine("Danger");
//                     // else
//                     //     Console.WriteLine("Good");

//                     p.instructions[0].type = InstructionType.TriggerThreshold;
//                     p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

//                     p.instructions[2].type = InstructionType.TriggerUpdate;
//                     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
//                     if (health > 0)
//                     {
//                         if (btnState.L2Btn)
//                         {
//                             p.instructions[1].type = InstructionType.TriggerThreshold;
//                             p.instructions[1].parameters = [controllerIndex, Trigger.Right, 220];
//                             // if (ammo > 0)
//                             // {
//                             //     // Bow effect
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 155, 132, 30, 0, 0, 0, 0]; // strongest
//                             // }
//                             // else
//                             // {
//                             //     // Normal
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             // }
//                             // Bow effect
//                             p.instructions[3].type = InstructionType.TriggerUpdate;
//                             p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 155, 132, 30, 0, 0, 0, 0]; // strongest
//                         }
//                         else
//                         {
//                             p.instructions[1].type = InstructionType.TriggerThreshold;
//                             p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                             // Melee
//                             p.instructions[3].type = InstructionType.TriggerUpdate;
//                             p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 155, 130, 20, 0, 0, 0, 0]; // strongest
//                         }
//                     }
//                     else
//                     {
//                         p.instructions[1].type = InstructionType.TriggerThreshold;
//                         p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                         p.instructions[3].type = InstructionType.TriggerUpdate;
//                         p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                     }

//                     if (health == 0)
//                     {
//                         currentState = "dead";
//                     }
//                     else if (health < 1f)
//                     {
//                         currentState = "danger";
//                     }
//                     else
//                     {
//                         currentState = "good";
//                     }

//                     // Restart timer on state change
//                     if (currentState != previousState)
//                     {
//                         flashTimer.Restart();
//                         previousState = currentState;
//                     }

//                     // Touchpad LED
//                     if (health == 0)
//                     {
//                         // Stop flashing and set steady color
//                         // flashing = false;

//                         p.instructions[4].type = InstructionType.RGBUpdate;
//                         p.instructions[4].parameters = [controllerIndex, 0, 0, 0]; // Off // Black
//                     }

//                     else if (health < 1f)
//                     {
//                         // Start breathing
//                         double elapsed = flashTimer.ElapsedMilliseconds;
//                         double period = 1000; // Full breathing cycle: 2 seconds // Fast breathing
//                                               //   double period = 2000; // Full breathing cycle: 2 seconds // Normal breathing
//                                               // double period = 4000; // Full breathing cycle: 2 seconds // Slow breathing

//                         double phase = (elapsed % period) / period; // Range: 0 to <1

//                         // Sine wave for smooth transition between 0 and 1
//                         double t = (Math.Sin(phase * 2 * Math.PI) + 1) / 2;

//                         // Interpolate between red (60,0,0) and white (100,100,100)
//                         int r = (int)(60 + t * (100 - 60));
//                         int g = (int)(0 + t * (100 - 0));
//                         int b = (int)(0 + t * (100 - 0));

//                         p.instructions[4].type = InstructionType.RGBUpdate;
//                         p.instructions[4].parameters = [controllerIndex, r, g, b];
//                     }

//                     else
//                     {
//                         // Stop breathing and set steady color for good health
//                         p.instructions[4].type = InstructionType.RGBUpdate;
//                         p.instructions[4].parameters = [controllerIndex, 5, 52, 255];
//                     }


//                     p.instructions[5].type = InstructionType.PlayerLED;
//                     p.instructions[5].parameters = [controllerIndex, false, false, true, false, false];

//                     // Console.WriteLine($"DualSense connected via {connectionType}");
//                     // Send UDP commands to DSX
//                     Console.WriteLine("Instructions Sent\n");
//                     Send(p);

//                     // Wait 100ms before sending the next instruction
//                     Thread.Sleep(100);

//                     Console.WriteLine("Waiting for Server Response...\n");

//                     // Make sure you setup some timeout for server response incase DSX has a bug or not running
//                     Process[] process = Process.GetProcessesByName("DSX");

//                     if (process.Length == 0)
//                     {
//                         Console.WriteLine("DSX is not running... \n");
//                     }
//                     else
//                     {
//                         try
//                         {
//                             // IPEndPoint remoteEP = new(IPAddress.Any, 0);
//                             byte[] bytesReceivedFromServer = client!.Receive(ref endPoint);

//                             if (bytesReceivedFromServer.Length > 0)
//                             {
//                                 ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}")!;
//                                 Console.WriteLine("===================================================================");

//                                 Console.WriteLine($"Status: {ServerResponseJson!.Status}");
//                                 DateTime CurrentTime = DateTime.Now;
//                                 TimeSpan Timespan = CurrentTime - TimeSent;
//                                 // First send shows high Milliseconds response time for some reason
//                                 Console.WriteLine($"Time Received: {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
//                                 Console.WriteLine($"isControllerConnected: {ServerResponseJson.isControllerConnected}");
//                                 Console.WriteLine($"BatteryLevel: {ServerResponseJson.BatteryLevel}");

//                                 Console.WriteLine("===================================================================\n");
//                             }
//                         }
//                         catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
//                         {
//                             Console.WriteLine("DSX disconnected or not running (Socket 10054). Retrying...");
//                         }
//                         catch (Exception ex)
//                         {
//                             Console.WriteLine($"Unexpected error communicating with DSX: {ex.Message}");
//                             string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                             string errorMessage = $"[{timestamp}] Unexpected error communicating with DSX: {ex.Message}\n{ex.StackTrace}";
//                             WriteLog(errorMessage);
//                         }
//                     }
//                 }
//                 Environment.Exit(1);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("\n==================================================");
//                 Console.WriteLine("A fatal error occurred:");
//                 Console.WriteLine(ex.ToString());
//                 string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                 string errorMessage = $"[{timestamp}] Unexpected error: [Loop Crash] {ex.Message}\n{ex.StackTrace}";
//                 WriteLog(errorMessage);
//                 Console.WriteLine("==================================================");
//                 Console.WriteLine("Press any key to exit...");
//                 Console.ReadKey();
//                 Environment.Exit(1);
//             }
//         }

//         static void Main(string[] args)
//         {
//             try
//             {
//                 if (gameProcessName.Length == 0)
//                 {
//                     Console.WriteLine("Game not found. Waiting...\n");
//                 }
//                 while (gameProcessName.Length == 0)
//                 {
//                     // Check if the game process is running
//                     gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping");

//                     // If the game process is not found, wait for a while and check again
//                     if (gameProcessName.Length == 0)
//                     {
//                         Thread.Sleep(1000); // Wait for 1 second before checking again
//                     }
//                 }
//                 ControllerLogic();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("\n==================================================");
//                 Console.WriteLine("A fatal error occurred:");
//                 Console.WriteLine(ex.ToString());
//                 string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                 string errorMessage = $"[{timestamp}] Unexpected error: [Loop Crash] {ex.Message}\n{ex.StackTrace}";
//                 WriteLog(errorMessage);
//                 Console.WriteLine("==================================================");
//                 Console.WriteLine("Press any key to exit...");
//                 Console.ReadKey();
//                 Environment.Exit(1);
//             }
//         }
//     }
// }