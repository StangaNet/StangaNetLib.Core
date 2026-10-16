using FluentAssertions;
using Microsoft.Extensions.Options;
using StangaNetLib.Core.Validators;

namespace StangaNetLib.Core.Tests.Validators;

public class SettingsValidatorBaseTests
{
    private sealed class SampleOptions
    {
        public string Name { get; set; } = "";
        public int MaxItems { get; set; }
    }

    private sealed class SampleValidator : SettingsValidatorBase<SampleOptions>
    {
        protected override void ValidateCore(SampleOptions options, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(options.Name))
                errors.Add("Name is required.");

            if (options.MaxItems < 1)
                errors.Add("MaxItems must be at least 1.");
        }
    }

    [Fact]
    public void Validate_WhenValid_ShouldSucceed()
    {
        var validator = new SampleValidator();
        var result = validator.Validate(name: null, new SampleOptions { Name = "core", MaxItems = 3 });

        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Validate_WhenInvalid_ShouldFailWithAllMessages()
    {
        var validator = new SampleValidator();
        var result = validator.Validate(name: "Sample", new SampleOptions { Name = " ", MaxItems = 0 });

        result.Succeeded.Should().BeFalse();
        result.Failures.Should().BeEquivalentTo(
        [
            "Name is required.",
            "MaxItems must be at least 1."
        ]);
    }

    [Fact]
    public void Validate_ShouldImplementIValidateOptions()
    {
        IValidateOptions<SampleOptions> validator = new SampleValidator();
        validator.Should().NotBeNull();
    }
}
