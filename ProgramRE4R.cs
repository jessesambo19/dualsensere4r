using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
// using System.Security.Cryptography;
using System.Text;
using Memory;
using SharpDX.DirectInput;

// using Swed64;
// using HidSharp;

namespace DualSenseRE4R
{
    internal class Program
    {
        static UdpClient? client;
        static IPEndPoint? endPoint;

        static DateTime TimeSent;

        static Process[] gameProcessName = Process.GetProcessesByName("re4");

        static FileVersionInfo? versionInfo;

        static string? fileVersion;

        static string platform = "";


        static void Connect()
        {
            client = new UdpClient();
            var portNumber = File.ReadAllText(@"C:\Temp\DualSenseX\DualSenseX_PortNumber.txt");
            endPoint = new IPEndPoint(Triggers.localhost, Convert.ToInt32(portNumber));
            Console.WriteLine($"Port number found is: {portNumber}\n");
        }

        static void Send(Packet data)
        {
            var RequestData = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
            client!.Send(RequestData, RequestData.Length, endPoint);
            TimeSent = DateTime.Now;
        }

        /// <summary>
        /// Returns DSX lightbar / player LED control to the active profile (and game emulation).
        /// Call once after loading; do not send RGB or PlayerLED instructions in the gameplay loop.
        /// </summary>
        static void RestoreProfileLedControl(int controllerIndex)
        {
            Send(new Packet
            {
                instructions = new[]
                {
                    new Instruction
                    {
                        type = InstructionType.ResetToUserSettings,
                        parameters = new object[] { controllerIndex }
                    }
                }
            });
        }

        static void CheckGameProcess()
        {
            if (gameProcessName.Length == 0)
            {
                Console.WriteLine("re4.exe not found. Waiting for the game to start...\n");
            }

            while (gameProcessName.Length == 0)
            {
                // Check if the game process is running
                gameProcessName = Process.GetProcessesByName("re4");

                // If the game process is not found, wait for a while and check again
                if (gameProcessName.Length == 0)
                {
                    Thread.Sleep(1000); // Wait for 1 second before checking again
                }
            }

            Console.WriteLine("========================================");
            Console.WriteLine(" Resident Evil 4 Remake DualSense Mod v1.0.0 by Jexar");
            Console.WriteLine(" Enhancing DualSense support for Resident Evil 4 Remake");
            Console.WriteLine("========================================\n");

            string? exePath = gameProcessName[0].MainModule!.FileName;
            versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            fileVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

            Console.WriteLine($"Game Name: {gameProcessName[0].ProcessName}");
            Console.WriteLine($"Product Version: {versionInfo.ProductVersion}");
            // Console.WriteLine($"File Version: {versionInfo.FileVersion}\n");
            Console.WriteLine($"File Version: {fileVersion}\n");

            string exeDirectory = Path.GetDirectoryName(exePath)!;

            var gogInfoFile = Directory.GetFiles(exeDirectory, "goggame-*.info");
            var gogFiles = Directory.GetFiles(exeDirectory, "goggame-*.*");

            if (File.Exists(Path.Combine(exeDirectory, "steam_api.dll")))
            {
                platform = "Steam";
            }
            else if (File.Exists(Path.Combine(exeDirectory, "EOSSDK-Win32-Shipping.dll"))
                     && !File.Exists(Path.Combine(exeDirectory, "steam_api.dll"))
                     && !File.Exists(Path.Combine(exeDirectory, "Galaxy.dll"))
                     && gogInfoFile.Length == 0
                     && gogFiles.Length == 0
                     && !File.Exists(Path.Combine(exeDirectory, "xgameruntime.dll"))
                     && !File.Exists(Path.Combine(exeDirectory, "MicrosoftGame.Config"))
                     && !File.Exists(Path.Combine(exeDirectory, "appxmanifest.xml")))
            {
                platform = "Epic Games Store";
            }
            else if (File.Exists(Path.Combine(exeDirectory, "Galaxy.dll")) || gogInfoFile.Length > 0 || gogFiles.Length > 0)
            {
                platform = "GOG";
            }
            else if (File.Exists(Path.Combine(exeDirectory, "xgameruntime.dll"))
                     || File.Exists(Path.Combine(exeDirectory, "MicrosoftGame.Config"))
                     || File.Exists(Path.Combine(exeDirectory, "appxmanifest.xml")))
            {
                platform = "Microsoft Store";
            }
            else
            {
                platform = "Unknown";
            }

            Console.WriteLine($"Platform: {platform}\n");
        }

        static bool IsValidMemory(string healthPointer, string knifeHealthPointer, string aimState1Pointer, string aimState2Pointer, string aimStateOnBoatPointer, string earlyGamePointer, string pauseStatesPointer, string pauseStates2Pointer, string pauseStates3Pointer, string gettingOnABoatPointer, string boatPointer, string weaponTypePointer, string backupAmmoPointer, string grenadeAmmoPointer, string flashbangAmmoPointer, string startMenuAndLoadingScreenPointer, string hasGrenadeOrFlashbangPointer, Mem mem)
        {
            try
            {
                Console.WriteLine("Addresses are invalid or zero. Waiting...\n");

                // IntPtr ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
                // ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
                // IntPtr pointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
                // healthPointer = mem.ResolvePointer(pointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);
                // int ammo = mem.ReadInt(ammoPointer);
                // float health = mem.SafeReadFloat(healthPointer);
                //  int ammo = mem.ReadInt(ammoPointer);
                int health = mem.ReadInt(healthPointer);
                float knifeHealth = mem.ReadFloat(knifeHealthPointer);
                int aimState1 = mem.ReadInt(aimState1Pointer);
                int aimState2 = mem.ReadInt(aimState2Pointer);
                int aimStateOnBoat = mem.ReadInt(aimStateOnBoatPointer);
                int earlyGame = mem.ReadInt(earlyGamePointer);
                int pauseStates1 = mem.ReadInt(pauseStatesPointer);
                int pauseStates2 = mem.ReadInt(pauseStates2Pointer);
                int pauseStates3 = mem.ReadInt(pauseStates3Pointer);
                int weaponType = mem.Read2Byte(weaponTypePointer);
                int boat = mem.ReadInt(boatPointer);
                int gettingOnABoat = mem.ReadInt(gettingOnABoatPointer);
                int backupAmmo = mem.ReadInt(backupAmmoPointer);
                int grenadeAmmo = mem.ReadInt(grenadeAmmoPointer);
                int flashbangAmmo = mem.ReadInt(flashbangAmmoPointer);
                int startMenuAndLoadingScreen = mem.ReadInt(startMenuAndLoadingScreenPointer);
                int hasGrenadeOrFlashbang = mem.ReadInt(hasGrenadeOrFlashbangPointer);
                // Console.WriteLine("Health: " + health);
                // Console.WriteLine("Knife Health: " + knifeHealth);
                return health != 0 
                || knifeHealth != 0 
                || aimState1 != 0 || aimState2 != 0 || aimStateOnBoat != 0 || earlyGame != 0 || pauseStates1 != 0 || pauseStates2 != 0 || pauseStates3 != 0 || weaponType != 0 || boat != 0 || gettingOnABoat != 0 || backupAmmo != 0 || grenadeAmmo != 0 || flashbangAmmo != 0 || startMenuAndLoadingScreen != 0 || hasGrenadeOrFlashbang != 0;
                // return true;
            }
            catch
            {
                // Likely invalid pointer format or unreadable memory
                return false;
            }
        }

