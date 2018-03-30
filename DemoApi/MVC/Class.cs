using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace DemoApi.MVC
{
    public class CsvOutputFormatter : OutputFormatter
    {
        private static readonly Type EnumerableType = typeof(IEnumerable);
        private readonly CsvFormatterOptions _options;

        public string ContentType { get; }

        public CsvOutputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            ContentType = "text/csv";
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            _options = csvFormatterOptions ?? throw new ArgumentNullException(nameof(csvFormatterOptions));
        }

        protected override bool CanWriteType(Type type)
        {

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return IsTypeOfIEnumerable(type);
        }

        private static bool IsTypeOfIEnumerable(Type type)
        {
            return type.GetInterfaces()
                .Any(interfaceType => EnumerableType.IsAssignableFrom(interfaceType));
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            var type = context.Object.GetType();

            var itemType = type.GetGenericArguments().Length > 0 ? 
                type.GetGenericArguments().Last() : 
                type.GetElementType();

            var streamWriter = new StreamWriter(response.Body);

            if (_options.UseSingleLineHeaderInCsv)
            {
                streamWriter.WriteLine(
                    string.Join<string>(
                        new string(_options.CsvDelimiter, 1), 
                        itemType.GetProperties().Select(x => x.Name)
                    )
                );
            }

            foreach (var obj in (IEnumerable<object>)context.Object)
            {
                var vals = obj.GetType().GetProperties().Select(
                    pi => pi.GetValue(obj, null)
                );

                var sb = new StringBuilder();

                foreach (var val in vals)
                {
                    string stringValue;
                    if (val == null)
                        stringValue = string.Empty;
                    else
                    {
                        var propertyType = val.GetType();
                        if (propertyType != typeof(string) && IsTypeOfIEnumerable(propertyType))
                            stringValue = string.Join(",", ((IEnumerable)val).Cast<object>()
                                .Select(x => x.ToString()).ToArray());
                        else
                            stringValue = val.ToString();
                        stringValue =
                            stringValue
                                .Replace("\"", "\"\"")
                                //TODO consider how to handle new lines
                                .Replace("\r", " ")
                                .Replace("\n", " ");
                    }
                    sb.Append("\"")
                        .Append(stringValue)
                        .Append("\"")
                        .Append(_options.CsvDelimiter);
                }

                if(sb.Length > 0)
                    await streamWriter.WriteLineAsync(sb.ToString(0, sb.Length - 1));
            }
            await streamWriter.FlushAsync();
        }
    }
    public class CsvFormatterOptions
    {
        public bool UseSingleLineHeaderInCsv { get; set; } = true;

        public char CsvDelimiter { get; set; } = ',';
    }
}
