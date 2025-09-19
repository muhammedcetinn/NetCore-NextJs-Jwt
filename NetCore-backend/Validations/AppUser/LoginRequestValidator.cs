using FluentValidation;
using NetCore_backend.DTOs;

namespace NetCore_backend.Validations.AppUser
{
    public class LoginRequestValidator : AbstractValidator<AuthDtos.LoginRequest>
    {
        public LoginRequestValidator() {
            RuleFor(r => r.Email).EmailAddress().WithMessage("Email formatı doğru değil!")
                .NotNull().WithMessage("Email boş olamaz!");


            RuleFor(r => r.Password)
                .NotNull().WithMessage("Şifre boş olamaz!");
        }
    }
}
