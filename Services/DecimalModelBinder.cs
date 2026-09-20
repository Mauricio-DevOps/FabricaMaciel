using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fabrica.Services;

// HTML number inputs post a dot; currency text inputs use the user's Brazilian format.
public sealed class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var value = context.ValueProvider.GetValue(context.ModelName);
        if (value == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, value);
        var text = value.FirstValue?.Trim();
        if (string.IsNullOrEmpty(text) && Nullable.GetUnderlyingType(context.ModelType) != null)
        {
            context.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }
        var culture = text?.Contains(',') == true ? CultureInfo.GetCultureInfo("pt-BR") : CultureInfo.InvariantCulture;
        if (decimal.TryParse(text, NumberStyles.Number, culture, out var number))
            context.Result = ModelBindingResult.Success(number);
        else
            context.ModelState.TryAddModelError(context.ModelName, "Informe um número válido. Exemplo: 20,50.");
        return Task.CompletedTask;
    }
}

public sealed class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        (Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType) == typeof(decimal)
            ? new DecimalModelBinder() : null;
}
