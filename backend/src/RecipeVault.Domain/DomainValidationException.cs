namespace RecipeVault.Domain;

/// <summary>
/// Thrown when an operation would break one of the domain's rules.
/// The API turns this into a 400 response, so messages are written for API consumers.
/// </summary>
public sealed class DomainValidationException(string message) : Exception(message);
