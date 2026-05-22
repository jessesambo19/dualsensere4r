// using Newtonsoft.Json;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// using System.Security.Cryptography;
// using System.Text;
// using Memory;
// using HidSharp;

// namespace DualSenseRE4R
// {
//     internal class Program
//     {
//         static UdpClient? client;
//         static IPEndPoint? endPoint;

//         static DateTime TimeSent;

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

//         static bool IsValidMemory(string ammoPointer, string healthPointer, Memory.Mem mem)
//         {
//             try
//             {
//                 int ammo = mem.ReadInt(ammoPointer);
//                 float health = mem.ReadFloat(healthPointer);
//                 return ammo != 0 || health != 0f;
//             }
//             catch
//             {
//                 // Likely invalid pointer format or unreadable memory
//                 return false;
//             }
//         }

//         public static (bool IsConnected, string? ConnectionType) GetDualSenseStatus()
//         {
//             var devices = DeviceList.Local.GetHidDevices();
//             foreach (var device in devices)
//             {
//                 if (device.VendorID == 0x054C)
//                 {
//                     if (device.ProductID == 0x0CE6)
//                         return (true, "USB");
//                     else if (device.ProductID == 0x0DF2)
//                         return (true, "Bluetooth");
//                 }
//             }
//             return (false, null);
//         }

//         static void Main(string[] args)
//         {
//             Connect();

//             Process[] gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");

//             Console.WriteLine("Monitoring game process...\n");

//             Mem mem = new();

//             // Open the game process
//             mem.OpenProcess("Kena-Win64-Shipping_original"); // Replace with your game's executable name (no ".exe")

//             // Pointer paths (replace with your actual addresses from Cheat Engine)
//             string ammoPointer = "Kena-Win64-Shipping_original.exe+060D4AC0,30,128,40,390,20,50,4B0";   // Example: base+offset1,offset2
//             string healthPointer = "Kena-Win64-Shipping_original.exe+060ECC20,F0,20,A8,CF0,508,110,10"; // Another example for float health

//             var dualsense = WujLibPad.Dualsense_Create(); // Find first available controller

//             int previousR2 = 0; // this keeps track of the threshold of the previous R2 pull
//             bool previousR1 = false;


//             Stopwatch flashTimer = new();
//             flashTimer.Start();

//             // bool flashing = false;
//             // bool redOn = false;

//             int previousAmmo = 0; // this keeps track of the previous ammo count
//             int previousBowAmmo = -1;

//             Packet p = new()
//             {
//                 // Set how many instructions you want to send at one time
//                 instructions = new Instruction[7]
//             };

//             int controllerIndex = 0;

//             // this will forever run triggering the different trigger modes to make it adaptive
//             // while (true)

//             var (isConnected, connectionType) = GetDualSenseStatus();

//             while (!isConnected)
//             {
//                 Console.WriteLine("No DualSense controller connected.");
//                 (isConnected, connectionType) = GetDualSenseStatus();
//                 Thread.Sleep(1000);
//             }

//             while (gameProcessName.Length == 0)
//             {
//                 // Check if the game process is running
//                 gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");

//                 // If the game process is not found, wait for a while and check again
//                 if (gameProcessName.Length == 0)
//                 {
//                     Console.WriteLine("Game not found. Waiting...\n");
//                     Thread.Sleep(1000); // Wait for 1 second before checking again
//                 }
//             }

//             while (!IsValidMemory(ammoPointer, healthPointer, mem))
//             {
//                 p.instructions[4].type = InstructionType.RGBUpdate;
//                 p.instructions[4].parameters = [controllerIndex, 0, 0, 0]; // Off // Black

//                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];

//                 Send(p);

//                 Console.WriteLine("Ammo or health values are invalid or zero. Waiting...\n");
//                 Thread.Sleep(1000);
//             }

//             while (gameProcessName.Length != 0)
//             {
//                 gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");

//                 // var (isConnected, connectionType) = GetDualSenseStatus();

//                 // while (!isConnected)
//                 // {
//                 //     Console.WriteLine("No DualSense controller connected.");
//                 //     (isConnected, connectionType) = GetDualSenseStatus();
//                 //     Thread.Sleep(1000);
//                 // }

//                 // if (WujLibPad.Dualsense_IsConnected(controllerIndex))
//                 if (isConnected)
//                 {

//                     // Read Ammo (int)
//                     int ammo = mem.ReadInt(ammoPointer);
//                     Console.WriteLine("Ammo: " + ammo);

//                     // Ammo status check
//                     if (ammo < 1)
//                         Console.WriteLine("No ammo");

//                     // Read Health (float)
//                     float health = mem.ReadFloat(healthPointer);
//                     Console.WriteLine("Health: " + health);

//                     // Health status check
//                     if (health < 1f)
//                         Console.WriteLine("Danger");
//                     else
//                         Console.WriteLine("Good");

//                     // This is all you need to use this library with UDP so u can get buttonstates without interrupting DSY
//                     WujLibPad.Dualsense_Read(dualsense);
//                     WujLibPad.ButtonState btnState = new();
//                     WujLibPad.Dualsense_GetButtonState(dualsense, ref btnState);

