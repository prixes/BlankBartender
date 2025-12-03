namespace BlankBartender.WebApi.Validation;

public interface ILiquidValidator
{
    bool IsValid(string? liquidName);
}
