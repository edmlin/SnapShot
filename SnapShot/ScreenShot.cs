using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnapShot
{
    static internal class ScreenShot
    {
        static ImageCodecInfo imageCodecInfo = GetEncoderInfo("image/jpeg");

        public static long Quality { get; private set; } = 80L;

        static ScreenShot()
        {
        }

        public static void Take(string fileName, long quality=80L)
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;
            EncoderParameter qualParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualParam;

            // Create a bitmap of the appropriate size to receive the screenshot.
            using (Bitmap bmp = new Bitmap(screenWidth, screenHeight))
            {
                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bmp.Size);
                }

                // Do something with the Bitmap here, like save it to a file:
                bmp.Save(fileName + ".jpg", imageCodecInfo, encoderParams);
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
