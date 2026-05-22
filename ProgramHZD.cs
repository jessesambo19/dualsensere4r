// using Newtonsoft.Json;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
// using System.Security.Cryptography;
// using System.Text;

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

//         static void Main(string[] args)
//         {
//             Connect();

//             Process[] gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");

//             Console.WriteLine("Monitoring game process...\n");

//             var dualsense = WujLibPad.Dualsense_Create(); // Find first available controller

//             int previousR2 = 0; // this keeps track of the threshold of the previous R2 pull

//             // int previousBowAmmo = -1;

//             // this will forever run triggering the different trigger modes to make it adaptive
//             while (true)

//             // while (gameProcessName.Length == 0)
//             // {
//             //     gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");
//             // }

//             // while (gameProcessName.Length != 0)
//             {
//                 gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");

//                 // This is all you need to use this library with UDP so u can get buttonstates without interrupting DSY
//                 WujLibPad.Dualsense_Read(dualsense);
//                 WujLibPad.ButtonState btnState = new();
//                 WujLibPad.Dualsense_GetButtonState(dualsense, ref btnState);

//                 Packet p = new();

//                 int controllerIndex = 0;

//                 // Set how many instructions you want to send at one time
//                 p.instructions = new Instruction[7];

//                 // TriggerThreshold needs 2 params LeftTrigger:0-255 RightTrigger:0-255
//                 // This is used for telling the emulation when to send the "pressed state"

//                 p.instructions[0].type = InstructionType.TriggerThreshold;
//                 // p.instructions[0].parameters = [controllerIndex, Trigger.Left, 140];
//                 p.instructions[0].parameters = [controllerIndex, Trigger.Left, 0];

//                 // ----------------------------------------------------------------------------------------------------------------------------

//                 // Adaptive Triggers:

//                 // *NOTE* If you're gonna be applying Adaptive triggers for both R2 And L2, you should send 2 UDP messages, one for L2 and one for R2

//                 // Inside your update loop:
//                 int currentR2 = btnState.R2; // assuming this gives you a 0–255 analog value
//                 bool droppedFromFullPress = false;
//                 // int currentAmmo = ammo;

//                 if (btnState.circle || btnState.cross || !btnState.L2Btn)
//                 {
//                     // droppedFromFullPress = false;
//                     previousR2 = 0;
//                 }

//                 if (btnState.L2Btn)
//                 {
//                     if (btnState.circle || btnState.cross)
//                     {
//                         previousR2 = 0;
//                     }
//                     // else if (btnState.L2 > 140 && currentR2 > 170)
//                     else if (btnState.L2 > 140 && currentR2 > 160)
//                     {
//                         previousR2 = currentR2; // gets the current threshold e.g anything above 170
//                         Console.WriteLine("Hi");
//                     }
//                     // droppedFromFullPress = previousR2 > 170 && currentR2 < 170; // it checks is L2 is pressed to 140 threshold and checks if the r2 was already
//                     // pulled to a number greater than 170 and it was let go in order to be less than 170
//                     droppedFromFullPress = previousR2 > 160 && currentR2 < 160;

//                     p.instructions[1].type = InstructionType.TriggerThreshold;
//                     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 160];
//                     // p.instructions[1].parameters = [controllerIndex, Trigger.Right, 155]; // hzd

//                     // Bow effect
//                     // p.instructions[3].type = InstructionType.TriggerUpdate;
//                     // // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 6, 3, 8];
//                     // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 5, 5, 8];
//                     // // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 8, 3, 8];

//                     p.instructions[3].type = InstructionType.TriggerUpdate;
//                     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 40, 0, 0, 0, 0, 0]; // was 50 // 40
//                                                                                                                                                                          // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 155, 175, 200, 0, 0, 0, 0];


