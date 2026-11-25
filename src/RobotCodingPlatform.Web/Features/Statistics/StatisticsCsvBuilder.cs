using System.Globalization;
using System.Text;

namespace RobotCodingPlatform.Web.Features.Statistics;

public static class StatisticsCsvBuilder
{
    public static byte[] BuildSummary(IReadOnlyList<ChallengeStatisticsDto> rows)
    {
        var builder = new StringBuilder();

        AppendRow(builder,
            "Испытание",
            "ID испытания",
            "Всего попыток",
            "Успешно",
            "Провалено",
            "В ожидании",
            "Уникальных пользователей",
            "Успешность, %",
            "Первая попытка",
            "Последняя попытка");

        foreach (var row in rows)
        {
            AppendRow(builder,
                row.ChallengeName,
                row.ChallengeId,
                row.TotalAttempts,
                row.Succeeded,
                row.Failed,
                row.Pending,
                row.UniqueUsers,
                row.SuccessRate,
                row.FirstAttemptAt,
                row.LastAttemptAt);
        }

        return ToUtf8Bytes(builder);
    }

    public static byte[] BuildDetail(IReadOnlyList<SolutionAttemptStatisticsRowDto> rows)
    {
        var builder = new StringBuilder();

        AppendRow(builder,
            "ID попытки",
            "Испытание",
            "ID пользователя",
            "Пользователь",
            "Email",
            "Статус",
            "Успех",
            "Ошибка",
            "Создана",
            "Завершена");

        foreach (var row in rows)
        {
            AppendRow(builder,
                row.AttemptId,
                row.ChallengeName,
                row.UserId,
                row.UserName,
                row.UserEmail,
                row.Status,
                row.Success is null ? "" : row.Success.Value ? "Да" : "Нет",
                row.Error,
                row.CreatedAt,
                row.CompletedAt);
        }

        return ToUtf8Bytes(builder);
    }

    private static void AppendRow(StringBuilder builder, params object?[] values)
    {
        builder.AppendLine(string.Join(';', values.Select(value => Escape(Format(value)))));
    }

    private static string Format(object? value) => value switch
    {
        null => "",
        DateTime date => date.ToUniversalTime()
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };

    private static string Escape(string value)
    {
        // Защита от CSV-инъекций: Excel не должен исполнять значения как формулы.
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            value = "'" + value;
        }

        if (!value.Contains(';') && !value.Contains('"') &&
            !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static byte[] ToUtf8Bytes(StringBuilder builder)
    {
        // BOM нужен, чтобы Excel корректно открыл кириллицу.
        return Encoding.UTF8.GetBytes("\uFEFF" + builder);
    }
}
