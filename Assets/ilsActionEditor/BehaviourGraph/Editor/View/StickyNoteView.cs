using System;
using ilsFramework.Core;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using StickyNoteView = UnityEditor.Experimental.GraphView.StickyNote;
namespace ilsActionEditor.Editor
{
    public class BaseStickyNoteView : StickyNoteView
    {
        public BehaviourGraphView	owner;
        public BaseStickyNoteData data;
        Label                   titleLabel;
        ColorField              colorField;

        public BaseStickyNoteView()
        {
            fontSize = StickyNoteFontSize.Small;
            theme = StickyNoteTheme.Classic;
        }

        public void Initialize(BehaviourGraphView graphView,BaseStickyNoteData data)
        {
            //this.note = note;
            owner = graphView;
            this.data = data;

            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => {
                this.data.title = e.newValue;
            });
            this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e => {
                this.data.content = e.newValue;
            });
        
            title = data.title;
            contents = data.content;
            SetPosition(data.position);
            fontSize = data.fontSize;
            theme = data.theme;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (data != null)
                data.position = newPos;
        }

        public override void OnResized()
        {
            if (data != null)
                data.position = GetPosition();
        }

        public virtual void OnSave()
        {
            data.theme = theme;
            data.fontSize = fontSize;
        }
    }
    [Serializable]
    public class BaseStickyNoteData
    {
        //保存便签的相关数据
        public string title;
        public string content;
        public Rect position;
        public StickyNoteTheme theme;
        public StickyNoteFontSize fontSize;
    }

}