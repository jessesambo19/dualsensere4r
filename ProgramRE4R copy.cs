// using Newtonsoft.Json;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// // using System.Security.Cryptography;
// using System.Text;
// using Memory;
// // using Swed64;
// // using HidSharp;

// namespace DualSenseRE4R
// {
//     internal class Program
//     {
//         static UdpClient? client;
//         static IPEndPoint? endPoint;

//         static DateTime TimeSent;

//         static Process[] gameProcessName = Process.GetProcessesByName("re4");

//         static FileVersionInfo? versionInfo;

//         static string? fileVersion;

//         static string platform = "";


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

//         /// <summary>
//         /// Returns DSX lightbar / player LED control to the active profile (and game emulation).
//         /// Call once after loading; do not send RGB or PlayerLED instructions in the gameplay loop.
//         /// </summary>
//         static void RestoreProfileLedControl(int controllerIndex)
//         {
//             Send(new Packet
//             {
//                 instructions = new[]
//                 {
//                     new Instruction
//                     {
//                         type = InstructionType.ResetToUserSettings,
//                         parameters = new object[] { controllerIndex }
//                     }
//                 }
//             });
//         }

//         static void CheckGameProcess()
//         {
//             if (gameProcessName.Length == 0)
//             {
//                 Console.WriteLine("TombRaider.exe not found. Waiting for the game to start...\n");
//             }

//             while (gameProcessName.Length == 0)
//             {
//                 // Check if the game process is running
//                 gameProcessName = Process.GetProcessesByName("TombRaider");

//                 // If the game process is not found, wait for a while and check again
//                 if (gameProcessName.Length == 0)
//                 {
//                     Thread.Sleep(1000); // Wait for 1 second before checking again
//                 }
//             }

//             Console.WriteLine("========================================");
//             Console.WriteLine(" Tomb Raider DualSense Mod v1.8.3 by Jexar");
//             Console.WriteLine(" Enhancing DualSense support for Tomb Raider");
//             Console.WriteLine("========================================\n");

//             string? exePath = gameProcessName[0].MainModule!.FileName;
//             versionInfo = FileVersionInfo.GetVersionInfo(exePath);
//             fileVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

//             Console.WriteLine($"Game Name: {gameProcessName[0].ProcessName}");
//             Console.WriteLine($"Product Version: {versionInfo.ProductVersion}");
//             // Console.WriteLine($"File Version: {versionInfo.FileVersion}\n");
//             Console.WriteLine($"File Version: {fileVersion}\n");

//             string exeDirectory = Path.GetDirectoryName(exePath)!;

//             var gogInfoFile = Directory.GetFiles(exeDirectory, "goggame-*.info");
//             var gogFiles = Directory.GetFiles(exeDirectory, "goggame-*.*");

//             if (File.Exists(Path.Combine(exeDirectory, "steam_api.dll")))
//             {
//                 platform = "Steam";
//             }
//             else if (File.Exists(Path.Combine(exeDirectory, "EOSSDK-Win32-Shipping.dll"))
//                      && !File.Exists(Path.Combine(exeDirectory, "steam_api.dll"))
//                      && !File.Exists(Path.Combine(exeDirectory, "Galaxy.dll"))
//                      && gogInfoFile.Length == 0
//                      && gogFiles.Length == 0
//                      && !File.Exists(Path.Combine(exeDirectory, "xgameruntime.dll"))
//                      && !File.Exists(Path.Combine(exeDirectory, "MicrosoftGame.Config"))
//                      && !File.Exists(Path.Combine(exeDirectory, "appxmanifest.xml")))
//             {
//                 platform = "Epic Games Store";
//             }
//             else if (File.Exists(Path.Combine(exeDirectory, "Galaxy.dll")) || gogInfoFile.Length > 0 || gogFiles.Length > 0)
//             {
//                 platform = "GOG";
//             }
//             else if (File.Exists(Path.Combine(exeDirectory, "xgameruntime.dll"))
//                      || File.Exists(Path.Combine(exeDirectory, "MicrosoftGame.Config"))
//                      || File.Exists(Path.Combine(exeDirectory, "appxmanifest.xml")))
//             {
//                 platform = "Microsoft Store";
//             }
//             else
//             {
//                 platform = "Unknown";
//             }

