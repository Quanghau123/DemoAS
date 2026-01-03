using FluentValidation;

using DemoEF.Application.DTOs.User;

namespace DemoEF.Application.Validation.User
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.UserRole)
                .Must(BeValidRole)
                .When(x => !string.IsNullOrWhiteSpace(x.UserRole))
                .WithMessage("Invalid user role");
        }

        private bool BeValidRole(string role)
            => Enum.TryParse<
                DemoEF.Domain.Enums.User.UserRole>(role, true, out _);
    }
}
