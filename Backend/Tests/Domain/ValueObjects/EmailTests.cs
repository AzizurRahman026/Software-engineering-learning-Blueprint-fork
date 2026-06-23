using Domain.Exceptions;
using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects;

// First real unit tests in the project: they pin down the behavior of the
// Email value object built on Day 3 (Create / TryCreate / value equality).
public class EmailTests
{
    [Fact]
    public void Create_WithValidInput_SetsValue()
    {
        var email = Email.Create("aziz@example.com");

        Assert.Equal("aziz@example.com", email.Value);
    }

    [Theory]
    [InlineData("  Aziz@Example.COM  ", "aziz@example.com")] // trimmed + lowercased
    [InlineData("USER@Domain.Io", "user@domain.io")]
    public void Create_NormalizesInput(string input, string expected)
    {
        var email = Email.Create(input);

        Assert.Equal(expected, email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankInput_ThrowsValidationException(string? input)
    {
        Assert.Throws<ValidationException>(() => Email.Create(input!));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@no-local.com")]
    [InlineData("two@@ats.com")]
    [InlineData("spaces in@email.com")]
    public void Create_WithMalformedInput_ThrowsValidationException(string input)
    {
        Assert.Throws<ValidationException>(() => Email.Create(input));
    }

    [Fact]
    public void TryCreate_WithValidInput_ReturnsTrueAndEmail()
    {
        var ok = Email.TryCreate("aziz@example.com", out var email);

        Assert.True(ok);
        Assert.NotNull(email);
        Assert.Equal("aziz@example.com", email!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("nope")]
    public void TryCreate_WithInvalidInput_ReturnsFalseAndNull(string? input)
    {
        var ok = Email.TryCreate(input, out var email);

        Assert.False(ok);
        Assert.Null(email);
    }

    [Fact]
    public void Equality_TwoEmailsWithSameNormalizedValue_AreEqual()
    {
        var a = Email.Create("Aziz@Example.com");
        var b = Email.Create("  aziz@example.com  ");

        // record value equality: equal by Value, not by reference.
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentEmails_AreNotEqual()
    {
        var a = Email.Create("aziz@example.com");
        var b = Email.Create("someone@example.com");

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }
}
