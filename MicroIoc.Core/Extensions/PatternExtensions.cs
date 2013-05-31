using System;

namespace MicroIoc
{
    public static class PatternExtensions
    {
        public static string PropertyPattern(this Type type, string memberName)
        {
            return type.FullName + "." + memberName;
        }

        public static string ConstructorParamPattern(this Type type, string memberName)
        {
            return type.FullName + "#" + memberName;
        }
    }
}
