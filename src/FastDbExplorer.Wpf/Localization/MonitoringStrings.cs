using System.Globalization;

namespace FastDbExplorer.Wpf.Localization;

/// <summary>All user-facing text of the monitoring (پایش و هشدار) screens.</summary>
public static class MonitoringStrings
{
    public const string Title = "پایش و هشدار";
    public const string Subtitle = "شرط‌ها را تعریف کنید؛ برنامه در بازه‌های زمانی انتخابی، فقط رکوردهای جدید را (به‌صورت فقط‌خواندنی) بررسی می‌کند.";
    public const string OpenMonitoring = "پایش";
    public const string NewMonitor = "پایش جدید";
    public const string Edit = "ویرایش";
    public const string Delete = "حذف";
    public const string ConfirmDeleteQuestion = "حذف شود؟";
    public const string Yes = "بله";
    public const string No = "خیر";
    public const string CheckNow = "بررسی اکنون";
    public const string Enabled = "فعال";
    public const string NoMonitors = "هنوز پایشی تعریف نشده است. با «پایش جدید» شروع کنید.";
    public const string Alerts = "هشدارها";
    public const string NoAlerts = "هنوز هشداری ثبت نشده است.";
    public const string OpenReport = "باز کردن گزارش";
    public const string ShowInFolder = "نمایش در پوشه";
    public const string Close = "بستن";
    public const string RunningNote = "بررسی‌ها فقط تا وقتی که برنامه باز و به سرور متصل است انجام می‌شوند. هیچ دستور تغییری روی پایگاه‌داده اجرا نمی‌شود.";
    public const string NeverChecked = "هنوز بررسی نشده";
    public const string FirstCheckNote = "در اولین بررسی فقط نقطه‌ی شروع ثبت می‌شود.";
    public const string BaselineSet = "نقطه‌ی شروع ثبت شد؛ از این پس رکوردهای جدید بررسی می‌شوند.";
    public const string NoNewMatches = "رکورد جدیدِ مطابق شرط نبود.";
    public const string Checking = "در حال بررسی…";

    public const string EditorTitleNew = "پایش جدید";
    public const string EditorTitleEdit = "ویرایش پایش";
    public const string Name = "نام پایش";
    public const string Database = "پایگاه‌داده";
    public const string Table = "جدول";
    public const string Target = "جدول موردنظر";
    public const string WatermarkColumn = "ستون تشخیص رکورد جدید";
    public const string WatermarkHint = "ستونی با مقدار افزایشی (شناسه یا تاریخ ثبت/تغییر) که برای پیداکردن رکوردهای جدید از آخرین بررسی استفاده می‌شود. همان نام ستون در هر جدول ممکن است متفاوت باشد؛ از فهرست انتخاب کنید.";
    public const string Conditions = "شرط‌ها (مثلاً مکان، مبدا، مقصد)";
    public const string AddCondition = "+ افزودن شرط";
    public const string Interval = "بازه‌ی بررسی (دقیقه)";
    public const string IntervalHint = "مثلاً 60 = هر یک ساعت، 120 = هر دو ساعت";
    public const string OutputFolder = "پوشه‌ی ذخیره‌ی گزارش Excel";
    public const string Browse = "انتخاب پوشه…";
    public const string PlaySound = "پخش صدا هنگام هشدار";
    public const string Save = "ذخیره";
    public const string PickFolderTitle = "انتخاب پوشه‌ی ذخیره‌ی گزارش";

    public const string ErrName = "برای پایش یک نام وارد کنید.";
    public const string ErrTarget = "پایگاه‌داده و جدول را انتخاب کنید.";
    public const string ErrWatermark = "ستون تشخیص رکورد جدید را انتخاب کنید (ستون عددی یا تاریخ).";
    public const string ErrNoConditions = "حداقل یک شرط تعریف کنید.";
    public const string ErrInterval = "بازه‌ی بررسی باید عددی بین 1 تا 1440 دقیقه باشد.";
    public const string ErrFolder = "مسیر پوشه‌ی گزارش را کامل وارد کنید (مثلاً D:\\Reports).";
    public const string ErrFolderCreate = "ایجاد یا دسترسی به این پوشه ممکن نیست: ";
    public const string ErrOpenFile = "باز کردن فایل ممکن نشد: ";

    public static string EveryMinutes(int minutes) =>
        minutes >= 60 && minutes % 60 == 0 ? $"هر {minutes / 60} ساعت" : $"هر {minutes} دقیقه";

    public static string LastChecked(DateTime utc) =>
        "آخرین بررسی: " + utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    public static string TimeText(DateTime utc) =>
        utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    public static string Matched(int count) => $"{count:N0} رکورد مطابق شرط پیدا شد.";

    public static string AlertSummary(int count, bool truncated) =>
        truncated
            ? $"{count:N0} رکورد جدید مطابق شرط (فقط اولین دسته در گزارش آمد؛ بقیه در بررسی بعدی)"
            : $"{count:N0} رکورد جدید مطابق شرط";

    public static string ToastTitle(string monitorName) => $"هشدار: {monitorName}";

    public static string CheckFailed(string reason) => $"بررسی ناموفق بود: {reason}";
}
