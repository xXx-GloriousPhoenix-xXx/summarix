namespace Summarix.Presentation.Configurations;

public sealed class FileUploadSettings
{
    public const string SectionName = "FileUpload";
    public long MaxFileSizeInMb { get; set; } = 500;
    public long MaxFileSizeInBytes => MaxFileSizeInMb * 1024 * 1024;
}
