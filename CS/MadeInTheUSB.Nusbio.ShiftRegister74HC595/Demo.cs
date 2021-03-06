/*
    Copyright (C) 2015 MadeInTheUSB LLC

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial 
    portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadeInTheUSB;
using MadeInTheUSB.GPIO;

namespace LedConsole
{
    class Demo
    {
        const NusbioGpio clockPin = NusbioGpio.Gpio0;  // YELLOW Pin connected to SH_CP of 74HC595
        const NusbioGpio dataPin  = NusbioGpio.Gpio1;  // BLUE Pin connected to DS of 74HC595
        const NusbioGpio latchPin = NusbioGpio.Gpio2;  // GREEN Pin connected to ST_CP of 74HC595
        

        //private const int LSBFIRST = 0;
        //private const int MSBFIRST = 1;

        //static void shiftOut(Nusbio Nusbio, NusbioGpio dataPin , NusbioGpio clockPin , int bitOrder , int val)
        //{
        //    int i;

        //    for (i = 0; i < 8; i++)
        //    {
        //        if (bitOrder == LSBFIRST)
        //        {
        //            var a = (val & (1 << i));
        //            Nusbio.GPIOS[dataPin].DigitalWrite(Nusbio.ConvertToPinState(a));
        //        }
        //        else
        //        {
        //            var b = (val & (1 << (7 - i)));
        //            Nusbio.GPIOS[dataPin].DigitalWrite(Nusbio.ConvertToPinState(b));
        //        }
        //        Nusbio.GPIOS[clockPin].DigitalWrite(PinState.High);
        //        Nusbio.GPIOS[clockPin].DigitalWrite(PinState.Low);
        //    }   
        //}

        //static void Register74HC595_8Bit_Send8BitValue(Nusbio Nusbio, byte v) {

        //    Nusbio.GPIOS[latchPin].DigitalWrite(PinState.Low);            
        //    shiftOut(Nusbio, dataPin, clockPin, MSBFIRST, v);
        //    Nusbio.GPIOS[latchPin].DigitalWrite(PinState.High);
        //}


        static string GetAssemblyProduct()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if(attribs.Length > 0)
                return  ((AssemblyProductAttribute) attribs[0]).Product;
            return null;
        }

        static void Cls(Nusbio nusbio, ShiftRegister74HC595 sr)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct(), ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.TitleBar(ConsoleEx.WindowHeight-2, Nusbio.GetAssemblyCopyright(), ConsoleColor.White, ConsoleColor.DarkBlue);

            ConsoleEx.Bar(0, ConsoleEx.WindowHeight - 4,string.Format("Extended Gpio Count:{0}, StartIndex:{1}, EndIndex:{2}",sr.MaxGpio, sr.MinGpioIndex, sr.MaxGpioIndex), ConsoleColor.Black, ConsoleColor.DarkCyan);
            ConsoleEx.Bar(0, ConsoleEx.WindowHeight-3, string.Format("Nusbio SerialNumber:{0}, Description:{1}", nusbio.SerialNumber, nusbio.Description), ConsoleColor.Black, ConsoleColor.DarkCyan);

            ConsoleEx.WriteMenu(-1, 2, "1)6 extra gpio pins demo   2)1 gpio pins demo   3) 16Bit Demo");

            ConsoleEx.WriteMenu(-1, 4, "Q)uit");
        }


        public static void GpioAnimatioOneAtTheTime(Nusbio nusbio, ShiftRegister74HC595 sr, int waitTime, bool demoGpio3to7Too = false)
        {
            // Gpio 0,1,2 are used to control the 2 shift register 74HC595, but we can use the other 5
            sr.SetGpioMask(ShiftRegister74HC595.ExGpio.None);

            for (int i = 0; i < sr.MaxGpio; i++)
            {
                var g = sr.GetGpioFromIndex(i + sr.MinGpioIndex);
                Console.WriteLine(g);
                sr.SetGpioMask(g);
                Thread.Sleep(waitTime);
                if (Console.KeyAvailable) return;
            }
            for (int i = sr.MaxGpio - 1; i >= 0; i--)
            {
                var g = sr.GetGpioFromIndex(i + sr.MinGpioIndex);
                Console.WriteLine(g);
                sr.SetGpioMask(g);
                Thread.Sleep(waitTime);
                if (Console.KeyAvailable) return;
            }
            sr.SetGpioMask(0);
            Thread.Sleep(waitTime * 10);
        }


        public static void TestSetting16bitAtOnce(Nusbio nusbio, ShiftRegister74HC595 sr, int waitTime)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct() + " 16 bit value demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.Gotoxy(0, 3);

            //sr.TestPins();

            sr.Reset();
            var p1 = 1;
            var p2 = 256;
            for(var i=0; i<8; i++)
            {
                sr.SetGpioMask(p1+p2);
                //sr.SetDataLinesAndAddrLines((byte)p1, (byte)(p2 >> 8));
                //sr.SetGpioMask((byte)p1, (byte)(p2 >> 8));

                Console.WriteLine("16 bite value:{0:000000} - {1} {2}",
                    p1 + p2,
                    MadeInTheUSB.WinUtil.BitUtil.BitRpr(p1),
                    MadeInTheUSB.WinUtil.BitUtil.BitRpr(p2 >> 8)
                );
                Thread.Sleep(waitTime);
                p1 *= 2;
                p2 *= 2;
            }

            Console.WriteLine("---");
            Thread.Sleep(waitTime);

            sr.SetGpioMask(0);
            p1      = 1;
            p2      = 256;
            var pp1 = p1;
            var pp2 = p2;
            for (var i = 0; i < 8; i++)
            {
                //sr.SetGpioMask(pp1 + pp2);
                sr.SetGpioMask((byte)pp1, (byte)(pp2 >> 8));

                Console.WriteLine("16 bite value:{0:000000} - {1} {2}",
                    pp1 + pp2,
                    MadeInTheUSB.WinUtil.BitUtil.BitRpr(pp1),
                    MadeInTheUSB.WinUtil.BitUtil.BitRpr(pp2 >> 8)
                );
                Thread.Sleep(waitTime);
                p1 *= 2;
                p2 *= 2;
                pp1 += p1;
                pp2 += p2;
            }
            Console.WriteLine("Hit any key to continue");
            Console.ReadKey();
            sr.SetGpioMask(0);
        }

        public static void GpioAnimation(Nusbio nusbio, ShiftRegister74HC595 sr, int waitTime, bool demoGpio3to7Too = false)
        {
            // Gpio 0,1,2 are used to control the 2 shift register 74HC595, but we can use the other 5
            var nusbioGpioLeft = new List<NusbioGpio>() { NusbioGpio.Gpio3, NusbioGpio.Gpio4, NusbioGpio.Gpio5, NusbioGpio.Gpio6, NusbioGpio.Gpio7, };
            sr.SetGpioMask(ShiftRegister74HC595.ExGpio.None);

            if (!demoGpio3to7Too)
            {
                GpioAnimatioOneAtTheTime(nusbio, sr, waitTime, demoGpio3to7Too);
            }
            if (demoGpio3to7Too) // Demo internal nusbio gpio 3..7 also
            {
                for (int i = 0; i < nusbioGpioLeft.Count; i++)
                {
                    nusbio[nusbioGpioLeft[i]].DigitalWrite(PinState.High);
                    Console.WriteLine(nusbioGpioLeft[i]);
                    Thread.Sleep(waitTime);
                    if (Console.KeyAvailable) return;
                }
            }
            for (int i = 0; i < sr.GetGpioEnums().Count; i++)
            {
                sr.DigitalWrite(i + sr.MinGpioIndex, PinState.High);
                Console.WriteLine(sr.GpioStates);
                Thread.Sleep(waitTime);
                if (Console.KeyAvailable) return;
            }
            for (int i = sr.GetGpioEnums().Count - 1; i >= 0; i--)
            {
                sr.DigitalWrite(i + sr.MinGpioIndex, PinState.Low);
                Console.WriteLine(sr.GpioStates);
                Thread.Sleep(waitTime);
                if (Console.KeyAvailable) return;
            }

            if (demoGpio3to7Too)
            {
                for (int i = nusbioGpioLeft.Count-1; i>=0; i--)
                {
                    nusbio[nusbioGpioLeft[i]].DigitalWrite(PinState.Low);
                    Console.WriteLine(nusbioGpioLeft[i]);
                    Thread.Sleep(waitTime);
                    if (Console.KeyAvailable) return;
                }
            }
            sr.SetGpioMask(0);
        }

        private static void GpioDemo(Nusbio nusbio, ShiftRegister74HC595 sr, bool demoGpio3to7Too = false)
        {
            Console.Clear();
            ConsoleEx.TitleBar(0, GetAssemblyProduct() + " Gpio 16 Demo", ConsoleColor.Yellow, ConsoleColor.DarkBlue);
            ConsoleEx.Gotoxy(0, 3);

            int wait = 150;
            while (true)
            {
                GpioAnimation(nusbio, sr, wait, demoGpio3to7Too);
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q) break;
            }
        }

        public static void Run(string[] args)
        {
            Console.WriteLine("Nusbio Initializing");
            var serialNumber = Nusbio.Detect();
            if (serialNumber == null) // Detect the first Nusbio available
            {
                Console.WriteLine("Nusbio not detected");
                return;
            }

            // Set to 8 if only using 1 Shift Register 74HC595
            const int MAX_EXTENDED_GPIO = 16;

            //Nusbio.BaudRate = Nusbio.FastestBaudRate / 512;

            using (var nusbio = new Nusbio(serialNumber))
            {
                var sr = new ShiftRegister74HC595(nusbio, MAX_EXTENDED_GPIO, dataPin, latchPin, clockPin);
                Cls(nusbio, sr);
                sr.Reset();
                while (nusbio.Loop())
                {
                    if (Console.KeyAvailable)
                    {
                        var k = Console.ReadKey(true).Key;
                        if (k == ConsoleKey.Q) break;

                        if (k == ConsoleKey.D1)
                            GpioDemo(nusbio, sr);
                        if (k == ConsoleKey.D2)
                            GpioDemo(nusbio, sr, demoGpio3to7Too: true);
                        if (k == ConsoleKey.D3)
                            TestSetting16bitAtOnce(nusbio, sr, 400);

                        Cls(nusbio, sr);
                    }
                }
            }
            Console.Clear();
        }
        

    }
}