//             Console.WriteLine($"Platform: {platform}\n");
//         }

//         static bool IsValidMemory(string healthPointer, string hasKnifePointer, string aimStatePointer1, string aimStatePointer2, string pauseStatesPointer, string weaponTypePointer, Mem mem)
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
//                 //  int ammo = mem.ReadInt(ammoPointer);
//                 int health = mem.ReadInt(healthPointer);
//                 // Console.WriteLine("Health: " + health);
//                 return health != 0;
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

//             // Process[] gameProcessName = Process.GetProcessesByName("re4");
//             gameProcessName = Process.GetProcessesByName("re4");

//             Console.WriteLine("Monitoring game process...\n");

//             Mem mem = new();

//             // Open the game process
//             mem.OpenProcess("re4"); // Replace with your game's executable name (no ".exe")

//             // var swed = new Swed("re4");
//             // var module = swed.GetModuleBase("re4.exe");
//             // var address = module + 0x33F539A;

//             // var code = swed.ReadBytes(address, 8);
//             // Console.WriteLine(BitConverter.ToString(code));

//             //             "re4.exe"+33F539A:
//             // db 41 89 49 14 41 80 79 1A 00

//             // read memory (example API may vary)
//             // var value = swed.ReadInt(address);



//             // Get the base address from game process
//             // IntPtr baseAddress = mem.GetModuleBase("re4.exe");

//             // Pointer paths (replace with your actual addresses from Cheat Engine)
//             // IntPtr ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
//             // IntPtr ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
//             string healthPointer = "re4.exe+0DA93C18,38,78,A8,40,168,10,A4"; // Another example for float health
//             string hasKnifePointer = "re4.exe+0DA8CDD0,88,18,68,48,B0,1B8,180"; // Another example for float health
//             string aimState1Pointer = "re4.exe+0DA56208,20,48,70,60,20,10,198"; // Another example for float health
//             string aimState2Pointer = "re4.exe+0DA8D620,B8,B0,168,B0,28,108,78"; // Another example for float health
//             string pauseStatesPointer = "re4.exe+0DA8F0B0,50,48,18,180,40,48,100"; // Another example for float health
//             string weaponTypePointer = "re4.exe+0DA4E4D0,78,20,30,20,20,1A8,50"; // Another example for float health
//             string boatPointer = "re4.exe+0DA8D5D8,60,78,10,C8,148,18,8"; // Another example for float health

//             // IntPtr healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
//             // IntPtr healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);

//             // Triggers only — no RGB / PlayerLED slots (those override DSX health LEDs).
//             Packet p = new()
//             {
//                 instructions = new Instruction[4]
//             };

//             int controllerIndex = 0;

//             while (!IsValidMemory(healthPointer, hasKnifePointer, aimState1Pointer, aimState2Pointer, pauseStatesPointer, weaponTypePointer, mem))
//             {
//                 gameProcessName = Process.GetProcessesByName("re4");

//                 if (gameProcessName.Length == 0)
//                 {
//                     Console.WriteLine("Game not found. Exiting...\n");
//                     Environment.Exit(1); // Stop the mod or script from continuing
//                 }

//                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];

//                 Send(p);

//                 Thread.Sleep(1000);
//             }

//             Console.WriteLine("Game memory ready — restoring lightbar to DSX profile / game emulation.\n");
//             RestoreProfileLedControl(controllerIndex);
//             Thread.Sleep(100);

//             try
//             {
//                 while (gameProcessName.Length != 0)
//                 {
//                     gameProcessName = Process.GetProcessesByName("re4");

//                     try
//                     {
//                         // Read Ammo (int)
//                         // ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
//                         // ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
//                         // int ammo = mem.ReadInt(ammoPointer);
//                         // Console.WriteLine("Ammo: " + ammo);

//                         // Ammo status check
//                         // if (ammo < 1)
//                         //     Console.WriteLine("No ammo");

