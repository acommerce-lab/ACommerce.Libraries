namespace ACommerce.Files.Operations.Abstractions;

/// <summary>
/// نوع عملية الملف.
/// </summary>
public sealed class FileOperationType
{
    public string Value { get; }
    private FileOperationType(string value) => Value = value;

    public static readonly FileOperationType Upload   = new("upload");
    public static readonly FileOperationType Download = new("download");
    public static readonly FileOperationType Delete   = new("delete");
    public static readonly FileOperationType Move     = new("move");
    public static readonly FileOperationType Copy     = new("copy");

    public static FileOperationType Custom(string v) => new(v);
    public override string ToString() => Value;
    public static implicit operator string(FileOperationType t) => t.Value;
}

/// <summary>
/// حالة الملف.
/// </summary>
public sealed class FileStatus
{
    public string Value { get; }
    private FileStatus(string value) => Value = value;

    public static readonly FileStatus Pending = new("pending");
    public static readonly FileStatus Stored  = new("stored");
    public static readonly FileStatus Failed  = new("failed");
    public static readonly FileStatus Deleted = new("deleted");

    public static FileStatus Custom(string v) => new(v);
    public override string ToString() => Value;
    public static implicit operator string(FileStatus s) => s.Value;
}

/// <summary>
/// مفاتيح علامات عمليات الملفات.
/// </summary>
public static class FileTags
{
    public const string Provider   = "storage_provider";  // "aliyun", "gcs", "local"
    public const string Operation  = "file_operation";    // "upload", "download", "delete"
    public const string Directory  = "directory";         // "listings", "profiles"
    public const string FileName   = "file_name";
    public const string ContentType = "content_type";
    public const string SizeBytes  = "size_bytes";
    public const string Status     = "file_status";
    public const string Url        = "file_url";
    public const string Role       = "role";              // "uploader", "storage"
    public const string Reason     = "reason";
}

/// <summary>
/// هوية الطرف في عمليات الملفات.
/// </summary>
public sealed class FilePartyId
{
    public string Type { get; }
    public string Id { get; }
    public string FullId { get; }

    private FilePartyId(string type, string id)
    {
        Type = type; Id = id; FullId = $"{type}:{id}";
    }

    public static FilePartyId User(string userId) => new("User", userId);
    public static FilePartyId Storage(string providerName) => new("Storage", providerName);
    public static FilePartyId File(string filePath) => new("File", filePath);
    public static FilePartyId System => new("System", "");

    public override string ToString() => string.IsNullOrEmpty(Id) ? Type : FullId;
    public static implicit operator string(FilePartyId pid) => pid.ToString();
}

/// <summary>
/// طلب رفع ملف.
/// </summary>
public record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Directory = null,
    Guid? UploaderId = null,
    Dictionary<string, string>? Metadata = null);

/// <summary>
/// نتيجة رفع ملف.
/// </summary>
public record FileUploadOutcome(
    bool Succeeded,
    string? FilePath,
    string? PublicUrl,
    string? Error = null,
    long? StoredSize = null);
