using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Saigor.Shared.Components
{
    public partial class EnumSelectField<TEnum> where TEnum : struct, Enum
    {
        public static List<Option> GetEnumOptions()
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new Option { Value = e, Text = GetDisplayName(e) })
                .ToList();
        }

        private static string GetDisplayName(TEnum value)
        {
            var field = typeof(TEnum).GetField(value.ToString()!);
            var attr = field?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            return attr?.Name ?? value.ToString()!;
        }

        public class Option
        {
            public TEnum Value { get; set; } = default!;
            public string Text { get; set; } = string.Empty;
        }
    }
} 