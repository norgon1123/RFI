using HoloToolkit.Unity;

/// <summary>
/// Hold color palette info for planes
/// </summary>
public class ColorPalette : Singleton<ColorPalette>
{
    public enum PaletteTypes
    {
        Normal = 0x1,
        Protanopia = 0x2,
        Tritanopia = 0x4,
        Monochromacy = 0x8
    }

    private PaletteTypes paletteType;

    public PaletteTypes PaletteType
    {
        get
        {
            return paletteType;
        }

        set
        {
            paletteType = value;
        }
    }

    private void Start()
    {
        paletteType = PaletteTypes.Normal;
    }

    /// <summary>
    /// Set the default color scheme
    /// </summary>
    public void Normal()
    {
        PaletteType = PaletteTypes.Normal;
    }

    /// <summary>
    /// Set color palette to be Protanopia-friendly
    /// </summary>
    public void Protanopia()
    {
        PaletteType = PaletteTypes.Protanopia;
    }

    /// <summary>
    /// Set color palette to be Tritanopia-friendly
    /// </summary>
    public void Tritanopia()
    {
        PaletteType = PaletteTypes.Tritanopia;
    }

    /// <summary>
    /// Set the color palette to be Monochramatic-friendly
    /// </summary>
    public void Monochromacy()
    {
        PaletteType = PaletteTypes.Monochromacy;
    }
}
