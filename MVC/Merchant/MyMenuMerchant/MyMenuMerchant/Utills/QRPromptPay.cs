//using System.Drawing;
//using System.Text.RegularExpressions;
//using System.Text;
//using ZXing;
//using ZXing.Common;
//using QRCoder;

//namespace MyMenuMerchant.Utills
//{
//    public class QRPromptPay
//    {
//        private QRPromptPayModel[] Values;
//        public QRPromptPay()
//        {
//            InitializeValues();
//        }

//        private void InitializeValues()
//        {
//            try
//            {
//                Values = new QRPromptPayModel[7];
//                Values[0] = new QRPromptPayModel()
//                {
//                    Field = "00",
//                    Length = "02",
//                    Data = "01"
//                    //หมายเลขเวอร์ชั่น (Version Number): ฟิลด์ 00 ความยาว 02, ข้อมูล "01" หมายถึงเวอร์ชั่นของมาตรฐาน QR Code นี้คือเวอร์ชั่น 1.
//                };

//                Values[1] = new QRPromptPayModel()
//                {
//                    Field = "01",
//                    Length = "02",
//                    Data = "11"
//                    //ประเภทของ QR (QR Type): ฟิลด์ 01 ความยาว 02, ข้อมูล "11" หมายถึง QR นี้สร้างขึ้นเพื่อใช้สำหรับการขายหลายครั้ง ถ้าเป็น 12 คือ QR สร้างขึ้นเพื่อใช้ครั้งเดียว. 
//                };

//                Values[2] = new QRPromptPayModel()
//                {
//                    Field = "29",
//                    Length = "37",
//                    Data = ""
//                    //ข้อมูลผู้ขาย(Merchant Account Information):
//                    //ฟิลด์ 29 ความยาว 37
//                };

//                Values[3] = new QRPromptPayModel()
//                {
//                    Field = "58",
//                    Length = "02",
//                    Data = "TH"
//                    //ประเทศ (Country Code): ฟิลด์ 58 ความยาว 02, ข้อมูล "TH" หมายถึงประเทศไทย.
//                };

//                Values[4] = new QRPromptPayModel()
//                {
//                    Field = "53",
//                    Length = "03",
//                    Data = "764"
//                    //สกุลเงิน (Currency): ฟิลด์ 53 ความยาว 03, ข้อมูล "764" หมายถึงสกุลเงินบาทตาม ISO 4217.
//                };

//                Values[5] = new QRPromptPayModel()
//                {
//                    Field = "54",
//                    Length = "00",
//                    Data = "000"
//                    //ยอดเงิน
//                };

//                Values[6] = new QRPromptPayModel()
//                {
//                    Field = "63",
//                    Length = "04",
//                    Data = ""
//                    //ค่า Checksum: ฟิลด์ 63 ความยาว 04, ค่านี้เป็นผลลัพธ์จากการคำนวณ CRC-16 โดยใช้ polynomial 0x1021 (XMODEM) และค่าเริ่มต้น 0xFFFF.
//                };
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }

//        public void SetPromptPayID(string promptPayID)
//        {
//            try
//            {
//                string QRPromptPay = "";
//                promptPayID = Regex.Replace(promptPayID, @"[^0-9]+", "");
//                if (promptPayID.Length == 10)
//                {
//                    QRPromptPay = new QRPromptPayModel()
//                    {
//                        Field = "01",
//                        Length = "13",
//                        Data = "0066" + promptPayID.Substring(1)
//                        //หมายเลขบัญชี: ฟิลด์ย่อย 01 ความยาว 13,
//                        //ข้อมูล "01130066000000000" หมายถึงหมายเลขโทรศัพท์ 000 - 000 - 0000 ในรูปแบบ PromptPay.
//                    }.ToString();
//                }
//                else if (promptPayID.Length == 13)
//                {
//                    QRPromptPay = new QRPromptPayModel()
//                    {
//                        Field = "02",
//                        Length = "13",
//                        Data = promptPayID
//                        //02 หมายเลขบัตรประชาชนไม่มีขีดคั่น
//                    }.ToString();
//                }
//                else
//                {
//                    throw new Exception();
//                }

//                string AID = new QRPromptPayModel()
//                {
//                    Field = "00",
//                    Length = "16",
//                    Data = "A000000677010111"
//                    //หมายเลขแอปพลิเคชั่น(AID): ฟิลด์ย่อย 00 ความยาว 16,
//                    //ข้อมูล "A000000677010111" หมายถึง PromptPay.
//                }.ToString();

//                Values[2] = new QRPromptPayModel()
//                {
//                    Field = "29",
//                    Length = "37",
//                    Data = AID + QRPromptPay
//                };

//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }

//        }

//        public string Payment(decimal amount, bool isOneTimeUse = false)
//        {
//            var valuesCopy = (QRPromptPayModel[])Values.Clone();
//            UpdateAmount(valuesCopy, amount);
//            UpdateQRType(valuesCopy, isOneTimeUse);

//            string data = SetArrayToStringFormat(valuesCopy);
//            string checksum = CRC16Calculator.ComputeChecksum(data);
//            UpdateChecksum(valuesCopy, checksum);

//            return SetArrayToStringFormat(valuesCopy);
//        }

//        public Image QRPayment(decimal amount, bool isOneTimeUse = false)
//        {
//            var data = Payment(amount, isOneTimeUse);

