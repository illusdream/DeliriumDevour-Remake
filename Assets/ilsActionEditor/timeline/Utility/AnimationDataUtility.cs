using System;
using System.Collections.Generic;
using System.Linq;

namespace ilsActionEditor
{

    public static class AnimationDataUtility
    {

        ///<summary>Given an object, returns possible field and prop paths marked with [AnimatableParameter] attribute</summary>
        public static string[] GetAnimatableMemberPaths(object root) {
            return Internal_GetAnimatableMemberPaths(root.GetType(), string.Empty);
        }

        //...
        static string[] Internal_GetAnimatableMemberPaths(Type type, string path) {
            var result = new List<string>();
            foreach ( var field in ReflectionTools.RTGetFields(type) ) {
                var current = path + field.Name;
                if (ReflectionTools.RTIsDefined<AnimatableParameterAttribute>(field, true) ) {
                    result.Add(current);
                }

                if (ReflectionTools.RTIsDefined<ParseAnimatableParametersAttribute>(field.FieldType, true) ) {
                    current += '.';
                    result.AddRange(Internal_GetAnimatableMemberPaths(field.FieldType, current));
                }
            }

            foreach ( var prop in ReflectionTools.RTGetProperties(type) ) {
                var current = path + prop.Name;
                if (ReflectionTools.RTIsDefined<AnimatableParameterAttribute>(prop, true) ) {
                    if (AnimatedParameter.supportedTypes.Contains(prop.PropertyType) ) {
                        result.Add(current);
                    }
                }

                if (ReflectionTools.RTIsDefined<ParseAnimatableParametersAttribute>(prop.PropertyType, true) ) {
                    current += '.';
                    result.AddRange(Internal_GetAnimatableMemberPaths(prop.PropertyType, current));
                }
            }

            return result.ToArray();
        }
    }
}