//                     // Packet p = new();

//                     // int controllerIndex = 0;

//                     // Set how many instructions you want to send at one time
//                     // p.instructions = new Instruction[7];

//                     // TriggerThreshold needs 2 params LeftTrigger:0-255 RightTrigger:0-255
//                     // This is used for telling the emulation when to send the "pressed state"

//                     p.instructions[0].type = InstructionType.TriggerThreshold;
//                     // p.instructions[0].parameters = [controllerIndex, Trigger.Left, 140];
//                     p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

//                     // ----------------------------------------------------------------------------------------------------------------------------

//                     // Adaptive Triggers:

//                     // *NOTE* If you're gonna be applying Adaptive triggers for both R2 And L2, you should send 2 UDP messages, one for L2 and one for R2

//                     // Inside your update loop:
//                     int currentR2 = btnState.R2; // assuming this gives you a 0–255 analog value
//                     bool droppedFromFullPress = false;
//                     // bool currentR1 = false;
//                     bool droppedFromR1FullPress = false;
//                     int currentAmmo = ammo;


//                     if (btnState.circle || btnState.cross || !btnState.L2Btn)
//                     {
//                         // droppedFromFullPress = false;
//                         previousR2 = 0;
//                     }

//                     if (btnState.L2Btn)
//                     {
//                         // figure out a way to delay bow adaptive triggers when reloading from 0 ammo ammount
//                         // Stopwatch reloadTimer = new();
//                         // bool isReloading = false;

//                         // if (ammo == 0)
//                         // {
//                         //     reloadTimer.Start();
//                         //     isReloading = true;

//                         //     while (reloadTimer.ElapsedMilliseconds < 2000)
//                         //     {
//                         //         Console.WriteLine("Reloading... " + (2000 - reloadTimer.ElapsedMilliseconds) + "ms left");
//                         //     }
//                         // }

//                         // if (reloadTimer.ElapsedMilliseconds > 2000)
//                         // {
//                         //     reloadTimer.Stop();
//                         // }


//                         if (ammo == 1)
//                         {
//                             previousAmmo = ammo;
//                         }
//                         // Ammo status check
//                         if (ammo > 0 || previousAmmo == 1
//                         // && !isReloading
//                         )
//                         {
//                             if (btnState.circle || btnState.cross)
//                             {
//                                 previousR2 = 0;
//                             }
//                             // else if (btnState.L2 > 140 && currentR2 > 170)
//                             else if (btnState.L2 > 140 && currentR2 > 160)
//                             {
//                                 previousR2 = currentR2; // gets the current threshold e.g anything above 170
//                                 Console.WriteLine("Hi");
//                             }
//                             // droppedFromFullPress = previousR2 > 170 && currentR2 < 170; // it checks is L2 is pressed to 140 threshold and checks if the r2 was already
//                             // pulled to a number greater than 170 and it was let go in order to be less than 170
//                             droppedFromFullPress = previousR2 > 160 && currentR2 < 160;

//                             bool currentR1 = btnState.R1;

//                             // Detect falling edge: previously pressed, now released
//                             droppedFromR1FullPress = previousR1 && !currentR1;

//                             // Update previous state for next check
//                             previousR1 = currentR1;

//                             p.instructions[1].type = InstructionType.TriggerThreshold;
//                             p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
//                             // p.instructions[1].parameters = [controllerIndex, Trigger.Right, 155]; // hzd

//                             // Bow effect
//                             // p.instructions[3].type = InstructionType.TriggerUpdate;
//                             // // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 6, 3, 8];
//                             // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 5, 5, 8];
//                             // // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 8, 3, 8];

//                             p.instructions[3].type = InstructionType.TriggerUpdate;
//                             p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 40, 0, 0, 0, 0, 0]; // was 50 // 40
//                                                                                                                                                                                  // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 155, 175, 200, 0, 0, 0, 0];

//                             // if (btnState.R2 > 170)
//                             if (btnState.R2 > 160)
//                             {
//                                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 20, 80, 0, 0, 0]; // default
//                                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 30, 80, 0, 0, 0];
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 50, 80, 0, 0, 0];
//                             }

//                         }
//                         else
//                         {
//                             p.instructions[3].type = InstructionType.TriggerUpdate;
//                             p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             // previousR2 = 0;
//                         }

//                     }
//                     else
//                     {
//                         p.instructions[1].type = InstructionType.TriggerThreshold;
//                         p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

//                         // when not aiming with L2 then a resistance is applied on R2 for melee
//                         p.instructions[3].type = InstructionType.TriggerUpdate;
//                         p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 40, 0, 0, 0, 0, 0]; // was 50 
//                                                                                                                                                                              //  p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 150, 175, 200, 0, 0, 0, 0 }; // third parameter controls the strength of the resistance
//                     }


