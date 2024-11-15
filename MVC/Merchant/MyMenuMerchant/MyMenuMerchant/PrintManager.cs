using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using MyMenuMerchant.Models;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace MyMenuMerchant
{
    public class PrintManager
    {
        Socket pSocket;
        private SettingPrinter setting;
        string spacebar80 = "                                            "; //42
        string spacebar48 = "                                ";
        int printsize = 1;
        public PrintManager(int v)
        {
            this.printsize = v;
        }
        public PrintManager(SettingPrinter setting)
        {
            this.setting = setting;
        }
        public async Task Open()
        {
            try
            {
                pSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                pSocket.ReceiveTimeout = 1000;
                pSocket.SendTimeout = 1000;
                IAsyncResult result = pSocket.BeginConnect(setting.IPADDRESS, Int32.Parse(setting.PORTNUMBER), null, null);

                bool success = result.AsyncWaitHandle.WaitOne(5000, true);

                if (pSocket.Connected)
                {
                    pSocket.EndConnect(result);
                }
                else
                {
                    pSocket.Close();
                    throw new ApplicationException("ไม่สามารถเชื่อมต่อกับเครื่องปริ้นได้");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

        }
        public async Task Close()
        {
            pSocket.Close();
        }
        public async Task Write(byte[] bytesData, int timeOut)
        {
            byte[] b = new byte[]
               {
                    (byte)27,
                    (byte)64
               };
            pSocket.Send(bytesData, bytesData.Length,0);
            pSocket.Send(Encoding.ASCII.GetBytes("\n\n"));

        }
        public string ReplaceSpacebar2(string data1, string data2)
        {
            string spacebar = "";
            if (printsize == 1)
            {
                spacebar = spacebar48;
            }
            else
            {
                spacebar = spacebar80;
            }
            if (spacebar.Length > (ThaiLength(data1) + ThaiLength(data2)))
            {
                int l = spacebar.Length - ThaiLength(data1) - ThaiLength(data2);
                data1 = data1 + Mid(spacebar, 1, l) + data2;
            }
            else
            {
                data1 = data1 + " " + data2;
            }
            return data1;
        }
        public int ThaiLength(string stringthai)
        {
            int len = 0;
            int l = stringthai.Length;
            for (int i = 0; i < l; ++i)
            {
                if (char.GetUnicodeCategory(stringthai[i]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    ++len;
            }
            return len;
        }
        private String Mid(String Param, int StartIndex, int Length)
        {
            String Result = Param.Substring(StartIndex, Length);
            return Result;
        }

        public static string PrintDateTime(DateTime d)
        {
            var destinationTimeZone = TimeZoneInfo.Local;
            string time = TimeZoneInfo.ConvertTimeFromUtc(d, destinationTimeZone).ToString("dd/MM/yyyy HH:mm tt", new CultureInfo("en-US"));
            return time;
        }
        public static List<string> SplitItemName(int length, string itemName)
        {
            List<string> names = new List<string>();
            string line1 = "", line2 = "", line3 = "";

            names.Add(line1);
            names.Add(line2);
            names.Add(line3);


            if (itemName.Length < length) { names[0] = itemName; }
            else
            {
                var rest = itemName.Split(' ');
                int j = 0;
                string text = "";
                for (int i = 0; i < rest.Count(); i++)
                {
                    text = names[j] + rest[i] + " ";
                    if (text.Length < length)
                    {
                        names[j] = text;
                    }
                    else
                    {
                        j = j + 1;
                        if (j < names.Count)
                        {
                            names[j] = rest[i] + " ";
                        }
                    }
                    if (j >= names.Count)
                    {
                        break;
                    }
                }

            }
            return names;
        }


        public byte[] GetLogo(Bitmap image)
        {
            string logo = "";
            
            BitmapData data = GetBitmapData(image);
            BitArray dots = data.Dots;
            byte[] width = BitConverter.GetBytes(data.Width);

            int offset = 0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((char)0x1B);
            bw.Write('@');

            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)24);

            while (offset < data.Height)
            {
                bw.Write((char)0x1B);
                bw.Write('*');         // bit-image mode
                bw.Write((byte)33);    // 24-dot double-density
                bw.Write(width[0]);  // width low byte
                bw.Write(width[1]);  // width high byte

                for (int x = 0; x < data.Width; ++x)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; ++b)
                        {
                            int y = (((offset / 8) + k) * 8) + b;
                            // Calculate the location of the pixel we want in the bit array.
                            // It'll be at (y * width) + x.
                            int i = (y * data.Width) + x;

                            // If the image is shorter than 24 dots, pad with zero.
                            bool v = false;
                            if (i < dots.Length)
                            {
                                v = dots[i];
                            }
                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }

                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write((char)0x0A);
            }
            // Restore the line spacing to the default of 30 dots.
            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)30);

            bw.Flush();
            byte[] bytes = stream.ToArray();
            //return logo + Encoding.Default.GetString(bytes);
            return bytes;
        }

        public BitmapData GetBitmapData(Bitmap b)
        {
            using (var bitmap = b)
            {
                var threshold = 127;
                var index = 0;
                double multiplier = 570; // this depends on your printer model. for Beiyang you should use 1000
                double scale = (double)(multiplier / (double)bitmap.Width);
                int xheight = (int)(bitmap.Height * scale);
                int xwidth = (int)(bitmap.Width * scale);
                var dimensions = xwidth * xheight;
                var dots = new BitArray(dimensions);

                for (var y = 0; y < xheight; y++)
                {
                    for (var x = 0; x < xwidth; x++)
                    {
                        var _x = (int)(x / scale);
                        var _y = (int)(y / scale);
                        var color = bitmap.GetPixel(_x, _y);
                        var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        dots[index] = (luminance < threshold);
                        index++;
                    }
                }

                return new BitmapData()
                {
                    Dots = dots,
                    Height = (int)(bitmap.Height * scale),
                    Width = (int)(bitmap.Width * scale)
                };
            }
        }

        public class BitmapData
        {
            public BitArray Dots
            {
                get;
                set;
            }

            public int Height
            {
                get;
                set;
            }

            public int Width
            {
                get;
                set;
            }
        }
        //public Bitmap ConvertImageToBitmap(string fileName)
        //{
        //    Bitmap bitmap;
        //    //แบบที่ 1
        //    using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
        //    {
        //        Image image = Image.FromStream(bmpStream);
        //        bitmap = new Bitmap(image);
        //    }
        //    return bitmap;
        //}
        //public byte[] GenBitmapCode(Bitmap image, bool doubleWidth, bool doubleHeight)
        //{
            

        //    var imageAfter = ConvertTo1Bit(image);
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        imageAfter.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
        //        return memoryStream.ToArray();
        //    }

        //    //MemoryStream memoryStream = new MemoryStream();

        //    //imageAfter.Save(memoryStream, ImageFormat.Bmp);
        //    //byte[] bitmapRecord = memoryStream.ToArray();
        //    //return bitmapRecord;
        //}
        
        //public static Bitmap ConvertTo1Bit(Bitmap input)
        //{
        //    var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
        //    var output = new Bitmap(input.Width, input.Height, PixelFormat.Format1bppIndexed);
        //    var data = new sbyte[input.Width, input.Height];
        //    var inputData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //    try
        //    {
        //        var scanLine = inputData.Scan0;
        //        var line = new byte[inputData.Stride];
        //        for (var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride)
        //        {
        //            Marshal.Copy(scanLine, line, 0, line.Length);
        //            for (var x = 0; x < input.Width; x++)
        //            {
        //                data[x, y] = (sbyte)(64 * (GetGreyLevel(line[x * 3 + 2], line[x * 3 + 1], line[x * 3 + 0]) - 0.5));
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        input.UnlockBits(inputData);
        //    }
        //    var outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
        //    try
        //    {
        //        var scanLine = outputData.Scan0;
        //        for (var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride)
        //        {
        //            var line = new byte[outputData.Stride];
        //            for (var x = 0; x < input.Width; x++)
        //            {
        //                var j = data[x, y] > 0;
        //                if (j) line[x / 8] |= masks[x % 8];
        //                var error = (sbyte)(data[x, y] - (j ? 32 : -32));
        //                if (x < input.Width - 1) data[x + 1, y] += (sbyte)(7 * error / 16);
        //                if (y < input.Height - 1)
        //                {
        //                    if (x > 0) data[x - 1, y + 1] += (sbyte)(3 * error / 16);
        //                    data[x, y + 1] += (sbyte)(5 * error / 16);
        //                    if (x < input.Width - 1) data[x + 1, y + 1] += (sbyte)(1 * error / 16);
        //                }
        //            }
        //            Marshal.Copy(line, 0, scanLine, outputData.Stride);
        //        }
        //    }
        //    finally
        //    {
        //        output.UnlockBits(outputData);
        //    }
        //    return output;
        //}
        //public static double GetGreyLevel(byte r, byte g, byte b)
        //{
        //    return (r * 0.299 + g * 0.587 + b * 0.114) / 255;
        //}
        public async Task PaperCut()
        {

            /*------------------------------/
            ESC:29
            t:86
            m Function
             65 Feeds paper to(cutting position + [n x basic calculated pitch]) and performs a full cut
             66 Feeds paper to(cutting position + [n x basic calculated pitch]) and performs a partial cut
            (one point uncut)
             67 Not Used
             68 Not Used
            n 0 >= n <= 255
            /-------------------------------*/


            /// Set bit-Image mode
            byte[] b = new byte[]
            {
                (byte)29,
                (byte)86,
                (byte)66,
                (byte)50
            };

            await Write(b, 100);

            //outputList.Add((byte)29);
            //outputList.Add((byte)86);
            //outputList.Add((byte)m);
            //outputList.Add((byte)n);
        }
    }
}
