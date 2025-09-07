using System;

namespace ilsActionEditor
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class EntryTranslationAttribute : Attribute
    {


        public EntryTranslationAttribute()
        {
           
        }
        
    }
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class ExitTranslationAttribute : Attribute
    {

    }


}