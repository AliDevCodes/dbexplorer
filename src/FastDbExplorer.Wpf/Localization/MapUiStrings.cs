namespace FastDbExplorer.Wpf.Localization;

/// <summary>Texts of the redesigned map panel and the marker (icon/colour) picker.</summary>
public static class MapUiStrings
{
    public const string AddExcelLayer = "افزودن لایه Excel";
    public const string ShowAll = "نمایش همه";
    public const string HideAll = "پنهان‌کردن همه";
    public const string NoMapLayers = "این نقشه لایهٔ جداگانه‌ای برای کنترل ندارد.";
    public const string EmptyExcelTitle = "هنوز لایه‌ای اضافه نشده";
    public const string EmptyExcelBody = "یک فایل Excel شامل نام و مختصات (عرض و طول جغرافیایی) اضافه کنید تا نقاط روی نقشه نمایش داده شوند.";
    public const string LayerSettings = "تنظیمات لایه";
    public const string ToggleVisibility = "نمایش یا پنهان‌کردن لایه";
    public const string MarkerLabel = "نماد روی نقشه";
    public const string ColorLabel = "رنگ";
    public const string DefaultMarker = "پیش‌فرض (نقطه ساده)";

    public static string TabExcel(int count) => $"نقاط Excel ({count})";

    public static string TabMap(int count) => $"لایه‌های نقشه ({count})";

    public static string IconName(string key) => key switch
    {
        "pin" => "نشانگر مکان",
        "dot" => "دایره",
        "target" => "هدف",
        "flag" => "پرچم",
        "star" => "ستاره",
        "heart" => "علاقه‌مندی",
        "home" => "خانه",
        "building" => "ساختمان",
        "bank" => "بانک",
        "factory" => "کارخانه",
        "hospital" => "درمانی",
        "school" => "آموزشی",
        "mosque" => "مسجد",
        "bolt" => "برق",
        "drop" => "آب",
        "warning" => "هشدار",
        "shield" => "امنیتی",
        "camera" => "دوربین",
        "tower" => "دکل",
        "plane" => "فرودگاه",
        "ship" => "بندر",
        "bus" => "حمل‌ونقل",
        "shop" => "فروشگاه",
        "person" => "فرد",
        _ => key
    };
}
