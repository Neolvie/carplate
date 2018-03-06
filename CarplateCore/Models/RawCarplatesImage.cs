using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace CarplateCore.Models
{
  public class RawCarplatesImage
  {
    public Bitmap Image { get; set; }

    public List<Carplate> DetectCarplates()
    {
      if (Image == null)
        throw new ArgumentException("Image not set");

      var rectangles = Processors.PlateDetector.DetectCarplates(Image);

      return rectangles.Select(x =>
      {
        var image = Image.Clone(x, PixelFormat.DontCare);
        return new Carplate(image);
      }).ToList();
    }

    public RawCarplatesImage(Bitmap image)
    {
      Image = image;
    }
  }
}