//                     // if (btnState.R2 > 170)
//                     if (btnState.R2 > 160)
//                     {
//                         p.instructions[3].type = InstructionType.TriggerUpdate;
//                         p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 20, 80, 0, 0, 0]; // default
//                                                                                                                                                                                         // p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 30, 80, 0, 0, 0];
//                                                                                                                                                                                         //p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 50, 80, 0, 0, 0];
//                     }
//                 }
//                 else
//                 {
//                     p.instructions[1].type = InstructionType.TriggerThreshold;
//                     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

//                     // when not aiming with L2 then a resistance is applied on R2 for melee
//                     p.instructions[3].type = InstructionType.TriggerUpdate;
//                     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 40, 0, 0, 0, 0, 0]; // was 50
//                 }

//                 // if (btnState.L2 > 140 && btnState.R2 > 160)
//                 // if (btnState.L2 > 140 && droppedFromFullPress) //hzd
//                 if (droppedFromFullPress
//                 //  btnState.L2 > 140 && previousBowAmmo != -1 && currentAmmo < previousBowAmmo
//                 ) //hzd

//                 {
//                     p.instructions[2].type = InstructionType.TriggerUpdate;
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 20, 80, 0, 0, 0];
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid,0, 40, 0, 0, 0, 0, 0]; // was 50
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 8, 8, 8];
//                     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseB, 1, 190, 100, 0, 0, 0, 0];
//                     previousR2 = 0;
//                 }
//                 else
//                 {
//                     // Alternatively: Aiming
//                     p.instructions[2].type = InstructionType.TriggerUpdate;
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 160, 30, 0, 0, 0, 0, 0]; // third parameter controls the strength of the resistance
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Bow, 0, 5, 5, 8];
//                     // p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 160, 50, 0, 0, 0, 0, 0];
//                     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.Normal];
//                 }

//                 // previousBowAmmo = currentAmmo;


//                 // Touchpad LED

//                 p.instructions[4].type = InstructionType.RGBUpdate;
//                 // p.instructions[4].parameters = [controllerIndex, 204, 85, 0]; // burnt orange
//                 p.instructions[4].parameters = [controllerIndex, 230, 110, 30]; // slightly lighter burnt orange
//                 // p.instructions[4].parameters = [controllerIndex, 180, 90, 50]; // more weathered burnt orange


//                 p.instructions[5].type = InstructionType.PlayerLED;
//                 p.instructions[5].parameters = [controllerIndex, false, false, true, false, false];


//                 Console.WriteLine("Instructions Sent\n");
//                 Send(p);

//                 // Wait 100ms before sending the next instruction
//                 Thread.Sleep(100);

//                 Console.WriteLine("Waiting for Server Response...\n");

//                 // Make sure you setup some timeout for server response incase DSX has a bug or not running
//                 Process[] process = Process.GetProcessesByName("DSX");

//                 if (process.Length == 0)
//                 {
//                     Console.WriteLine("DSX is not running... \n");
//                 }
//                 else
//                 {
//                     byte[] bytesReceivedFromServer = client!.Receive(ref endPoint);

//                     if (bytesReceivedFromServer.Length > 0)
//                     {
//                         ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}")!;
//                         Console.WriteLine("===================================================================");

//                         Console.WriteLine($"Status: {ServerResponseJson!.Status}");
//                         DateTime CurrentTime = DateTime.Now;
//                         TimeSpan Timespan = CurrentTime - TimeSent;
//                         // First send shows high Milliseconds response time for some reason
//                         Console.WriteLine($"Time Received: {ServerResponseJson.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
//                         Console.WriteLine($"isControllerConnected: {ServerResponseJson.isControllerConnected}");
//                         Console.WriteLine($"BatteryLevel: {ServerResponseJson.BatteryLevel}");

//                         Console.WriteLine("===================================================================\n");
//                     }
//                 }

//                 Console.WriteLine("Press any key to send again\n");
//             }

//             Console.WriteLine("Game closed. Terminating adaptive triggers...");
//         }
//     }
// }