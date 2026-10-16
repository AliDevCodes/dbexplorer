namespace FastDbExplorer.Wpf.Localization;

/// <summary>Texts for the optional encrypted SQL-password storage (separate file to keep this change self-contained).</summary>
public static class CredentialStrings
{
    public const string RememberPassword = "رمز عبور را به خاطر بسپار";
    public const string RememberPasswordNote = "رمز فقط روی همین رایانه و برای همین کاربر ویندوز، با رمزنگاری ویندوز (DPAPI) نگه‌داری می‌شود و هرگز در فایل پروفایل‌ها نوشته نمی‌شود.";
    public const string PasswordPromptNote = "اگر «رمز عبور را به خاطر بسپار» خاموش باشد، رمز SQL در هر اتصال پرسیده می‌شود و فقط در حافظه می‌ماند.";
    public const string PasswordStorageNote = "رمزها هرگز در فایل پروفایل‌ها ذخیره نمی‌شوند. نگه‌داری رمز اختیاری است و جداگانه و رمزگذاری‌شده انجام می‌شود.";
    public const string RememberedTooltip = "رمز عبور این اتصال به‌صورت رمزگذاری‌شده نگه‌داری می‌شود";
    public const string StoredPasswordRejected = "رمز ذخیره‌شده پذیرفته نشد و پاک شد؛ در اتصال بعدی رمز جدید را وارد کنید.";
}
