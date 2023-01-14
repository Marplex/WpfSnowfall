using System;
using System.Windows.Controls;

namespace WpfSnowfall.Snowflakes;

public partial class Snowflake : UserControl
{
    private const int _totalVariants = 2;
    public static UserControl Generate(int? variant = null)
    {
        var random = variant ?? new Random().Next(0, _totalVariants);
        return random switch
        {
            0 => new Snowflake1(),
            1 => new Snowflake2(),
            _ => new Snowflake1(),
        };
    }
}