        public static void WriteLog(string message)
        {
            try
            {
                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string root = Directory.GetParent(currentDir)?.Parent?.FullName ?? currentDir;
                string logPath = Path.Combine(root, "DualSenseTR.log");

                if (!Directory.Exists(root))
                {
                    Console.WriteLine("Log directory not found.");
                    return;
                }

                File.AppendAllText(logPath, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        static void ControllerLogic()
        {
            Connect();

            // Process[] gameProcessName = Process.GetProcessesByName("re4");
            gameProcessName = Process.GetProcessesByName("re4");

            Console.WriteLine("Monitoring game process...\n");

            Mem mem = new();

            // Open the game process
            mem.OpenProcess("re4"); // Replace with your game's executable name (no ".exe")

            // var swed = new Swed("re4");
            // var module = swed.GetModuleBase("re4.exe");
            // var address = module + 0x33F539A;

            // var code = swed.ReadBytes(address, 8);
            // Console.WriteLine(BitConverter.ToString(code));

            //             "re4.exe"+33F539A:
            // db 41 89 49 14 41 80 79 1A 00

            // read memory (example API may vary)
            // var value = swed.ReadInt(address);



            // Get the base address from game process
            // IntPtr baseAddress = mem.GetModuleBase("re4.exe");

            // Pointer paths (replace with your actual addresses from Cheat Engine)
            // IntPtr ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
            // IntPtr ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
            string healthPointer = "re4.exe+0DA8D590,20,10,138,58,210,30,44"; // Another example for float health
            string knifeHealthPointer = "re4.exe+0DA8D2D0,50,18,28,70,48,218,14"; // Another example for float health
            // string aimState1Pointer = "re4.exe+0DC70CC8,90,30,20,1B0,28,10"; // Another example for float health
            // string aimState1Pointer = "re4.exe+0DA8D590,20,170,C0,98,B0,10,198"; // Another example for float health
            string aimState1Pointer = "re4.exe+0DA96D18,40,20,180,1F8,120,160,8"; // Another example for float health
            string aimState2Pointer = "re4.exe+0DA8D620,B8,B0,168,B0,28,108,78"; // Another example for float health
            // string aimState2Pointer = "re4.exe+0DAA7190,B8,B0,168,B0,18,108,38"; // Another example for float health
            string aimStateOnBoatPointer = "re4.exe+0DA8D620,B8,B0,18,98,B0,50,44"; // look for this
            string earlyGamePointer = "re4.exe+0DA8D560,178,80,30,0,D0,398,FB0"; // look for this
            string pauseStatesPointer = "re4.exe+0DA8F0B0,50,48,18,180,40,48,100"; // Another example for float health
            string pauseStates2Pointer = "re4.exe+0DA4EEB8,98,A8,210,200,230,50,4C"; // Another example for float health
            string pauseStates3Pointer = "re4.exe+0DA4E518,5B0,78,48,4B0,708,38,38"; // Another example for float health
            string weaponTypePointer = "re4.exe+0DC70CC8,90,30,20,1B0,30,B8,50"; // Another example for float health
            string boatPointer = "re4.exe+0DA8D5D8,60,78,10,C8,148,18,8"; // Another example for float health
            string gettingOnABoatPointer = "re4.exe+0DA8F0D8,210,1A8,440,130,60,68,DB8"; // Another example for float health
            string backupAmmoPointer = "re4.exe+0DA8CDD0,88,18,A0,70,B8,80,2C"; // Another example for float health
            string grenadeAmmoPointer = "re4.exe+0DA4E9A8,10,10,318,A0,38,F18,10"; // Another example for float health
            string flashbangAmmoPointer = "re4.exe+0DA62250,A8,68,1D8,70,38,ED0,6A0"; // Another example for float health
            string startMenuAndLoadingScreenPointer = "re4.exe+0DE20BA0,68,40,E0,288,A0,1BC"; // Another example for float health
            string hasGrenadeOrFlashbangPointer = "re4.exe+0DA8D590,C8,70,A8,A0,58,68,C4"; // Another example for float health


            // IntPtr healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
            // IntPtr healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);

            // Triggers only — no RGB / PlayerLED slots (those override DSX health LEDs).
            Packet p = new()
            {
                instructions = new Instruction[6]
            };

            int controllerIndex = 0;

            while (!IsValidMemory(healthPointer, knifeHealthPointer, aimState1Pointer, aimState2Pointer, aimStateOnBoatPointer, earlyGamePointer, pauseStatesPointer, pauseStates2Pointer, pauseStates3Pointer, gettingOnABoatPointer, boatPointer, weaponTypePointer, backupAmmoPointer, grenadeAmmoPointer, flashbangAmmoPointer, startMenuAndLoadingScreenPointer, hasGrenadeOrFlashbangPointer, mem))
            {
                gameProcessName = Process.GetProcessesByName("re4");

                if (gameProcessName.Length == 0)
                {
                    Console.WriteLine("Game not found. Exiting...\n");
                    Environment.Exit(1); // Stop the mod or script from continuing
                }

                p.instructions[2].type = InstructionType.TriggerUpdate;
                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                p.instructions[3].type = InstructionType.TriggerUpdate;
                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];

                Send(p);

                Thread.Sleep(1000);
            }

            Console.WriteLine("Game memory ready — restoring lightbar to DSX profile / game emulation.\n");
            RestoreProfileLedControl(controllerIndex);
            Thread.Sleep(100);

            try
            {
                // Initialize DirectInput
                var directInput = new DirectInput();

                // Find a connected DualSense controller
                var joystickGuid = Guid.Empty;

                foreach (var deviceInstance in directInput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    break;
                }

                if (joystickGuid == Guid.Empty)
                {
                    Console.WriteLine("No controller found. Please connect a DualSense controller.");
                    return;
                }

                // Instantiate the controller
                var joystick = new Joystick(directInput, joystickGuid);
                Console.WriteLine("Controller connected: " + joystick.Information.ProductName);

                // Acquire the device
                joystick.Acquire();

                while (gameProcessName.Length != 0)
                {
                    gameProcessName = Process.GetProcessesByName("re4");

                    string currentPointerName = string.Empty;
                    string currentPointerValue = string.Empty;

                    try
                    {


                        // Poll the controller
                        joystick.Poll();
                        var state = joystick.GetCurrentState();

                        // Get button states
                        var buttons = state.Buttons;
                        // Read Ammo (int)
                        // ammoPointerBase = IntPtr.Add(baseAddress, 0x060D4AC0);
                        // ammoPointer = mem.ResolvePointer(ammoPointerBase, [0x30, 0x128, 0x40, 0x390, 0x20, 0x50, 0x4B0]);
                        // int ammo = mem.ReadInt(ammoPointer);
                        // Console.WriteLine("Ammo: " + ammo);

                        // Ammo status check
                        // if (ammo < 1)
                        //     Console.WriteLine("No ammo");

                        // Read Health (float)
                        // healthPointerBase = IntPtr.Add(baseAddress, 0x060ECC20);
                        // healthPointer = mem.ResolvePointer(healthPointerBase, [0xF0, 0x20, 0xA8, 0xCF0, 0x508, 0x110, 0x10]);
                        currentPointerName = nameof(healthPointer);
                        currentPointerValue = healthPointer;
                        int health = mem.ReadInt(healthPointer);
                        Console.WriteLine("Health: " + health);
                        currentPointerName = nameof(knifeHealthPointer);
                        currentPointerValue = knifeHealthPointer;
                        float knifeHealth = mem.ReadFloat(knifeHealthPointer);
                        currentPointerName = nameof(aimState1Pointer);
                        currentPointerValue = aimState1Pointer;
                        int aimState1 = mem.ReadInt(aimState1Pointer);
                        currentPointerName = nameof(aimState2Pointer);
                        currentPointerValue = aimState2Pointer;
                        int aimState2 = mem.ReadInt(aimState2Pointer);
                        currentPointerName = nameof(aimStateOnBoatPointer);
                        currentPointerValue = aimStateOnBoatPointer;
                        int aimStateOnBoat = mem.ReadInt(aimStateOnBoatPointer);
                        currentPointerName = nameof(earlyGamePointer);
                        currentPointerValue = earlyGamePointer;
                        int earlyGame = mem.ReadInt(earlyGamePointer);
                        currentPointerName = nameof(pauseStatesPointer);
                        currentPointerValue = pauseStatesPointer;
                        int pauseStates1 = mem.ReadInt(pauseStatesPointer);
                        currentPointerName = nameof(pauseStates2Pointer);
                        currentPointerValue = pauseStates2Pointer;
                        int pauseStates2 = mem.ReadInt(pauseStates2Pointer);
                        currentPointerName = nameof(pauseStates3Pointer);
                        currentPointerValue = pauseStates3Pointer;
                        int pauseStates3 = mem.ReadInt(pauseStates3Pointer);
                        currentPointerName = nameof(weaponTypePointer);
                        currentPointerValue = weaponTypePointer;
                        int weaponType = mem.Read2Byte(weaponTypePointer);
                        currentPointerName = nameof(boatPointer);
                        currentPointerValue = boatPointer;
                        int boat = mem.ReadInt(boatPointer);
                        currentPointerName = nameof(gettingOnABoatPointer);
                        currentPointerValue = gettingOnABoatPointer;
                        int gettingOnABoat = mem.ReadInt(gettingOnABoatPointer);
                        currentPointerName = nameof(backupAmmoPointer);
                        currentPointerValue = backupAmmoPointer;
                        int backupAmmo = mem.ReadInt(backupAmmoPointer);
                        currentPointerName = nameof(grenadeAmmoPointer);
                        currentPointerValue = grenadeAmmoPointer;
                        int grenadeAmmo = mem.ReadInt(grenadeAmmoPointer);
                        currentPointerName = nameof(flashbangAmmoPointer);
                        currentPointerValue = flashbangAmmoPointer;
                        int flashbangAmmo = mem.ReadInt(flashbangAmmoPointer);
                        currentPointerName = nameof(startMenuAndLoadingScreenPointer);
                        currentPointerValue = startMenuAndLoadingScreenPointer;
                        int startMenuAndLoadingScreen = mem.ReadInt(startMenuAndLoadingScreenPointer);
                        currentPointerName = nameof(hasGrenadeOrFlashbangPointer);
                        currentPointerValue = hasGrenadeOrFlashbangPointer;
                        int hasGrenadeOrFlashbang = mem.ReadInt(hasGrenadeOrFlashbangPointer);

                        Console.WriteLine("Health: " + health);
                        Console.WriteLine("Knife Health: " + knifeHealth);
                        Console.WriteLine("Aim State 1: " + aimState1);
                        Console.WriteLine("Aim State On Boat: " + aimStateOnBoat);
                        Console.WriteLine("Early Game Value: " + earlyGame);
                        Console.WriteLine("Aim State 2: " + aimState2);
                        Console.WriteLine("Pause States: " + pauseStates1);
                        Console.WriteLine("Pause States 2: " + pauseStates2);
                        Console.WriteLine("Pause States 3: " + pauseStates3);
                        Console.WriteLine("Weapon Type: " + weaponType);
                        Console.WriteLine("backupAmmo: " + backupAmmo);
                        Console.WriteLine("grenadeAmmo: " + grenadeAmmo);
                        Console.WriteLine("flashbangAmmo: " + flashbangAmmo);
                        Console.WriteLine($"L1: {buttons[4]}");
                        Console.WriteLine($"Left Trigger: {state.RotationX}");
                        Console.WriteLine("Start Menu and Loading Screen: " + startMenuAndLoadingScreen);
                        Console.WriteLine("Has Grenade or Flashbang: " + hasGrenadeOrFlashbang);
                        Console.WriteLine("Boat: " + boat);
                        Console.WriteLine("Getting On A Boat: " + gettingOnABoat);



                        // Console.WriteLine($"Swed read value: {value} from address: {address:X}\n");
                        // Console.WriteLine(BitConverter.ToString(code));



                        // Health status check
                        // if (health < 1f)
                        //     Console.WriteLine("Danger");
                        // else
                        //     Console.WriteLine("Good");


                        // if (health == 0 || health == 32759 || pauseStates1 > 0 || pauseStates2 == 71 || pauseStates3 == 256 || earlyGame == 0)
                        if (
                            health == 0 || health == 32759 || 
                        // startMenuAndLoadingScreen == 56 || 
                        pauseStates1 > 0 || (pauseStates2 == 71 && gettingOnABoat == 55) || (pauseStates3 == 256 && gettingOnABoat == 55) || earlyGame == 0 
                        // || ((weaponType == 17 || weaponType == 529) && state.RotationX > 32768 && backupAmmo == -1 && hasGrenadeOrFlashbang == 0 && (flashbangAmmo == 0 || grenadeAmmo == 0)) // this will make the effect inactive when either throwable has 0 ammo, but when one is 0, the other one will switch between inactive and active during throws
                        )
                        {

                            // if (
                            // // weapon_type == "" &&
                            // weaponType == 0) // Off
                            // {
                            //     p.instructions[4].type = InstructionType.RGBUpdate;
                            //     p.instructions[4].parameters = [controllerIndex, 0, 0, 0];

                            //     // resets when Lara doesn't have a weapon during loading screens, menus, cutscenes or during gameplay
                            //     previousBowAmmo = -1;
                            //     previousBowAmmo2 = -1;
                            //     previousBackupBowAmmo = -1;
                            //     previousBackupBowAmmo2 = -1;
                            //     previousHandgunAmmo = -1;
                            //     previousHandgunAmmo2 = -1;
                            //     previousMachinegunAmmo = -1;
                            //     previousMachinegunAmmo2 = -1;
                            //     previousGrenadeAmmo = -1;
                            //     previousShotgunAmmo = -1;
                            //     previousShotgunAmmo2 = -1;
                            //     previousShootArrow = -1;
                            //     previousShootHandgun = -1;
                            //     previousShootGrenade = -1;
                            //     previousShootShotgun = -1;

                            //     previousWeaponType = "";
                            //     previousWeaponTypeId = 0;
                            //     previousWeaponTypeId2 = 0;

                            //     lastMachinegunShotTime = 0;
                            // }

                            // Reset left trigger threshold to 0
                            p.instructions[0].type = InstructionType.TriggerThreshold;
                            p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

                            // Reset right trigger threshold to 0
                            p.instructions[1].type = InstructionType.TriggerThreshold;
                            p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

                            // Reset right trigger to off
                            p.instructions[2].type = InstructionType.TriggerUpdate;
                            p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                            // if (
                            //     // weapon_type.Contains("arrow") ||
                            //     weaponType == 17 && knifeHealth == 1 && aimState == 0
                            //  //  || weaponType == 5273748904 long
                            //  )
                            // {
                            //     // Reset right trigger to off
                            //     p.instructions[3].type = InstructionType.TriggerUpdate;
                            //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                            // }
                            // else {
                            // Reset right trigger to off
                            p.instructions[3].type = InstructionType.TriggerUpdate;
                            p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                            // }

                        }
                        else
                        {

                            // if (
                            //     // weapon_type.Contains("arrow") ||
                            //     weaponType == 17 && knifeHealth == 1 && aimState == 0
                            //  //  || weaponType == 5273748904 long
                            //  )
                            // {
                            //     // Reset right trigger to off
                            //     p.instructions[3].type = InstructionType.TriggerUpdate;
                            //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                            // }                       
                            if (
                            // weapon_type.Contains("handgun") ||
                            gettingOnABoat == 55
                            // || weaponType == 4326952780 long
                            )
                            {

                                if (boat == 26)
                                {
                                    // 256 is for aiming a weapon
                                    // if (aimState == 256)
                                    // {
                                    p.instructions[0].type = InstructionType.TriggerThreshold;
                                    p.instructions[0].parameters = [controllerIndex, Trigger.Left, 100];
                                    // p.instructions[1].type = InstructionType.TriggerThreshold;
                                    // p.instructions[1].parameters = [controllerIndex, Trigger.Right, 100];
                                    // }
                                    // else
                                    // {
                                    //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                    //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                    // }


                                    // Single pistol idle
                                    // Aiming with one pistol
                                    p.instructions[2].type = InstructionType.TriggerUpdate;
                                    // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
                                    // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
                                    // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest
                                    p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Medium]; // moderate
                                    // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 20, 0, 0, 0, 0];


                                    // if (state.RotationX < 32768 && buttons[4] && weaponType == 15)
                                    if ((aimStateOnBoat == 31 || buttons[4]) && weaponType == 15)
                                    // if (aimStateOnBoat == 31 && weaponType == 15)
                                    {

                                        p.instructions[1].type = InstructionType.TriggerThreshold;
                                        p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];

                                        p.instructions[3].type = InstructionType.TriggerUpdate;
                                        // original code
                                        // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 5, 158, 95, 0, 0, 0, 0];
                                        // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 175, 120, 0, 0, 0, 0]; // Resistance building up for quick fire
                                        // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 90, 165, 60, 0, 0, 0, 0]; // Smooth resistance for sustained fire
                                        p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 150, 130, 10, 0, 0, 0, 0]; // strongest // was 30


                                    }
                                    else
                                    {
                                        p.instructions[1].type = InstructionType.TriggerThreshold;
                                        p.instructions[1].parameters = [controllerIndex, Trigger.Right, 100];
                                        // if (aimState == 256)
                                        // {
                                        // Hand gun or Pistol:
                                        p.instructions[3].type = InstructionType.TriggerUpdate;
                                        // original code
                                        p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Medium]; // moderate
                                        // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 90, 165, 95, 0, 0, 0, 0]; // Smooth resistance for sustained fire
                                        // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 20, 0, 0, 0, 0];
                                    }
                                }
                                else
                                {
                                    p.instructions[0].type = InstructionType.TriggerThreshold;
                                    p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];
                                    p.instructions[1].type = InstructionType.TriggerThreshold;
                                    p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

                                    // Reset right trigger to off
                                    p.instructions[2].type = InstructionType.TriggerUpdate;
                                    p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                    p.instructions[3].type = InstructionType.TriggerUpdate;
                                    p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                }


                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if (
                                // weapon_type.Contains("arrow") ||
                                // ((knifeHealth == 1 || weaponType == 11 || weaponType == 523) && ((aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0))) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0) || (aimState1 == 256 && aimState2 > 0 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0) || (aimState1 == 256 && buttons[4]) || (aimState1 == 256 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (state.RotationX < 32768) || (state.RotationX < 32768 && buttons[4]) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                // (knifeHealth == 1 || weaponType == 11 || weaponType == 523) && (state.RotationX < 32768)
                                // (knifeHealth == 1 || weaponType == 11 || weaponType == 523) && (state.RotationX < 32768 && aimState2 >= 0 || weaponType == 11 || weaponType == 523 && buttons[4] && aimState1 == 0)
                                // (knifeHealth >= 1 && knifeHealth != 5 || weaponType == 11 || weaponType == 523) && (state.RotationX < 32768 && aimState2 >= 0 || weaponType == 11 || weaponType == 523)
                                (
                                    // knifeHealth > 0 || weaponType == 11 || weaponType == 523) && (state.RotationX < 32768 && aimState1 != 11 || weaponType == 11 || weaponType == 523)
                                    knifeHealth > 0 || weaponType == 11 || weaponType == 523) && (state.RotationX < 32768 && aimState2 == 0 || weaponType == 11 || weaponType == 523)
                                // && (buttons[4] && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0)
                                // aimState1 == 0 && knifeHealth == 1 || aimState1 == 32 && (weaponType == 11 || weaponType == 523) || weaponType == 143 || weaponType == 655 || weaponType == 15 || weaponType == 527 || ((weaponType == 17 || weaponType == 529) && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0))

                             //  || weaponType == 5273748904 long
                             )
                            {
                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                // if (aimState == 256)
                                // {
                                // Bow effect
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.FEEDBACK, 4, 5]; // strongest


                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // else if (weaponType == 11) 
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if ((aimStateOnBoat == 6 || aimStateOnBoat == 9) && pauseStates3 == 256 && state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 && aimState2 > 0) {
                                // do nothing
                            }
                            else if (
                                // weapon_type.Contains("arrow") ||
                                // ((knifeHealth == 1 || weaponType == 11 || weaponType == 523) && ((aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0))) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0) || (aimState1 == 256 && aimState2 > 0 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0) || (aimState1 == 256 && buttons[4]) || (aimState1 == 256 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (state.RotationX < 32768) || (state.RotationX < 32768 && buttons[4]) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                    // weaponType == 143 || weaponType == 655 || weaponType == 15 || weaponType == 527 || ((state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 17 || weaponType == 529) && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0))
                                    // (weaponType == 143 || weaponType == 655 || (hasGrenadeOrFlashbang == 0 && flashbangAmmo > 0 && backupAmmo <= 0)) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will be inactive when both throwables have 0 ammo 
                                    // (weaponType == 143 || weaponType == 655) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will be inactive when not holding a throwable
                                    // (weaponType == 143 || weaponType == 655 || hasGrenadeOrFlashbang == 0) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will remain active even when ammo is at 0 for both throwables
                                    // (weaponType == 143 || weaponType == 655 || hasGrenadeOrFlashbang == 0) && (aimState1 == 11) // will remain active even when ammo is at 0 for both throwables
                                    (weaponType == 143 || weaponType == 655 || hasGrenadeOrFlashbang == 0) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 && aimState2 > 0) // will remain active even when ammo is at 0 for both throwables
                                // aimState1 == 0 && knifeHealth == 1 || aimState1 == 32 && (weaponType == 11 || weaponType == 523) || weaponType == 143 || weaponType == 655 || weaponType == 15 || weaponType == 527 || ((weaponType == 17 || weaponType == 529) && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0))
                             //  || weaponType == 5273748904 long
                             )
                            {
                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                // if (aimState == 256)
                                // {
                                // Bow effect
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30

                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // else if (weaponType == 11) 
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if (
                                // weapon_type.Contains("arrow") ||
                                // ((knifeHealth == 1 || weaponType == 11 || weaponType == 523) && ((aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0))) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0 && aimState2 == 0) || (aimState1 == 256 && aimState2 == 0) || (aimState1 == 256 && aimState2 > 0 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (aimState1 == 0) || (aimState1 == 256 && buttons[4]) || (aimState1 == 256 && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0)) // not aiming with a knife, or aiming with a knife
                                // knifeHealth >= 1 && (state.RotationX < 32768) || (state.RotationX < 32768 && buttons[4]) || weaponType == 143  || weaponType == 655 || weaponType == 15 || weaponType == 527 // not aiming with a knife, or aiming with a knife
                                    // weaponType == 143 || weaponType == 655 || weaponType == 15 || weaponType == 527 || ((state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 17 || weaponType == 529) && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0))
                                    // (weaponType == 15 || weaponType == 527 || (hasGrenadeOrFlashbang == 0 && grenadeAmmo > 0)) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will be inactive when both throwables have 0 ammo 
                                    // (weaponType == 15 || weaponType == 527) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will be inactive when not holding a throwable
                                    // (weaponType == 15 || weaponType == 527 || hasGrenadeOrFlashbang == 0) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) // will remain active even when ammo is at 0 for both throwables
                                    // (weaponType == 15 || weaponType == 527 || hasGrenadeOrFlashbang == 0) && (aimState1 == 11) // will remain active even when ammo is at 0 for both throwables
                                    (weaponType == 15 || weaponType == 527 || hasGrenadeOrFlashbang == 0) && (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) // will remain active even when ammo is at 0 for both throwables
                                // aimState1 == 0 && knifeHealth == 1 || aimState1 == 32 && (weaponType == 11 || weaponType == 523) || weaponType == 143 || weaponType == 655 || weaponType == 15 || weaponType == 527 || ((weaponType == 17 || weaponType == 529) && backupAmmo <= 0 && (grenadeAmmo > 0 || flashbangAmmo > 0))
                             //  || weaponType == 5273748904 long
                             )
                            {
                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                // if (aimState == 256)
                                // {
                                // Bow effect
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30

                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // else if (weaponType == 11) 
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            // else if (
                            // // weapon_type.Contains("handgun") ||
                            // // aimState1 == 256 && aimState2 > 0 // not aiming with a knife
                            // // aimState1 == 256 // not aiming with a knife
                            // state.RotationX > 32768
                            // // || weaponType == 4326952780 long
                            // )
                            // {

                            //     // 256 is for aiming a weapon
                            //     // if (aimState == 256)
                            //     // {
                            //     p.instructions[1].type = InstructionType.TriggerThreshold;
                            //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                            //     // }
                            //     // else
                            //     // {
                            //     //     p.instructions[1].type = InstructionType.TriggerThreshold;
                            //     //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                            //     // }


                            //     // Single pistol idle
                            //     // Aiming with one pistol
                            //     // p.instructions[2].type = InstructionType.TriggerUpdate;
                            //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
                            //     // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
                            //     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest


                            //     // if (aimState == 256)
                            //     // {
                            //     // Hand gun or Pistol:
                            //     p.instructions[3].type = InstructionType.TriggerUpdate;
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft];
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 148, 32, 0, 0, 0, 0]; // default
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 125, 148, 160, 0, 0, 0, 0]; // moderate
                            //     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 27, 40, 86, 102, 184, 172, 2 }; // prefered

                            //     // alternative with a gradual resistance
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 158, 40, 0, 0, 0, 0]; // moderate
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 98, 158, 40, 0, 0, 0, 0]; // moderate
                            //     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 101, 158, 40, 0, 0, 0, 0]; // moderate // or 30
                            //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 4, 6, 4]; // moderate
                            //                                                                                                   // }
                            //                                                                                                   // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                            //                                                                                                   // {
                            //                                                                                                   //     // Reset right trigger to off
                            //                                                                                                   //     p.instructions[3].type = InstructionType.TriggerUpdate;
                            //                                                                                                   //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                            //                                                                                                   // }
                            //                                                                                                   // else
                            //                                                                                                   // {
                            //                                                                                                   //     // Reset right trigger to off
                            //                                                                                                   //     p.instructions[3].type = InstructionType.TriggerUpdate;
                            //                                                                                                   //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                            //                                                                                                   // }
                            // }

