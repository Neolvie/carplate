using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Point = OpenCvSharp.Point;

namespace CarplateCore.Processors
{
  public static class PlateDetector
  {
    public static List<Rectangle> DetectCarplates(Bitmap image)
    {
      var result = new List<Rectangle>();

      var original = BitmapConverter.ToMat(image);
      var gray = image.ToGrayscaleMat();
      var src = new Mat();

      gray.CopyTo(src);    
      var threshImage = new Mat();
      Cv2.AdaptiveThreshold(gray, threshImage, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 9, 9);
      SaveImage(threshImage, "threshhold");

      Point[][] contours;
      HierarchyIndex[] hierarchyIndexes;
      Cv2.FindContours(threshImage, out contours, out hierarchyIndexes, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

      if (contours.Length == 0)
        return new List<Rectangle>();

      var sorted = contours
        .OrderByDescending(x => Cv2.ContourArea(x))
        .Take(100)
        .Where(x =>
        {          
          var rect = Cv2.BoundingRect(x);
          return rect.IsHorizontalCarplateBlock() && GetMeanColor(gray, rect)[0] > 135;
        })
        .ToList();

      foreach (var contour in sorted)
      {
        var boundingRect = Cv2.BoundingRect(contour);

        result.Add(boundingRect.ToRectangle());
        var meanColor = GetMeanColor(gray, boundingRect);

        Cv2.Rectangle(original,
          new Point(boundingRect.X, boundingRect.Y),
          new Point(boundingRect.X + boundingRect.Width, boundingRect.Y + boundingRect.Height),
          new Scalar(0, 0, 255), 2);

        Cv2.PutText(original, meanColor[0].ToString(), new Point(boundingRect.X + 10, boundingRect.Y + 10), HersheyFonts.HersheyPlain, 0.75, Scalar.Red);
      }

      SaveImage(original, "detected");

      return result;
    }

    private static Rectangle ToRectangle(this Rect rect)
    {
      return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    private static Scalar GetMeanColor(Mat img, Rect rect)
    {
      var region = img.Clone(rect);
      return Cv2.Mean(region);
    }

    private static void SaveImage(Mat mat, string name)
    {
      var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      path = Path.Combine(path, $"{Environment.TickCount}_{name}.bmp");
      
      using (var img = BitmapConverter.ToBitmap(mat))
      {
        img.Save(path);
      }
    }

    private static bool IsHorizontalCarplateBlock(this OpenCvSharp.Rect rect, double minRatio = 2.5, double maxRation = 4.5)
    {
      return (rect.Width > rect.Height * minRatio) && (rect.Width < rect.Height * maxRation); 
    }

    private static Mat ToGrayscaleMat(this Bitmap image)
    {
      var src = BitmapConverter.ToMat(image);
      var gray = new Mat();

      if (src.Channels() == 3)
      {
        Cv2.CvtColor(src, gray, ColorConversionCodes.RGB2GRAY);
      }
      else
      {
        gray = src;
      }

      return gray;
    }
  }
}