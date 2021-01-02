using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Image = System.Windows.Controls.Image;
using System.Threading;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace P2P_AV
{
    class ControlsStreamer
    {
        static int bufSize = 20;
        static Socket client;

        public static bool enabled;
        static int role;

        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;

        public async static Task MainAsync(int role, string addr, int port)
        {
            ControlsStreamer.role = role;

            if (role == 0)
            {
                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start(1);
                client = server.AcceptSocket();
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
            }
            else
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(addr, port);
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
                new Thread(new ThreadStart(receiverThreadProcessor)).Start();
            }
            await Task.Delay(-1);
        }

        static void receiverThreadProcessor()
        {
            byte[] data = new byte[20];

            bool lastLeft = false;
            bool lastRight = false;
            bool lastMiddle = false;
            bool lastX1 = false;
            bool lastX2 = false;
            while (true)
            {
                try
                {
                    while (client != null && client.Connected)
                    {
                        while (client.Available < 20) continue;
                        client.Receive(data);
                        if (!enabled) continue;

                        if (data[0] == 1)
                        {
                            double localX = BitConverter.ToDouble(data, 1);
                            double localY = BitConverter.ToDouble(data, 9);

                            double globalX = localX * ScreenWidth;
                            double globalY = localY * ScreenHeight;

                            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)globalX, (int)globalY);

                            Console.WriteLine($"local {Math.Round(localX, 5)} {Math.Round(localY, 5)}     global {Math.Round(globalX, 5)} {Math.Round(globalY, 5)}");
                        }
                        else if (data[0] == 2)
                        {
                            if (lastLeft != (data[1] & 0b00000001) > 0)
                            {
                                if ((data[1] & 0b00000001) > 0) Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                                else Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            }

                            if (lastRight != (data[1] & 0b00000010) > 0)
                            {
                                if ((data[1] & 0b00000010) > 0) Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                                else Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            }

                            if (lastMiddle != (data[1] & 0b00000100) > 0)
                            {
                                if ((data[1] & 0b00000100) > 0) Win32.mouse_event(Win32.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                                else Win32.mouse_event(Win32.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                            }

                            if (lastX1 != (data[1] & 0b00001000) > 0)
                            {
                                if ((data[1] & 0b00001000) > 0) Win32.mouse_event(Win32.MOUSEEVENTF_XDOWN, 0, 0, 0, 0);
                                else Win32.mouse_event(Win32.MOUSEEVENTF_XUP, 0, 0, 0, 0);
                            }

                            if (lastX2 != (data[1] & 0b00010000) > 0)
                            {
                                if ((data[1] & 0b00010000) > 0) Win32.mouse_event(Win32.MOUSEEVENTF_XDOWN, 0, 0, 0, 0);
                                else Win32.mouse_event(Win32.MOUSEEVENTF_XUP, 0, 0, 0, 0);
                            }

                            lastLeft = (data[1] & 0b00000001) > 0;
                            lastRight = (data[1] & 0b00000010) > 0;
                            lastMiddle = (data[1] & 0b00000100) > 0;
                            lastX1 = (data[1] & 0b00001000) > 0;
                            lastX2 = (data[1] & 0b00010000) > 0;
                        }
                        else if (data[0] == 3)
                        {
                            if (data[1] == 0)
                            {
                                int kc = BitConverter.ToInt32(data, 2);
                                Win32.keybd_event(kc, 0x45, Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP, 0);
                                Console.WriteLine($"Key release event {data[2]}");
                            }
                            if (data[1] == 1)
                            {
                                int kc = BitConverter.ToInt32(data, 2);
                                Win32.keybd_event(kc, 0x45, Win32.KEYEVENTF_EXTENDEDKEY, 0);
                                Console.WriteLine($"Key press event {data[2]}");
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public static void mousePositionChanged(double localX, double localY)
        {
            if (!enabled) return;
            if (role == 1) return;
            if (client == null || !client.Connected) return;

            byte[] ev = new byte[20];
            ev[0] = 1;
            Array.Copy(BitConverter.GetBytes(localX), 0, ev, 1, 8);
            Array.Copy(BitConverter.GetBytes(localY), 0, ev, 9, 8);
            client.Send(ev);
        }

        public static void mouseButtonPressed(MouseEventArgs args)
        {
            if (!enabled) return;
            if (role == 1) return;
            if (client == null || !client.Connected) return;

            byte[] ev = new byte[20];
            ev[0] = 2;
            byte mouseBtnData = 0;
            if (args.LeftButton == MouseButtonState.Pressed) mouseBtnData += 1;
            if (args.RightButton == MouseButtonState.Pressed) mouseBtnData += 2;
            if (args.MiddleButton == MouseButtonState.Pressed) mouseBtnData += 4;
            if (args.XButton1 == MouseButtonState.Pressed) mouseBtnData += 8;
            if (args.XButton2 == MouseButtonState.Pressed) mouseBtnData += 16;
            ev[1] = mouseBtnData;
            client.Send(ev);
        }

        public static void keyboardButtonPressed(KeyEventArgs args)
        {
            if (!enabled) return;
            if (role == 1) return;
            if (client == null || !client.Connected) return;

            byte[] ev = new byte[20];
            ev[0] = 3;
            ev[1] = 1;
            Array.Copy(BitConverter.GetBytes(KeyInterop.VirtualKeyFromKey(args.Key)), 0, ev, 2, 4);
            client.Send(ev);
        }

        public static void keyboardButtonPressed(System.Windows.Forms.Keys key)
        {
            if (!enabled) return;
            if (role == 1) return;
            if (client == null || !client.Connected) return;

            byte[] ev = new byte[20];
            ev[0] = 3;
            ev[1] = 1;
            Array.Copy(BitConverter.GetBytes((int)key), 0, ev, 2, 4);
            client.Send(ev);
        }

        public static void keyboardButtonReleased(KeyEventArgs args)
        {
            if (!enabled) return;
            if (role == 1) return;
            if (client == null || !client.Connected) return;

            byte[] ev = new byte[20];
            ev[0] = 3;
            ev[1] = 0;
            Array.Copy(BitConverter.GetBytes(KeyInterop.VirtualKeyFromKey(args.Key)), 0, ev, 2, 4);
            client.Send(ev);
        }
    }
}
