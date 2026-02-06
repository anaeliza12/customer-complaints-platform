namespace ApiChamados.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(List<string> errors)
            : base("Um ou mais erros de validação ocorreram.")
        {
            Errors = errors;
        }
    }
}