                            else if (
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 1 || weaponType == 1025 || weaponType == 513) // SG-09 R - a custom handgun made for Leon
                            // (aimState1 == 11) && (weaponType == 1 || weaponType == 1025 || weaponType == 513) // SG-09 R - a custom handgun made for Leon
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 1 || weaponType == 1025 || weaponType == 513) // SG-09 R - a custom handgun made for Leon
                            // aimState1 == 32 && (weaponType == 1 || weaponType == 1025 || weaponType == 513) // not aiming with a knife
                                                                                                                                                                                         // || weaponType == 4326952780 long
                            )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }


                                // Single pistol idle
                                // Aiming with one pistol
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                // if (aimState == 256)
                                // {
                                // Hand gun or Pistol:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 148, 32, 0, 0, 0, 0]; // default
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 125, 148, 160, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 27, 40, 86, 102, 184, 172, 2 }; // prefered

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 158, 40, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 98, 158, 40, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 101, 158, 40, 0, 0, 0, 0]; // moderate // or 30
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 170, 105, 0, 0, 0, 0]; // Crisp trigger snap for precision shooting
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 150, 45, 0, 0, 0, 0]; // Crisp trigger snap for precision shooting
                                                                                                                              // }
                                                                                                                              // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                                                                                                              // {
                                                                                                                              //     // Reset right trigger to off
                                                                                                                              //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                                                                                                              //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                                                                                                              // }
                                                                                                                              // else
                                                                                                                              // {
                                                                                                                              //     // Reset right trigger to off
                                                                                                                              //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                                                                                                              //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                                                                                                              // }
                            }
                            else if (
                            // weapon_type.Contains("handgun") ||
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 12289 || weaponType == 13313 || weaponType == 12801) // Sentinel Nine - a fully customized handgun for tackling bioterrorism
                            // (aimState1 == 11) && (weaponType == 12289 || weaponType == 13313 || weaponType == 12801) // Sentinel Nine - a fully customized handgun for tackling bioterrorism
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 12289 || weaponType == 13313 || weaponType == 12801) // Sentinel Nine - a fully customized handgun for tackling bioterrorism
                            // aimState1 == 32 && (weaponType == 1 || weaponType == 1025 || weaponType == 513 || weaponType == 12289 || weaponType == 13313 || weaponType == 12801) // not aiming with a knife
                                                                                                                                                                                         // || weaponType == 4326952780 long
                            )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }


                                // Single pistol idle
                                // Aiming with one pistol
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 7, 0, 20, 0, 0, 0, 0]; // strongest
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                // if (aimState == 256)
                                // {
                                // Hand gun or Pistol:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VerySoft];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 100, 148, 32, 0, 0, 0, 0]; // default
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 125, 148, 160, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 27, 40, 86, 102, 184, 172, 2 }; // prefered

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 105, 158, 40, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 98, 158, 40, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 101, 158, 40, 0, 0, 0, 0]; // moderate // or 30
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 4, 6, 8]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 110, 175, 120, 0, 0, 0, 0]; // Like weapon 1, slightly more intense for bioterrorism weapon
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 110, 155, 45, 0, 0, 0, 0]; // Like weapon 1, slightly more intense for bioterrorism weapon
                                                                                                                              // }
                                                                                                                              // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                                                                                                              // {
                                                                                                                              //     // Reset right trigger to off
                                                                                                                              //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                                                                                                              //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                                                                                                              // }
                                                                                                                              // else
                                                                                                                              // {
                                                                                                                              //     // Reset right trigger to off
                                                                                                                              //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                                                                                                              //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                                                                                                              // }
                            }

                            else if (
                                // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 5 || weaponType == 517) // SR M193 - a bolt action rifle with a scope
                                // (aimState1 == 11) && (weaponType == 5 || weaponType == 517) // SR M193 - a bolt action rifle with a scope
                                (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) && (weaponType == 5 || weaponType == 517) // SR M193 - a bolt action rifle with a scope
                                // aimState1 == 32 && (weaponType == 5 || weaponType == 517)
                            // || weaponType == 7526949996 // long
                            )
                            {
                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 210];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 25, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 25, 0, 30, 0, 0, 0, 0]; // stronger
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                                // if (aimState == 256

                                //     )
                                // {
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 60, 40, 86, 102, 184, 172, 2 }; // machine gun
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 135, 32, 0, 0, 0, 0]; // default
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 93, 135, 55, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 93, 135, 160, 0, 0, 0, 0]; // stronger

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 145, 50, 0, 0, 0, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 70, 145, 50, 0, 0, 0, 0]; // moderate // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 140, 40, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 3, 5, 4]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, 2, 4, 7]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 93, 183, 160, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 115, 195, 120, 0, 0, 0, 0]; // Powerful rifle with weighty precision trigger
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 85, 165, 40, 0, 0, 0, 0]; // Powerful rifle with weighty precision trigger

                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }

                            else if (
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 66 || weaponType == 578) // Skull Shaker - a shotgun with a sawed off stock and barrel
                            // (aimState1 == 11) && (weaponType == 66 || weaponType == 578) // Skull Shaker - a shotgun with a sawed off stock and barrel
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) && (weaponType == 66 || weaponType == 578) // Skull Shaker - a shotgun with a sawed off stock and barrel
                            // aimState1 == 32 && (weaponType == 66 || weaponType == 578)
                             //  || weaponType == 7146889127 long
                             )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                // Aiming
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                                // if (aimState == 256)
                                // {
                                // Shotgun or any heavy gun:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Soft]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 2, 4, 4]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 55, 145, 170, 0, 0, 0, 0]; // Sawed-off shotgun - heavy, violent trigger pull
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 55, 145, 60, 0, 0, 0, 0]; // Sawed-off shotgun - heavy, violent trigger pull

                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if (
                            // weapon_type.Contains("shotgun") ||
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 2 || weaponType == 514) // W-870 - a 12-gauge pump action shotgun
                            // (aimState1 == 11) && (weaponType == 2 || weaponType == 514) // W-870 - a 12-gauge pump action shotgun
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) && (weaponType == 2 || weaponType == 514) // W-870 - a 12-gauge pump action shotgun
                            // aimState1 == 32 && (weaponType == 2 || weaponType == 514)
                             //  || weaponType == 7146889127 long
                             )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                // Aiming
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                                // if (aimState == 256)
                                // {
                                // Shotgun or any heavy gun:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Hard]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 2, 4, 4]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 73, 163, 160, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 155, 165, 0, 0, 0, 0]; // Pump-action shotgun - forceful pump resistance
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 155, 60, 0, 0, 0, 0]; // Pump-action shotgun - forceful pump resistance


                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if (
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 3 || weaponType == 515) // TPM - a small lightweight submachine gun
                            // (aimState1 == 11) && (weaponType == 3 || weaponType == 515) // TPM - a small lightweight submachine gun
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) && (weaponType == 3 || weaponType == 515) // TPM - a small lightweight submachine gun
                            // aimState1 == 32 && (weaponType == 3 || weaponType == 515)
                             //  || weaponType == 7146889127 long
                             )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 200];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                // Aiming
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                                // if (aimState == 256)
                                // {
                                // Shotgun or any heavy gun:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VIBRATION, 0, 9, 7, 7, 10, 0]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VIBRATION, 3, 8, 9]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseB, 9, 55, 110]; // moderate
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.VIBRATION, 4, 8, 11]; // Submachine gun - rapid fire stutter


                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            else if (
                            // (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 || aimState2 > 0) && (weaponType == 9 || weaponType == 521) // bolt thrower - a weapon that shoots bolt projectiles
                            // (aimState1 == 11) && (weaponType == 9 || weaponType == 521) // bolt thrower - a weapon that shoots bolt projectiles
                            (state.RotationX > 32768 && aimState2 >= 0 && aimState2 == 0 ||aimState2 > 0) && (weaponType == 9 || weaponType == 521) // bolt thrower - a weapon that shoots bolt projectiles
                            // aimState1 == 32 && (weaponType == 9 || weaponType == 521)
                             //  || weaponType == 7146889127 long
                             )
                            {

                                // 256 is for aiming a weapon
                                // if (aimState == 256)
                                // {
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 220];
                                // }
                                // else
                                // {
                                //     p.instructions[1].type = InstructionType.TriggerThreshold;
                                //     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];
                                // }

                                // Aiming
                                // p.instructions[2].type = InstructionType.TriggerUpdate;
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 0, 0, 0, 0, 0];
                                // // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 63, 0, 30, 0, 0, 0, 0]; // stronger
                                // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.FEEDBACK, 0, 5, 4]; // strongest

                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];

                                // if (aimState == 256)
                                // {
                                // Shotgun or any heavy gun:
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.RigidAB, 95, 36, 38, 133, 186, 217, 129]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 122, 32, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 50, 0, 0, 0, 0]; // default moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 65, 122, 160, 0, 0, 0, 0]; // stronger
                                // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.Soft };

                                // alternative with a gradual resistance
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 45, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 132, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 139, 50, 0, 0, 0, 0];
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 52, 136, 50, 0, 0, 0, 0]; // preferred
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Soft]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.WEAPON, 4, 8, 8]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.SemiAutomaticGun, 2, 6, 8]; // moderate
                                // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 135, 200, 145, 0, 0, 0, 0]; // Bolt thrower - heavy draw, precise tension release
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Pulse, 80, 170, 145, 0, 0, 0, 0]; // Bolt thrower - heavy draw, precise tension release

                                // }
                                // else if (knifeHealth == 1) // if player has a knife equipped, use a different effect for the right trigger when not aiming
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 50, 50, 0, 0, 0, 0, 0]; // was 50 // 40 // 30
                                // }
                                // else
                                // {
                                //     // Reset right trigger to off
                                //     p.instructions[3].type = InstructionType.TriggerUpdate;
                                //     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                                // }
                            }
                            // else if (knifeHealth >= 1 && aimState1 == 0 && aimState2 > 0 && backupAmmo > -1)
                            // else if (knifeHealth == 1 && aimState1 == 0 && backupAmmo > -1)
                            else if ((weaponType == 17 || weaponType == 529) && knifeHealth > 0 && state.RotationX > 32768)
                            {
                                // do nothing
                            }
                            else
                            {
                                 // Reset left trigger threshold to 0
                                // p.instructions[0].type = InstructionType.TriggerThreshold;
                                // p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

                                // Reset right trigger threshold to 0
                                p.instructions[1].type = InstructionType.TriggerThreshold;
                                p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

                                // Reset right trigger to off
                                p.instructions[2].type = InstructionType.TriggerUpdate;
                                p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
                                p.instructions[3].type = InstructionType.TriggerUpdate;
                                p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Normal];
                            }

                        }
                    }
                    catch (OverflowException ex)
                    {
                        Console.WriteLine($"Memory pointer invalid after save reload while reading {currentPointerName}: {ex.Message}");
                        Console.WriteLine($"Pointer name: {currentPointerName}");
                        Console.WriteLine($"Pointer address string: {currentPointerValue}");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("Please restart the mod after loading a save.");
                        Console.WriteLine("Continuing to run and retrying on the next loop iteration.");
                        Thread.Sleep(100);
                        continue;
                    }

                    // Player number
                    p.instructions[4].type = InstructionType.PlayerLED;
                    p.instructions[4].parameters = [controllerIndex, false, false, true, false, false];

                    // Player LED for new revision controllers
                    p.instructions[5].type = InstructionType.PlayerLEDNewRevision;
                    p.instructions[5].parameters = [controllerIndex, PlayerLEDNewRevision.One];

                    Console.WriteLine("Instructions Sent\n");
                    Send(p);

                    // Wait 100ms before sending the next instruction
                    Thread.Sleep(100);

                    Console.WriteLine("Waiting for Server Response...\n");

                    // Make sure you setup some timeout for server response incase DSX has a bug or not running
                    Process[] process1 = Process.GetProcessesByName("DSX");
                    Process[] process2 = Process.GetProcessesByName("DualSenseY");

                    // Checks if either DSX or DualSenseY is running
                    if (process1.Length == 0 && process2.Length == 0)
                    {
                        Console.WriteLine("DSX is not running... \n");
                    }
                    else
                    {
                        try
                        {
                            byte[] bytesReceivedFromServer = client!.Receive(ref endPoint);

                            if (bytesReceivedFromServer.Length > 0)
                            {
                                Console.WriteLine("Tomb Raider DualSense Mod Initialized\n");
                                ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}")!;
                                Console.WriteLine("===================================================================");
                                Console.WriteLine($"Status: {ServerResponseJson!.Status}");
                                DateTime CurrentTime = DateTime.Now;
                                TimeSpan Timespan = CurrentTime - TimeSent;
                                Console.WriteLine($"Time Received: {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
                                Console.WriteLine($"isControllerConnected: {ServerResponseJson.isControllerConnected}");
                                Console.WriteLine($"BatteryLevel: {ServerResponseJson.BatteryLevel}");
                                Console.WriteLine("===================================================================\n");
                            }
                        }
                        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            Console.WriteLine("Connection reset by DSX (10054). Retrying...");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error communicating with DSX: {ex.Message}");
                            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            string errorMessage = $"[{timestamp}] Unexpected error communicating with DSX: {ex.Message}\n{ex.StackTrace}";
                            // Functions.WriteLog(errorMessage);
                        }
                    }
                }
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine("A fatal error occurred:");
                Console.WriteLine(ex.ToString());
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string errorMessage = $"[{timestamp}] Unexpected error: [Loop Crash] {ex.Message}\n{ex.StackTrace}";
                WriteLog(errorMessage);
                Console.WriteLine("==================================================");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                CheckGameProcess();
                ControllerLogic();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine("A fatal error occurred:");
                Console.WriteLine(ex.ToString());
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string errorMessage = $"[{timestamp}] Unexpected error: {ex.Message}\n{ex.StackTrace}";
                // Functions.WriteLog(errorMessage);

                // if (ex is Win32Exception win32Ex && win32Ex.NativeErrorCode == 5)
                // {
                //     // Access denied specific handling
                //     Console.WriteLine("\nAccess denied while trying to communicate with the game. The game may be running as Administrator.");
                //     Console.WriteLine("Fix: Run this mod as Administrator and try again.");
                // }

                Console.WriteLine("==================================================");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}