//                         // Read Health (float)
//                         // healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
//                         // healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);
//                         int health = mem.ReadInt(healthPointer);
//                         Console.WriteLine("Health: " + health);
//                         int hasKnife = mem.ReadInt(hasKnifePointer);
//                         int aimState1 = mem.ReadInt(aimState1Pointer);
//                         int aimState2 = mem.ReadInt(aimState2Pointer);
//                         int pauseStates = mem.ReadInt(pauseStatesPointer);
//                         int weaponType = mem.Read2Byte(weaponTypePointer);
//                         int boat = mem.ReadInt(boatPointer);
//                         Console.WriteLine("Health: " + health);
//                         Console.WriteLine("Has Knife: " + hasKnife);
//                         Console.WriteLine("Aim State 1: " + aimState1);
//                         Console.WriteLine("Aim State 2: " + aimState2);
//                         Console.WriteLine("Pause States: " + pauseStates);
//                         Console.WriteLine("Weapon Type: " + weaponType);


//                         // Console.WriteLine($"Swed read value: {value} from address: {address:X}\n");
//                         // Console.WriteLine(BitConverter.ToString(code));



//                         // Health status check
//                         // if (health < 1f)
//                         //     Console.WriteLine("Danger");
//                         // else
//                         //     Console.WriteLine("Good");


//                         if (health == 0 || pauseStates > 0) // aimState == 1 for cutscenes, pauseStates == 0 for pause and map menus
//                         {

//                             // if (
//                             // // weapon_type == "" &&
//                             // weaponType == 0) // Off
//                             // {
//                             //     p.instructions[4].type = InstructionType.RGBUpdate;
//                             //     p.instructions[4].parameters = [controllerIndex, 0, 0, 0];

//                             //     // resets when Lara doesn't have a weapon during loading screens, menus, cutscenes or during gameplay
//                             //     previousBowAmmo = -1;
//                             //     previousBowAmmo2 = -1;
//                             //     previousBackupBowAmmo = -1;
//                             //     previousBackupBowAmmo2 = -1;
//                             //     previousHandgunAmmo = -1;
//                             //     previousHandgunAmmo2 = -1;
//                             //     previousMachinegunAmmo = -1;
//                             //     previousMachinegunAmmo2 = -1;
//                             //     previousGrenadeAmmo = -1;
//                             //     previousShotgunAmmo = -1;
//                             //     previousShotgunAmmo2 = -1;
//                             //     previousShootArrow = -1;
//                             //     previousShootHandgun = -1;
//                             //     previousShootGrenade = -1;
//                             //     previousShootShotgun = -1;

//                             //     previousWeaponType = "";
//                             //     previousWeaponTypeId = 0;
//                             //     previousWeaponTypeId2 = 0;

//                             //     lastMachinegunShotTime = 0;
//                             // }

//                             // Reset left trigger threshold to 0
//                             p.instructions[0].type = InstructionType.TriggerThreshold;
//                             p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

//                             // Reset right trigger threshold to 0
//                             p.instructions[1].type = InstructionType.TriggerThreshold;
//                             p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

//                             // Reset right trigger to off
//                             p.instructions[2].type = InstructionType.TriggerUpdate;
//                             p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                             // if (
//                             //     // weapon_type.Contains("arrow") ||
//                             //     weaponType == 17 && hasKnife == 1 && aimState == 0
//                             //  //  || weaponType == 5273748904 long
//                             //  )
//                             // {
//                             //     // Reset right trigger to off
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             // }
//                             // else {
//                             // Reset right trigger to off
//                             p.instructions[3].type = InstructionType.TriggerUpdate;
//                             p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             // }

//                         }
//                         else
//                         {

//                             // if (
//                             //     // weapon_type.Contains("arrow") ||
//                             //     weaponType == 17 && hasKnife == 1 && aimState == 0
//                             //  //  || weaponType == 5273748904 long
//                             //  )
//                             // {
//                             //     // Reset right trigger to off
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             // }                       
//                             if (
//                             // weapon_type.Contains("handgun") ||
//                             boat == 26
//                                                                                                              // || weaponType == 4326952780 long
//                             )
//                             {

