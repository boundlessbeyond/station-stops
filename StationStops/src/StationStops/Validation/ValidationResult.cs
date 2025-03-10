namespace StationStops.Validation;
internal class ValidationResult
{
    private ValidationResult(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Successful => new ValidationResult(null);
    public static ValidationResult Failure(string error) => new ValidationResult(error);

    public bool Success => ErrorMessage == null;
    public string? ErrorMessage { get; }
}
