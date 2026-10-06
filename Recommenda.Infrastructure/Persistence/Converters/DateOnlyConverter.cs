using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Recommenda.Infrastructure.Persistence.Converters;

// O driver devolve DATE como DateTime; a conversão preserva DateOnly no domínio.
public sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
    value => value.ToDateTime(TimeOnly.MinValue),
    value => DateOnly.FromDateTime(value));