//                                 // 256 is for aiming a weapon
//                                 // if (aimState == 256)
//                                 // {
//                                 p.instructions[0].type = InstructionType.TriggerThreshold;
//                                 p.instructions[0].parameters = [controllerIndex, Trigger.Left, 100];
//                                 p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 p.instructions[1].parameters = [controllerIndex, Trigger.Right, 100];
//                                 // }
//                                 // else
//                                 // {
//                                 //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                                 // }


//                                 // Single pistol idle
//                                 // Aiming with one pistol
//                                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                                 // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
//                                 // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
//                                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest


//                                 // if (aimState == 256)
//                                 // {
//                                 // Hand gun or Pistol:
//                                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.FEEDBACK, 0, 5, 4]; // moderate
//                                                                                                                               // }
//                                                                                                                               // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                                                                                                                               // {
//                                                                                                                               //     // Reset right trigger to off
//                                                                                                                               //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                                                                                                               //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                                                                                                                               // }
//                                                                                                                               // else
//                                                                                                                               // {
//                                                                                                                               //     // Reset right trigger to off
//                                                                                                                               //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                                                                                                               //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                                                                                                                               // }
//                             }       
//                             else if (
//                                 // weapon_type.Contains("arrow") ||
//                                 // (hasKnife == 1 && ((aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0))) || weaponType == 143 || weaponType == 15 // not aiming with a knife, or aiming with a knife
//                                 hasKnife == 1 && (aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0) // not aiming with a knife, or aiming with a knife

//                                                                                                                                                                          //  || weaponType == 5273748904 long
//                              )
//                             {

//                                 // 256 is for aiming a weapon
//                                 // if (aimState == 256)
//                                 // {
//                                 p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
//                                 // }
//                                 // else
//                                 // {
//                                 //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                                 // }

//                                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
//                                 // if (aimState == 256)
//                                 // {
//                                 // Bow effect
//                                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30

//                                 // }
//                                 // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                                 // else if (weaponType == 11) 
//                                 // {
//                                 //     // Reset right trigger to off
//                                 //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                                 // }
//                                 // else
//                                 // {
//                                 //     // Reset right trigger to off
//                                 //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                                 // }
//                             }
//                             else if (
//                             // weapon_type.Contains("handgun") ||
//                             aimState1 == 256 && aimState2 == 256 // not aiming with a knife
//                                                                                                              // || weaponType == 4326952780 long
//                             )
//                             {

//                                 // 256 is for aiming a weapon
//                                 // if (aimState == 256)
//                                 // {
//                                 p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
//                                 // }
//                                 // else
//                                 // {
//                                 //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                                 //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                                 // }


//                                 // Single pistol idle
//                                 // Aiming with one pistol
//                                 // p.instructions[2].type = InstructionType.TriggerUpdate;
//                                 // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
//                                 // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
//                                 // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest


//                                 // if (aimState == 256)
//                                 // {
//                                 // Hand gun or Pistol:
//                                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft];
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 148, 32, 0, 0, 0, 0]; // default
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 125, 148, 160, 0, 0, 0, 0]; // moderate
//                                 // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 27, 40, 86, 102, 184, 172, 2 }; // prefered

//                                 // alternative with a gradual resistance
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 158, 40, 0, 0, 0, 0]; // moderate
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 98, 158, 40, 0, 0, 0, 0]; // moderate
//                                 // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 101, 158, 40, 0, 0, 0, 0]; // moderate // or 30
//                                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 4, 6, 4]; // moderate
//                                                                                                                               // }
//                                                                                                                               // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                                                                                                                               // {
//                                                                                                                               //     // Reset right trigger to off
//                                                                                                                               //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                                                                                                               //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                                                                                                                               // }
//                                                                                                                               // else
//                                                                                                                               // {
//                                                                                                                               //     // Reset right trigger to off
//                                                                                                                               //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                                                                                                                               //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                                                                                                                               // }
//                             }

//                             // else if (
//                             // // weapon_type.Contains("handgun") ||
//                             // aimState1 == 256 && aimState2 == 256 && (weaponType == 1 || weaponType == 12289) // not aiming with a knife
//                             //                                                                                  // || weaponType == 4326952780 long
//                             // )
//                             // {

