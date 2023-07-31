using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySourceGenerator.Attributes.FieldInitialization
{
    internal static class Sources
    {
        internal const string s_getComponentAttribute = @"
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal class GetComponentAttribute : Attribute
{
    public enum TargetType 
    {
        This = 0,
        Parent = 1,
        Child = 2,
    }

    public GetComponentAttribute(TargetType targetType = TargetType.This) 
    {
    }
}"
;

        internal const string s_getSingletonAttribute = @"
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
internal class GetSingletonAttribute : Attribute
{
}
"
;
    }
}
