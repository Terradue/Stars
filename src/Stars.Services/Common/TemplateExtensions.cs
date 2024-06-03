// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: TemplateExtensions.cs

using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Terradue.Stars.Common
{
    public static class TemplateExtensions
    {
        public static string ReplaceMacro<T>(this string template, string key, T obj)
        {
            return Regex.Replace(template, @"{(?<exp>[^}]+)}", match =>
            {
                var p = Expression.Parameter(typeof(T), key);
                var e = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda(new[] { p }, null, match.Groups["exp"].Value);
                return (e.Compile().DynamicInvoke(obj) ?? "").ToString();
            });
        }
    }
}