//                             //     // 256 is for aiming a weapon
//                             //     // if (aimState == 256)
//                             //     // {
//                             //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                             //     // }


//                             //     // Single pistol idle
//                             //     // Aiming with one pistol
//                             //     // p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
//                             //     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

//                             //     p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
//                             //     // if (aimState == 256)
//                             //     // {
//                             //     // Hand gun or Pistol:
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 148, 32, 0, 0, 0, 0]; // default
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 125, 148, 160, 0, 0, 0, 0]; // moderate
//                             //     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 27, 40, 86, 102, 184, 172, 2 }; // prefered

//                             //     // alternative with a gradual resistance
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 158, 40, 0, 0, 0, 0]; // moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 98, 158, 40, 0, 0, 0, 0]; // moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 101, 158, 40, 0, 0, 0, 0]; // moderate // or 30
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 4, 6, 4]; // moderate
//                             //                                                                                                   // }
//                             //                                                                                                   // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                             //                                                                                                   // {
//                             //                                                                                                   //     // Reset right trigger to off
//                             //                                                                                                   //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //                                                                                                   //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             //                                                                                                   // }
//                             //                                                                                                   // else
//                             //                                                                                                   // {
//                             //                                                                                                   //     // Reset right trigger to off
//                             //                                                                                                   //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //                                                                                                   //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             //                                                                                                   // }
//                             // }

//                             // else if (
//                             //     // weapon_type.Contains("machinegun") ||
//                             //     aimState1 == 256 && aimState2 == 256 && weaponType == 5
//                             // // || weaponType == 7526949996 // long
//                             // )
//                             // {
//                             //     // 256 is for aiming a weapon
//                             //     // if (aimState == 256)
//                             //     // {
//                             //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                             //     // }

//                             //     // p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 25, 0, 0, 0, 0, 0, 0];
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 25, 0, 30, 0, 0, 0, 0]; // stronger
//                             //     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

//                             //     p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                             //     // if (aimState == 256

//                             //     //     )
//                             //     // {
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 60, 40, 86, 102, 184, 172, 2 }; // machine gun
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 135, 32, 0, 0, 0, 0]; // default
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 93, 135, 55, 0, 0, 0, 0]; // moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 93, 135, 160, 0, 0, 0, 0]; // stronger

//                             //     // alternative with a gradual resistance
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 145, 50, 0, 0, 0, 0]; // moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 70, 145, 50, 0, 0, 0, 0]; // moderate // preferred
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 140, 40, 0, 0, 0, 0];
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 3, 5, 4]; // moderate

//                             //     // }
//                             //     // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             //     // }
//                             // }

//                             // else if (
//                             // // weapon_type.Contains("shotgun") ||
//                             // aimState1 == 256 && aimState2 == 256 && weaponType == 66
//                             //  //  || weaponType == 7146889127 long
//                             //  )
//                             // {

//                             //     // 256 is for aiming a weapon
//                             //     // if (aimState == 256)
//                             //     // {
//                             //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                             //     // }

//                             //     // Aiming
//                             //     // p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
//                             //     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

//                             //     p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                             //     // if (aimState == 256)
//                             //     // {
//                             //     // Shotgun or any heavy gun:
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
//                             //     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

//                             //     // alternative with a gradual resistance
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 2, 4, 4]; // moderate

//                             //     // }
//                             //     // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             //     // }
//                             // }
//                             // else if (
//                             // // weapon_type.Contains("shotgun") ||
//                             // aimState1 == 256 && aimState2 == 256 && weaponType == 3
//                             //  //  || weaponType == 7146889127 long
//                             //  )
//                             // {

//                             //     // 256 is for aiming a weapon
//                             //     // if (aimState == 256)
//                             //     // {
//                             //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     p.instructions[1].type = InstructionType.TriggerThreshold;
//                             //     //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
//                             //     // }

//                             //     // Aiming
//                             //     // p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
//                             //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
//                             //     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

//                             //     p.instructions[2].type = InstructionType.TriggerUpdate;
//                             //     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

//                             //     // if (aimState == 256)
//                             //     // {
//                             //     // Shotgun or any heavy gun:
//                             //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
//                             //     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