//            string logoPath = "logo.png";
//            string logoTopPath = "template.png";
//            var qrCodeWithLogo = GenerateQRCodeWithLogo(data, logoPath, logoTopPath);


//            //Bitmap qrCodeImage = GenerateQRCode(data);

//            //Bitmap qrCodeWithLogo = InsertLogo(qrCodeImage, logoPath, logoTopPath);

//            return qrCodeWithLogo;
//        }

//        public Image GenerateQRCodeWithLogo(string code, string logoPath, string templatePath)
//        {
//            // Generate QR Code
//            QRCodeGenerator qrGenerator = new QRCodeGenerator();
//            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.L);
//            QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
//            //Bitmap qrImage = qrCode.GetGraphic(20);
//            Bitmap qrImage = qrCode.GetGraphic(20, Color.Black, Color.White, false);

//            // Load and resize logo
//            Image logo = Image.FromFile(logoPath);
//            double logoAreaLimit = qrImage.Width * qrImage.Height * 0.0147;
//            double scale = Math.Sqrt(logoAreaLimit / (logo.Width * logo.Height));
//            logo = new Bitmap(logo, new Size((int)(logo.Width * scale), (int)(logo.Height * scale)));

//            // Center logo on QR code
//            using (Graphics g = Graphics.FromImage(qrImage))
//            {
//                int left = (qrImage.Width - logo.Width) / 2;
//                int top = (qrImage.Height - logo.Height) / 2;
//                g.DrawImage(logo, new Point(left, top));
//            }

//            // Load template and paste QR code onto it
//            int newQRWidth = 600;
//            int newQRHeight = 600;

//            Image templateImage = Image.FromFile(templatePath);
//            using (Graphics g = Graphics.FromImage(templateImage))
//            {
//                int left = (templateImage.Width - newQRWidth) / 2;

//                g.DrawImage(qrImage, new Rectangle(left, 418, newQRWidth, newQRHeight));

//            }

//            using (Graphics graphics = Graphics.FromImage(templateImage))
//            {
//                string text = "สแกน QR เพื่อชำระเงิน";
//                Font font = new Font("Arial", 50, FontStyle.Bold);
//                SizeF textSize = graphics.MeasureString(text, font);

//                int left = (templateImage.Width - (int)textSize.Width) / 2;
//                int top = (templateImage.Height + newQRHeight + 418 - (int)textSize.Height) / 2;

//                Point point = new Point(left, top);
//                SolidBrush brush = new SolidBrush(ColorTranslator.FromHtml("#00427A"));

//                graphics.DrawString(text, font, brush, point);

//            }

//            return templateImage;
//        }



//        private void UpdateAmount(QRPromptPayModel[] values, decimal amount)
//        {
//            string amountString = amount.ToString("0.00");
//            string length = amountString.Length.ToString("00");

//            values[5] = new QRPromptPayModel()
//            {
//                Field = "54",
//                Length = length,
//                Data = amountString
//                //ยอดเงิน
//            };
//        }

//        private void UpdateQRType(QRPromptPayModel[] values, bool isOneTimeUse)
//        {
//            string qrType = isOneTimeUse ? "12" : "11";
//            values[1] = new QRPromptPayModel()
//            {
//                Field = "01",
//                Length = "02",
//                Data = qrType
//                //ประเภทของ QR (QR Type): ฟิลด์ 01 ความยาว 02, ข้อมูล "11" หมายถึง QR นี้สร้างขึ้นเพื่อใช้สำหรับการขายหลายครั้ง ถ้าเป็น 12 คือ QR สร้างขึ้นเพื่อใช้ครั้งเดียว. 
//            };
//        }

//        private void UpdateChecksum(QRPromptPayModel[] values, string checksum)
//        {
//            values[6] = new QRPromptPayModel()
//            {
//                Field = "63",
//                Length = "04",
//                Data = checksum
//                //ค่า Checksum: ฟิลด์ 63 ความยาว 04, ค่านี้เป็นผลลัพธ์จากการคำนวณ CRC-16 โดยใช้ polynomial 0x1021 (XMODEM) และค่าเริ่มต้น 0xFFFF.
//            };
//        }

//        private string SetArrayToStringFormat(QRPromptPayModel[] models)
//        {
//            var result = new StringBuilder();
//            foreach (var model in models)
//            {
//                result.Append(model.ToString());
//            }
//            return result.ToString();
//        }
//    }

//    internal class QRPromptPayModel
//    {
//        public string Field { get; set; }
//        public string Length { get; set; }
//        public string Data { get; set; }

//        public override string ToString()
//        {
//            return $"{Field}{Length}{Data}";
//        }
//    }

//    public class CRC16Calculator
//    {
//        public static string ComputeChecksum(string StringData)
//        {

//            byte[] data = System.Text.Encoding.UTF8.GetBytes(StringData);

//            ushort poly = 0x1021; // Polynomial used in CRC-16/XMODEM
//            ushort crc = 0xFFFF; // Initial value

//            foreach (byte b in data)
//            {
//                crc ^= (ushort)(b << 8);
//                for (int i = 0; i < 8; i++)
//                {
//                    if ((crc & 0x8000) != 0)
//                    {
//                        crc = (ushort)((crc << 1) ^ poly);
//                    }
//                    else
//                    {
//                        crc <<= 1;
//                    }
//                }
//            }
//            return crc.ToString("X4");
//        }
//    }
//}
