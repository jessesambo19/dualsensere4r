// using Newtonsoft.Json;
// using System.Diagnostics;
// using System.Net;
// using System.Net.Sockets;
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

//             // this will forever run triggering the different trigger modes to make it adaptive
//             // while (true)

//             while (gameProcessName.Length == 0)
//             {
//                 gameProcessName = Process.GetProcessesByName("Kena-Win64-Shipping_original");
//             }


//             while (gameProcessName.Length != 0)
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
//                 p.instructions[0].parameters = [controllerIndex, Trigger.Left, 80];

//                 // ----------------------------------------------------------------------------------------------------------------------------

//                 // Adaptive Triggers:

//                 // *NOTE* If you're gonna be applying Adaptive triggers for both R2 And L2, you should send 2 UDP messages, one for L2 and one for R2

//                 // if (btnState.L2 > 140 && btnState.R2 > 200 && !square_pressed)
//                 if (btnState.L2 > 80 && btnState.R2 > 250)
//                 {
//                     p.instructions[2].type = InstructionType.TriggerUpdate;
//                     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.VibratePulseB, 60, 10, 20, 80, 0, 0, 0];
//                 }
//                 else
//                 {
//                     // Alternatively: Aiming
//                     p.instructions[2].type = InstructionType.TriggerUpdate;
//                     // p.instructions[2].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 160, 30, 0, 0, 0, 0, 0 }; // third parameter controls the strength of the resistance
//                     // p.instructions[2].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 80, 30, 0, 0, 0, 0, 100 }; // 38% - 40% pull
//                     p.instructions[2].parameters = [controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 40, 30, 0, 0, 0, 0, 100]; // 27% - 30% pull
//                     //                  p.instructions[2].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 150, 175, 35, 0, 0, 0, 0 }; // third parameter controls the strength of the resistance
//                     // p.instructions[2].parameters = new object[] { controllerIndex, Trigger.Left, TriggerMode.Resistance, 3, 1 }; // 40% pull
//                 }
//                 if (btnState.L2Btn)
//                 // if (btnState.L2 > 80)
//                 {
//                     p.instructions[1].type = InstructionType.TriggerThreshold;
//                     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 250];

//                     // Bow effect
//                     p.instructions[3].type = InstructionType.TriggerUpdate;
//                     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.Bow, 0, 8, 3, 7];

//                 }
//                 else
//                 {
//                     p.instructions[1].type = InstructionType.TriggerThreshold;
//                     p.instructions[1].parameters = [controllerIndex, Trigger.Right, 0];

//                     // when not aiming with L2 then a resistance is applied on R2 for melee
//                     p.instructions[3].type = InstructionType.TriggerUpdate;
//                     p.instructions[3].parameters = [controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.Rigid, 0, 50, 0, 0, 0, 0, 0];
//                     // p.instructions[3].parameters = new object[] { controllerIndex, Trigger.Right, TriggerMode.CustomTriggerValue, CustomTriggerValueMode.PulseA, 160, 30, 35, 0, 0, 0, 0 }; 0, 20, 35, 70, 200
//                 }

//                 // Touchpad LED

//                 p.instructions[4].type = InstructionType.RGBUpdate;
//                 // p.instructions[4].parameters = [controllerIndex, 0, 255, 255];
//                 p.instructions[4].parameters = [controllerIndex, 5, 52, 255];

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