//                             //     // alternative with a gradual resistance
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
//                             //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VIBRATION, 0, 9, 7, 7, 10, 0]; // moderate
//                             //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VIBRATION, 3, 8, 9]; // moderate


//                             //     // }
//                             //     // else if (hasKnife == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
//                             //     // }
//                             //     // else
//                             //     // {
//                             //     //     // Reset right trigger to off
//                             //     //     p.instructions[3].type = InstructionType.TriggerUpdate;
//                             //     //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             //     // }
//                             // }
//                             else
//                             {
//                                 // Reset right trigger to off
//                                 p.instructions[2].type = InstructionType.TriggerUpdate;
//                                 p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
//                                 p.instructions[3].type = InstructionType.TriggerUpdate;
//                                 p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
//                             }

//                         }
//                     }
//                     catch (OverflowException ex)
//                     {
//                         Console.WriteLine("Memory pointer invalid after save reload: " + ex.Message);
//                         Console.WriteLine("Please restart the mod after loading a save.");
//                         Environment.Exit(1);
//                     }

//                     Console.WriteLine("Instructions Sent\n");
//                     Send(p);

//                     // Wait 100ms before sending the next instruction
//                     Thread.Sleep(100);

//                     Console.WriteLine("Waiting for Server Response...\n");

//                     // Make sure you setup some timeout for server response incase DSX has a bug or not running
//                     Process[] process1 = Process.GetProcessesByName("DSX");
//                     Process[] process2 = Process.GetProcessesByName("DualSenseY");

//                     // Checks if either DSX or DualSenseY is running
//                     if (process1.Length == 0 && process2.Length == 0)
//                     {
//                         Console.WriteLine("DSX is not running... \n");
//                     }
//                     else
//                     {
//                         try
//                         {
//                             byte[] bytesReceivedFromServer = client!.Receive(ref endPoint);

//                             if (bytesReceivedFromServer.Length > 0)
//                             {
//                                 Console.WriteLine("Tomb Raider DualSense Mod Initialized\n");
//                                 ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}")!;
//                                 Console.WriteLine("===================================================================");
//                                 Console.WriteLine($"Status: {ServerResponseJson!.Status}");
//                                 DateTime CurrentTime = DateTime.Now;
//                                 TimeSpan Timespan = CurrentTime - TimeSent;
//                                 Console.WriteLine($"Time Received: {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
//                                 Console.WriteLine($"isControllerConnected: {ServerResponseJson.isControllerConnected}");
//                                 Console.WriteLine($"BatteryLevel: {ServerResponseJson.BatteryLevel}");
//                                 Console.WriteLine("===================================================================\n");
//                             }
//                         }
//                         catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
//                         {
//                             Console.WriteLine("Connection reset by DSX (10054). Retrying...");
//                         }
//                         catch (Exception ex)
//                         {
//                             Console.WriteLine($"Unexpected error communicating with DSX: {ex.Message}");
//                             string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                             string errorMessage = $"[{timestamp}] Unexpected error communicating with DSX: {ex.Message}\n{ex.StackTrace}";
//                             // Functions.WriteLog(errorMessage);
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
//                 CheckGameProcess();
//                 ControllerLogic();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("\n==================================================");
//                 Console.WriteLine("A fatal error occurred:");
//                 Console.WriteLine(ex.ToString());
//                 string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//                 string errorMessage = $"[{timestamp}] Unexpected error: {ex.Message}\n{ex.StackTrace}";
//                 // Functions.WriteLog(errorMessage);

//                 // if (ex is Win32Exception win32Ex && win32Ex.NativeErrorCode == 5)
//                 // {
//                 //     // Access denied specific handling
//                 //     Console.WriteLine("\nAccess denied while trying to communicate with the game. The game may be running as Administrator.");
//                 //     Console.WriteLine("Fix: Run this mod as Administrator and try again.");
//                 // }

//                 Console.WriteLine("==================================================");
//                 Console.WriteLine("Press any key to exit...");
//                 Console.ReadKey();
//                 Environment.Exit(1);
//             }
//         }
//     }
// }