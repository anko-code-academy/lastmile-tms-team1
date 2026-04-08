using FluentValidation;

namespace LastMile.TMS.Application.Features.ParcelRegistration.Commands.CreateParcel;

public class CreateParcelCommandValidator : AbstractValidator<CreateParcelCommand>
{
    public CreateParcelCommandValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.ServiceType)
            .IsInEnum().WithMessage("Service type is required.");

        // Shipper Address
        RuleFor(x => x.ShipperAddress.Street1)
            .NotEmpty().WithMessage("Sender street address is required.")
            .MaximumLength(200).WithMessage("Sender street address must not exceed 200 characters.");

        RuleFor(x => x.ShipperAddress.City)
            .NotEmpty().WithMessage("Sender city is required.")
            .MaximumLength(100).WithMessage("Sender city must not exceed 100 characters.");

        RuleFor(x => x.ShipperAddress.State)
            .NotEmpty().WithMessage("Sender state is required.")
            .MaximumLength(100).WithMessage("Sender state must not exceed 100 characters.");

        RuleFor(x => x.ShipperAddress.PostalCode)
            .NotEmpty().WithMessage("Sender postal code is required.")
            .MaximumLength(20).WithMessage("Sender postal code must not exceed 20 characters.");

        RuleFor(x => x.ShipperAddress.CountryCode)
            .NotEmpty().WithMessage("Sender country code is required.")
            .MaximumLength(2).WithMessage("Sender country code must be 2 characters.");

        RuleFor(x => x.ShipperAddress.Email)
            .EmailAddress().WithMessage("Sender email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.ShipperAddress.Email));

        RuleFor(x => x.ShipperAddress.Phone)
            .Matches(@"^\+?[\d\s\-\(\)]{7,20}$").WithMessage("Sender phone must be a valid phone number.")
            .When(x => !string.IsNullOrWhiteSpace(x.ShipperAddress.Phone));

        // Recipient Address
        RuleFor(x => x.RecipientAddress.Street1)
            .NotEmpty().WithMessage("Recipient street address is required.")
            .MaximumLength(200).WithMessage("Recipient street address must not exceed 200 characters.");

        RuleFor(x => x.RecipientAddress.City)
            .NotEmpty().WithMessage("Recipient city is required.")
            .MaximumLength(100).WithMessage("Recipient city must not exceed 100 characters.");

        RuleFor(x => x.RecipientAddress.State)
            .NotEmpty().WithMessage("Recipient state is required.")
            .MaximumLength(100).WithMessage("Recipient state must not exceed 100 characters.");

        RuleFor(x => x.RecipientAddress.PostalCode)
            .NotEmpty().WithMessage("Recipient postal code is required.")
            .MaximumLength(20).WithMessage("Recipient postal code must not exceed 20 characters.");

        RuleFor(x => x.RecipientAddress.CountryCode)
            .NotEmpty().WithMessage("Recipient country code is required.")
            .MaximumLength(2).WithMessage("Recipient country code must be 2 characters.");

        RuleFor(x => x.RecipientAddress.Email)
            .EmailAddress().WithMessage("Recipient email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientAddress.Email));

        RuleFor(x => x.RecipientAddress.Phone)
            .Matches(@"^\+?[\d\s\-\(\)]{7,20}$").WithMessage("Recipient phone must be a valid phone number.")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientAddress.Phone));

        // Weight & Dimensions
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.");

        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");

        RuleFor(x => x.DeclaredValue)
            .GreaterThanOrEqualTo(0).WithMessage("Declared value must be 0 or greater.");

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency code must be 3 characters or less.");
    }
}
