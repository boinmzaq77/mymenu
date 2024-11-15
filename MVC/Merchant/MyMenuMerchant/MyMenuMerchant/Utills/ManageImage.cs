using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Drawing.Imaging;

namespace MyMenuMerchant.Utills
{
    public static class ManageImage
    {
        public static MemoryStream Resize(IFormFile originImage, System.Drawing.Size size)
        {

            try
            {
                using (var fileStream = originImage.OpenReadStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        using (var img = System.Drawing.Image.FromStream(memoryStream))
                        {
                            var ratioH = (double)size.Height / img.Height;
                            var ratioW = (double)size.Width / img.Width;
                            var ratio = ratioH < ratioW ? ratioH : ratioW;

                            var newWidth = (int)(img.Width * ratio);
                            var newHeight = (int)(img.Height * ratio);
                            var newImage = new Bitmap(newWidth, newHeight);

                            using (var g = Graphics.FromImage(newImage))
                            {
                                g.DrawImage(img, 0, 0, newWidth, newHeight);
                            }

                           
                            MemoryStream newMemoryStream = new MemoryStream();
                            newImage.Save(newMemoryStream, System.Drawing.Imaging.ImageFormat.Png);

                            newMemoryStream.Position = 0;

                            return newMemoryStream;

                            //var streamContent = new StreamContent(newMemoryStream);
                            //streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                            ////newMemoryStream.Dispose();
                            //return streamContent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