//                     // if (btnState.L2 > 140 && btnState.R2 > 160)
//                     // if (btnState.L2 > 140 && droppedFromFullPress) //hzd
//                     if (
//                      // droppedFromFullPress
//                      btnState.L2 > 140 && previousBowAmmo != -1 && currentAmmo < previousBowAmmo
//                     ) //hzd

//                     {
//                         p.instructions[2].type = InstructionType.TriggerUpdate;
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 20, 80, 0, 0, 0];
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 40, 0, 0, 0, 0, 0]; // was 50
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 8, 8, 8];
//                         p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseB, 1, 190, 100, 0, 0, 0, 0];
//                         previousR2 = 0;
//                         previousAmmo = 0;
//                     }
//                     else if (btnState.L2 > 140 && droppedFromR1FullPress && btnState.R1)
//                     {
//                         p.instructions[2].type = InstructionType.TriggerUpdate;
//                         p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibrateResistance, 60, 10, 140, 0, 0, 0, 0];
//                     }
//                     else
//                     {
//                         // Alternatively: Aiming
//                         p.instructions[2].type = InstructionType.TriggerUpdate;
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 160, 30, 0, 0, 0, 0, 0]; // third parameter controls the strength of the resistance
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 5, 5, 8];
//                         // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 160, 50, 0, 0, 0, 0, 0];
//                         p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                     }

//                     previousBowAmmo = currentAmmo;


//                     // Touchpad LED

//                     // Health status check
//                     // if (health < 1f)
//                     // {
//                     //     p.instructions[4].type = InstructionType.RGBUpdate;
//                     //     // p.instructions[4].parameters = [controllerIndex, 255, 0, 0]; // Red
//                     //     // p.instructions[4].parameters = [controllerIndex, 240, 0, 0]; // Dimmer Red
//                     //     p.instructions[4].parameters = [controllerIndex, 60, 0, 0]; // Very Dim Red
//                     //     // p.instructions[4].parameters = [controllerIndex, 180, 0, 0]; // Soft Red
//                     // }

//                     // else
//                     // {
//                     //     p.instructions[4].type = InstructionType.RGBUpdate;
//                     //     p.instructions[4].parameters = [controllerIndex, 5, 52, 255]; // blue
//                     // }

//                     if (health == 0)
//                     {
//                         // Stop flashing and set steady color
//                         // flashing = false;

//                         p.instructions[4].type = InstructionType.RGBUpdate;
//                         p.instructions[4].parameters = [controllerIndex, 0, 0, 0]; // Off // Black
//                     }

//                     // else if (health < 1f)
//                     // {
//                     //     // Start flashing
//                     //     if (!flashing)
//                     //     {
//                     //         flashing = true;
//                     //         flashTimer.Restart();
//                     //     }

//                     //     // if (flashTimer.ElapsedMilliseconds >= 250) // fast // quarter second
//                     //     if (flashTimer.ElapsedMilliseconds >= 500) // normal // half second
//                     //     // if (flashTimer.ElapsedMilliseconds >= 1000) // slow // 1 second

//                     //     {
//                     //         redOn = !redOn;
//                     //         flashTimer.Restart();
//                     //     }

//                     //     p.instructions[4].type = InstructionType.RGBUpdate;
//                     //     p.instructions[4].parameters = redOn
//                     //         // ? [controllerIndex, 255, 0, 0]    // Red
//                     //         // ? [controllerIndex, 240, 0, 0]    // Dimmer Red
//                     //         ? [controllerIndex, 60, 0, 0]    // Very Dim Red
//                     //         // ? [controllerIndex, 180, 0, 0]    // Soft Red
//                     //         // : [controllerIndex, 0, 0, 0];     // Off // Black
//                     //         // : [controllerIndex, 255, 255, 255];     // Off // White
//                     //         : [controllerIndex, 100, 100, 100];     // Off // Dimmer White
//                     //                                                 // : [controllerIndex, 57, 57, 57];     // Off // Very Dim White
//                     // }
//                     // else
//                     // {
//                     //     // Stop flashing and set steady color
//                     //     flashing = false;

//                     //     p.instructions[4].type = InstructionType.RGBUpdate;
//                     //     p.instructions[4].parameters = [controllerIndex, 5, 52, 255];
//                     // }

//                     // // Add a small CPU-friendly sleep to avoid maxing out CPU
//                     // Thread.Sleep(10);

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
//                         // Stop flashing and set steady color
//                         // flashing = false;

//                         p.instructions[4].type = InstructionType.RGBUpdate;
//                         p.instructions[4].parameters = [controllerIndex, 5, 52, 255];
//                     }


//                     p.instructions[5].type = InstructionType.PlayerLED;
//                     p.instructions[5].parameters = [controllerIndex, false, false, true, false, false];

//                     // var (isConnected, connectionType) = GetDualSenseStatus();
//                     // if (isConnected)
//                     // {
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
//                         }
//                     }

//                     Console.WriteLine("Press any key to send again\n");
//                 }
//                 else
//                 {
//                     Console.WriteLine("No DualSense controller connected.");
//                 }
//             }

//             Console.WriteLine("Game closed. Terminating adaptive triggers...");
//         }
//     }
// }