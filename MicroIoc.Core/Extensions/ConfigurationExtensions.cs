using System;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MicroIoc;
using System.Linq.Expressions;

namespace MicroIoc
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration Property<T, TProp>(this IConfiguration configuration, Expression<Func<T, TProp>> propertyExpression, TProp value)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression is not a valid member expression");

            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression is not a valid property on the class");

            return configuration.Configure<T>(new InjectedProperty<TProp>(propertyInfo.Name, value));
        }

        public static IConfiguration ConstructorParam<T, TParam>(this IConfiguration configuration, string name, TParam value)
        {
            return configuration.Configure<T>(new InjectedConstructorParam<TParam>(name, value));
        }
    }
}
