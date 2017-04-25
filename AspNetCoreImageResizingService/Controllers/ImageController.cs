// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreImageResizingService
{
  public class ImageController : Controller
  {
    public async Task<IActionResult> Index(string url, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destinationWidth, int destinationHeight)
    {
      using (Image sourceImage = await this.LoadImageFromUrl(url))
      {
        if (sourceImage != null)
        {
          try
          {
            using (Image destinationImage = this.CropImage(sourceImage, sourceX, sourceY, sourceWidth, sourceHeight, destinationWidth, destinationHeight))
            {
              Stream outputStream = new MemoryStream();

              destinationImage.Save(outputStream, ImageFormat.Jpeg);
              outputStream.Seek(0, SeekOrigin.Begin);
              return this.File(outputStream, "image/png");
            }
          }

          catch
          {
            // Add error logging here
          }
        }
      }

      return this.NotFound();
    }

    private async Task<Image> LoadImageFromUrl(string url)
    {
      Image image = null;

      try
      {
        using (HttpClient httpClient = new HttpClient())
        using (HttpResponseMessage response = await httpClient.GetAsync(url))
        using (Stream inputStream = await response.Content.ReadAsStreamAsync())
        using (Bitmap temp = new Bitmap(inputStream))
          image = new Bitmap(temp);
      }

      catch
      {
        // Add error logging here
      }

      return image;
    }

    private Image CropImage(Image sourceImage, int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destinationWidth, int destinationHeight)
    {
      Image destinationImage = new Bitmap(destinationWidth, destinationHeight);

      using (Graphics g = Graphics.FromImage(destinationImage))
        g.DrawImage(
          sourceImage,
          new Rectangle(0, 0, destinationWidth, destinationHeight),
          new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
          GraphicsUnit.Pixel
        );

      return destinationImage;
    }
  }
}