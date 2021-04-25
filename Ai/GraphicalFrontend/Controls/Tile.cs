using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GraphicalFrontend.Controls
{
  [TemplatePart(Name = nameof(PART_Image), Type = typeof(Image))]
  internal class Tile : Control
  {
    static Tile()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));

      for (var i = 0; i < 136; i++)
      {
        var tileTypeId = i / 4;
        var index = tileTypeId % 9;
        var suit = "mpsj"[tileTypeId / 9];
        var resourceName = $"{suit}{index + 1}.png";
        var image = LoadImage(resourceName);
        ImagesByTileId[i] = image;
      }

      TileBack = LoadImage("j9.png");
    }

    public int TileId
    {
      get => (int) GetValue(TileIdProperty);
      set => SetValue(TileIdProperty, value);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      PART_Image = (Image) Template.FindName(nameof(PART_Image), this);
      UpdateImage();
    }

    private static readonly BitmapImage[] ImagesByTileId = new BitmapImage[136];
    private static readonly BitmapImage TileBack;

    public static readonly DependencyProperty TileIdProperty = DependencyProperty.Register(
      nameof(TileId), typeof(int), typeof(Tile), new PropertyMetadata(default(int), OnTileIdChanged));

    private Image? PART_Image;

    private static BitmapImage LoadImage(string resourceName)
    {
      var fullResourceName = "GraphicalFrontend.Resources.Tiles.Flat." + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      var image = new BitmapImage();
      image.BeginInit();
      image.StreamSource = stream;
      image.EndInit();
      image.Freeze();
      return image;
    }

    private static void OnTileIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var t = (Tile) d;
      t.UpdateImage();
    }

    private void UpdateImage()
    {
      if (PART_Image == null)
      {
        return;
      }

      if (TileId < 0 || TileId > 135)
      {
        PART_Image.Source = TileBack;
        return;
      }

      PART_Image.Source = ImagesByTileId[TileId];
    }
  }
}