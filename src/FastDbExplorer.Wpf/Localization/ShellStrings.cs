namespace FastDbExplorer.Wpf.Localization;

/// <summary>Text of the app shell: navigation rail, top bar, home dashboard, "connect first" prompts.</summary>
public static class ShellStrings
{
    public const string NavHome = "خانه";
    public const string NavExplorer = "کاوشگر";
    public const string NavMap = "نقشه";
    public const string NavMonitoring = "پایش";

    public const string NotConnected = "متصل نیستید";
    public const string ActiveDatabase = "پایگاه‌داده‌ی فعال";

    public const string ExplorerPromptTitle = "برای کاوش در جدول‌ها ابتدا به سرور وصل شوید";
    public const string ExplorerPromptBody = "پس از اتصال، پایگاه‌داده‌ها و جدول‌ها همین‌جا نمایش داده می‌شوند. نقشه بدون اتصال هم کار می‌کند.";
    public const string MonitoringPromptTitle = "برای پایش داده‌های جدید ابتدا به سرور وصل شوید";
    public const string MonitoringPromptBody = "پایش‌ها تا وقتی به سرور وصل هستید و برنامه باز است، در پس‌زمینه اجرا می‌شوند.";
    public const string GoToConnect = "رفتن به اتصال";

    public const string HomeWelcome = "خوش آمدید";
    public const string HomeSubtitle = "کاوش داده‌ها، نقشه و پایش هشدارها، همه در یک‌جا.";
    public const string ExplorerTileHint = "جدول‌ها را ببینید و روی سرور فیلتر کنید.";
    public const string MapTileHint = "نقشه‌ها و لایه‌های مختصات را باز کنید.";
    public const string MonitoringTileHint = "ردیف‌های جدید را زیر نظر بگیرید و هشدار بگیرید.";
    public const string LatestAlerts = "آخرین هشدارها";
    public const string NoAlertsYet = "هنوز هشداری ثبت نشده است.";
    public const string AllAlerts = "همه‌ی هشدارها";

    public const string SearchButton = "جستجو در جدول‌ها، پایش‌ها و بخش‌ها…";
    public const string PaletteHint = "جستجوی جدول، پایش یا بخش…";
    public const string PaletteEmpty = "نتیجه‌ای یافت نشد.";
    public const string KindSection = "بخش";
    public const string KindTable = "جدول";
    public const string KindMonitor = "پایش";
    public const string KindAction = "دستور";

    public static string RowsApprox(object rows) => $"حدود {rows:N0} ردیف";

    public static string WithShortcut(string title, int number) => $"{title} (Ctrl+{number})";

    public static string DatabasesCount(int count) => $"{count:N0} پایگاه‌داده";

    public static string MonitorsCount(int count) =>
        count == 0 ? "هنوز پایشی تعریف نشده است." : $"{count:N0} پایش تعریف شده است.";
}
