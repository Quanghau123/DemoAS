using FluentValidation;

using DemoEF.Application.DTOs.User;

namespace DemoEF.Application.Validation.User
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
