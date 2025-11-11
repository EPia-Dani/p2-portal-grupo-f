using System;
using UnityEngine;

namespace Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : Attribute
    {
        public string buttonText;
        public int spacing;
        public ButtonAttribute(string buttonText = null, int spacing = 0)
        {
            this.buttonText = buttonText;
            this.spacing = spacing;
        }
    }
}