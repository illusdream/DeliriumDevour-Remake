using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ilsFramework.Core;
using Sirenix.OdinInspector.Editor;

namespace ilsActionEditor
{
    public class TranslationAttributeProcesser : OdinAttributeProcessor<AFSMStateTranslation>
    {
        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            return property.Attributes.Any((t=>t.GetType() ==typeof(EntryTranslationAttribute) ||t.GetType() ==typeof(ExitTranslationAttribute)  ));
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new Sirenix.OdinInspector.ShowIfAttribute("ShowTranslations"));
            base.ProcessSelfAttributes(property, attributes);
        }
    }
}