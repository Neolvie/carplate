using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarplateCore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarplateCore.Tests
{
  [TestClass]
  public class CarplatesDetect
  {
    private static readonly string ImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testimages");
    private static readonly string ModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessmodels");

    [TestMethod]
    public void FindContours()
    {
      var result = new List<Task>();
      foreach (var file in Directory.GetFiles(ImagePath))
      {
        Task.Run(() => ProcessFile(file)).Wait();
      }

    }

    private void ProcessFile(string file)
    {
      var path = Path.Combine(ImagePath, file);
      var result = new List<string>();
      var resultFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp", $"{Path.GetFileNameWithoutExtension(file)}");
      if (!Directory.Exists(resultFolder))
        Directory.CreateDirectory(resultFolder);

      using (var image = new Bitmap(path))
      {
        var rawCarplateImage = new RawCarplatesImage(image);
        var carplates = rawCarplateImage.DetectCarplates();

        foreach (var carplate in carplates)
        {
          carplate.Image.Save(Path.Combine(resultFolder, $"{Environment.TickCount}.bmp"));
          carplate.RecognizeNumber(ModelPath);
          if (string.IsNullOrEmpty(carplate.RawNumber))
            continue;

          var res = $"Text: {carplate.RawNumber}, confidence: {carplate.Confidence}";
          result.Add(res);
        }
      }

      if (result.Any())
      {
        var savePath = Path.Combine(resultFolder, "result.txt");
        File.WriteAllLines(savePath, result);
      }
    }
  }
}
