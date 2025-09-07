using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ilsActionEditor.Editor
{
    public class IconButton : Button
    {
        private Label buttonText;
        private Image buttonIcon;

        public override string text
        {
            get
            {
                if (buttonText != null)
                {
                    return  buttonText.text;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (buttonText != null)
                {
                    buttonText.text = value;
                }
            }
        }

        public Image Icon
        {
            get => buttonIcon;
        }

        public IconButton(string text,string tooltip,Texture2D icon)
        {
            this.tooltip = tooltip;
            // 创建文本
            if (!string.IsNullOrEmpty(text))
            {
                buttonText = new Label(text);
            }
            
            // 创建图标
            buttonIcon = new Image();
            buttonIcon.scaleMode = ScaleMode.StretchToFill;
            buttonIcon.image = icon; // 加载图片资源
            buttonIcon.style.flexDirection = FlexDirection.Row;

            // 组装元素
            this.Add(buttonIcon);
            this.Add(buttonText);
        }
    }

    public class ToolbarIconButton : ToolbarButton
    {
        private Label buttonText;
        private Image buttonIcon;
        
        public override string text
        {
            get
            {
                if (buttonText != null)
                {
                    return  buttonText.text;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (buttonText != null)
                {
                    buttonText.text = value;
                }
            }
        }

        public Image Icon
        {
            get => buttonIcon;
        }

        public ToolbarIconButton(string text,string tooltip,Texture2D icon)
        {
            this.tooltip = tooltip;
            // 创建图标
            if (!string.IsNullOrEmpty(text))
            {
                buttonText = new Label(text);
            }
            buttonIcon = new Image();
            buttonIcon.scaleMode = ScaleMode.ScaleToFit;
            buttonIcon.image = icon; 
            buttonIcon.style.flexDirection = FlexDirection.Row;
            
            // 组装元素
            this.style.flexDirection = FlexDirection.Row;
            this.Add(buttonIcon);
            this.Add(buttonText);
        }
    }
    
    public class IconToggle : Toggle
    {
        private Label buttonText;
        private Image buttonIcon;

        public string text
        {
            get
            {
                if (buttonText != null)
                {
                    return  buttonText.text;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (buttonText != null)
                {
                    buttonText.text = value;
                }
            }
        }

        public Image Icon
        {
            get => buttonIcon;
        }

        public IconToggle(string text,string tooltip,Texture2D icon)
        {
            this.tooltip = tooltip;
            // 创建文本
            if (!string.IsNullOrEmpty(text))
            {
                buttonText = new Label(text);
            }
            
            // 创建图标
            buttonIcon = new Image();
            buttonIcon.scaleMode = ScaleMode.StretchToFill;
            buttonIcon.image = icon; // 加载图片资源
            buttonIcon.style.flexDirection = FlexDirection.Row;

            // 组装元素
            this.Add(buttonIcon);
            this.Add(buttonText);
        }
    }
}