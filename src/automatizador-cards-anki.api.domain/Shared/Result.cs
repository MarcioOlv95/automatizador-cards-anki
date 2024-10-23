namespace automatizador_cards_anki.api.domain.Shared;

public class Result
{
    public Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public Result(List<string> erros)
    {
        Errors = erros;
        IsSuccess = false;
    }

    public Result(string erro)
    {
        Errors.Add(erro);
        IsSuccess = false;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; } = new List<string>();

    public static Result Success() => new(true);
    public static Result Failure(List<string> erros) => new(erros);
    public static Result Failure(string erro) => new(erro);
}
