// Validators/UploadFileDtoValidator.cs
using ACommerce.Files.AspNetCore.DTOs.Upload;
using FluentValidation;

namespace ACommerce.Files.AspNetCore.Validators;

public class UploadFileDtoValidator : AbstractValidator<UploadFileDto>
{
	public UploadFileDtoValidator()
	{
		RuleFor(x => x.File)
			.NotNull()
			.WithMessage("File is required");

		RuleFor(x => x.File.Length)
			.LessThanOrEqualTo(10 * 1024 * 1024) // 10 MB
			.WithMessage("File size must not exceed 10 MB")
			.When(x => x.File != null);

		RuleFor(x => x.File.ContentType)
			.Must(BeValidContentType)
			.WithMessage("Invalid file type")
			.When(x => x.File != null);
	}

	private bool BeValidContentType(string contentType)
	{
		var allowedTypes = new[]
		{
			"image/jpeg",
			"image/jpg",
			"image/png",
			"image/gif",
			"image/webp",
			"application/pdf",
			"application/msword",
			"application/vnd.openxmlformats-officedocument.wordprocessingml.document"
		};

		return allowedTypes.Contains(contentType.ToLowerInvariant());
	}
}

