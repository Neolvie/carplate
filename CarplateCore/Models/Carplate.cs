using System;
using System.Drawing;
using Tesseract;

namespace CarplateCore.Models
{
  public class Carplate
  {
    public string RawNumber { get; set; }
    public Bitmap Image { get; set; }
    public bool Recognized { get; set; }
    public double Confidence { get; set; }

    public void RecognizeNumber(string modelPath)
    {
      if (Image == null || string.IsNullOrEmpty(modelPath))
        throw new ArgumentException("Image not set");

      using (var engine = new TesseractEngine(modelPath, "rus+eng", EngineMode.LstmOnly))
      {
        using (var page = engine.Process(Image))
        {
          RawNumber = page.GetText();
          Confidence = page.GetMeanConfidence();
          Recognized = true;
        }
      }
    }

    public Carplate(Bitmap image)
    {
      Image = image;
    }
  }
}