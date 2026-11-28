using System.ComponentModel.DataAnnotations;
using _31241023037_VoTanQuy.Models;

namespace _31241023037_VoTanQuy.Validation;

public class CheckDateGreaterThanTodayAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        if (value is not DateTime date)
        {
            return new ValidationResult(ErrorMessage ?? "Ngày phát hành không hợp lệ.");
        }

        // Existing films naturally have historical release dates. Keep the
        // future-date rule for new records, but do not block editing seeded/existing films.
        var movie = validationContext.ObjectInstance as Movie;
        if (date.Date >= DateTime.Today || (movie?.ID ?? 0) > 0)
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(
            ErrorMessage ?? "Với phim mới, ngày phát hành phải từ hôm nay trở đi.");
    }
}
