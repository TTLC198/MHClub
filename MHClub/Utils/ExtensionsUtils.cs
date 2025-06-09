using System.Text;
using MHClub.Domain.Models;

namespace MHClub.Utils;

public static class ExtensionsUtils
{
    public static TSelf TrimStringProperties<TSelf>(this TSelf input)
    {
        var stringProperties = input.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite);

        foreach (var stringProperty in stringProperties)
        {
            var currentValue = (string)stringProperty.GetValue(input, null);
            if (currentValue != null)
                stringProperty.SetValue(input, currentValue.Trim(), null);
        }
        return input;
    }

    public static string GetFullCategoryName(this Category category)
    {
        List<string> stringNames = [category.Name ?? "Неизвестная"];
        if (category.ParentCategory is not null)
        {
            var parent = category.ParentCategory;
            stringNames = stringNames.Prepend($"{category.ParentCategory.Name}").ToList();
            if (parent.ParentCategory is not null)
            {
                stringNames = stringNames.Prepend($"{parent.ParentCategory.Name}").ToList();
            }
        }
        return string.Join(" / ", stringNames);
    }
}