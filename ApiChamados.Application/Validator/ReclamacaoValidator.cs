using FluentValidation;
using ApiChamados.Domain.Models;

namespace ApiChamados.Application.Validators
{
    public class ReclamacaoValidator : AbstractValidator<Reclamacao>
    {
        public ReclamacaoValidator()
        {
            RuleFor(x => x.CorrelationId)
                .NotEmpty().WithMessage("O CorrelationId é obrigatório para o rastreio da mensagem.");

            RuleFor(x => x.Texto)
                .NotEmpty().WithMessage("O texto da reclamação não pode estar vazio.")
                .MinimumLength(10).WithMessage("O texto da reclamação deve ter pelo menos 10 caracteres.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("O Status fornecido é inválido.");

            RuleFor(x => x.DataCriacao)
                .NotEmpty().WithMessage("A data de criação é obrigatória.");

            RuleFor(x => x.Cliente)
                .NotNull().WithMessage("Os dados do cliente são obrigatórios.");

            RuleSet("ClienteRules", () => {
                RuleFor(x => x.Cliente.Nome)
                    .NotEmpty().WithMessage("O nome do cliente é obrigatório.");

                RuleFor(x => x.Cliente.Cpf)
                    .NotEmpty().WithMessage("O CPF do cliente é obrigatório.")
                    .Matches(@"^\d{11}$").WithMessage("O CPF deve conter 11 dígitos numéricos.");

                RuleFor(x => x.Cliente.Email)
                    .NotEmpty().WithMessage("O e-mail do cliente é obrigatório.")
                    .EmailAddress().WithMessage("O formato do e-mail é inválido.");
            });

            RuleFor(x => x.PerfilCliente)
                .NotNull().WithMessage("O perfil do cliente (Data Mesh) é obrigatório para o processamento.");

            When(x => x.PerfilCliente != null, () => {
                RuleFor(x => x.PerfilCliente.Cpf)
                    .NotEmpty().WithMessage("O CPF no perfil do cliente é obrigatório.");

                RuleFor(x => x.PerfilCliente.ScoreCredito)
                    .NotEmpty().WithMessage("O Score de Credito do cliente deve ser informado.");

                RuleFor(x => x.PerfilCliente.Produtos)
                    .NotNull().WithMessage("A lista de produtos do perfil não pode ser nula.");
            });

            RuleFor(x => x.Categorias)
                .NotNull().WithMessage("A lista de categorias não pode ser nula.")
                .Must(c => c.Any()).WithMessage("A reclamação deve ser classificada em pelo menos uma categoria.");

            RuleFor(x => x.Anexos)
                .NotNull().WithMessage("A lista de anexos não pode ser nula.");
        }
    }
}