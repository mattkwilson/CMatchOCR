using System;
using System.Drawing;
using System.Linq;
using Windows.Foundation;
using CMatchOCR;
using NUnit.Framework;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CMatchOCRTest
{
    public class ScreenTextFinderTest
    {
        private string screenshotFilePath;
        private ScreenTextFinder screenTextFinder;

        [SetUp]
        public void Setup()
        {
            screenTextFinder = new ScreenTextFinder();
            screenshotFilePath = AppContext.BaseDirectory.Substring(0,
                AppContext.BaseDirectory.IndexOf("bin", StringComparison.Ordinal));
            screenshotFilePath += "Screenshots\\screenshot.png";
        }

        [Test]
        public async Task FindTest()
        {

            var desktopBounds = Screen.PrimaryScreen.Bounds;
            var result = await screenTextFinder.FindAll("bitmap", desktopBounds);

            var enumerable = result as Rect[] ?? result.ToArray();
            
            if (!enumerable.Any())
                Assert.Fail();
          
            // take a screenshot and highlight the results
            var bitmap = ScreenshotUtility.Screenshot(desktopBounds);

            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var rect in enumerable)
                {
                    var r = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                    g.DrawRectangle(new Pen(Color.Red), r);
                }
            }
            bitmap.Save(screenshotFilePath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}