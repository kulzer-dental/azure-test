using System.Globalization;

namespace KDC.Main.Services;

public class CultureScope : IDisposable
{
    private readonly CultureInfo _previousCulture;
    private readonly CultureInfo _previousUICulture;

    public CultureScope(CultureInfo culture)
    {
        _previousCulture = Thread.CurrentThread.CurrentCulture;
        _previousUICulture = Thread.CurrentThread.CurrentUICulture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public void Dispose()
    {
        Thread.CurrentThread.CurrentCulture = _previousCulture;
        Thread.CurrentThread.CurrentUICulture = _previousUICulture;
    }
}