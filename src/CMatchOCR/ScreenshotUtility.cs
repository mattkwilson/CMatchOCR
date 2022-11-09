using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;

namespace CMatchOCR
{
	/// <summary>
	/// A utility for taking screenshots of the desktop
	/// </summary>
	public static class ScreenshotUtility
	{
		/// <summary>
		/// Take a screenshot of the desktop at the size and position of screenRect
		/// </summary>
		/// <param name="screenRect">The size and position of the screenshot on the desktop</param>
		/// <returns>The screenshot as a Bitmap image</returns>
		public static Bitmap Screenshot(Rectangle screenRect)
		{
			Bitmap bitmap = new Bitmap(screenRect.Width, screenRect.Height);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, screenRect.Size);
			}

			return bitmap;
		}

		/// <summary>
		/// Take a screenshot of the desktop at the size and position of screenRect
		/// </summary>
		/// <param name="screenRect">The size and position of the screenshot on the desktop</param>
		/// <returns>The a raw uncompressed screenshot as a SoftwareBitamp</returns>
		public static async Task<SoftwareBitmap> ScreenshotRaw(Rectangle screenRect)
		{
			Bitmap bitmap = new Bitmap(screenRect.Width, screenRect.Height);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, screenRect.Size);
			}

			SoftwareBitmap softwareBitmap = null;
			using (Stream stream = new MemoryStream())
			{
				bitmap.Save(stream, ImageFormat.Png);
				BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
				softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
			}

			if (softwareBitmap == null)
			{
				throw new NullReferenceException("Error taking screenshot: softwareBitmap is null");
			}
			return softwareBitmap;

		}

	}
}