using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Qui
{
    internal class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        const uint INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_MOVE = 1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);


        static CancellationTokenSource _cts = new CancellationTokenSource();

        [STAThread]
        static void Main(string[] args)
        {
            CreateIcon();
            DoSomethingInBackground();
            Application.Run();
        }

        static void CreateIcon()
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "TestApp";
            trayIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Qui.Resources.clock.ico"));
            trayIcon.ContextMenuStrip = new ContextMenuStrip();
            trayIcon.ContextMenuStrip.Items.Add(
                "Quit", null, 
                (sender, args) => 
                { 
                    Task.Run(
                        () => 
                        { 
                            _cts.Cancel(); 
                            Application.Exit(); 
                        } 
                    );
                }
            );
            trayIcon.Visible = true;
        }

        private static void DoSomethingInBackground()
        {
            var ct = _cts.Token;
            Task.Run(
                async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        MoveMouse(1, 1, MOUSEEVENTF_MOVE);
                        MoveMouse(-1, -1, MOUSEEVENTF_MOVE);
                        await Task.Delay(30000, ct);
                    }
                    Application.Exit();
                }
            );
        }

        private static void MoveMouse(int x, int y, uint flag)
        {
            var inputs = new INPUT[]
            {
                new INPUT
                {
                    Type = 0,
                    Data = new MOUSEKEYBDHARDWAREINPUT
                    {
                        Mouse = new MOUSEINPUT
                        {
                            X = x,
                            Y = y,
                            Flags = flag
                        }
                    }
                }
            };
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
