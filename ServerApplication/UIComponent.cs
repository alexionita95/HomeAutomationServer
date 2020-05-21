using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{
    public enum ComponentShape
    {
        Rectangle,Circle
    }
    public enum ComponentAttribute
    {
        Mouse = 1,
        Keyboard = 2,
        All = Mouse + Keyboard

    }
    public class UIComponent
    {
        public long ID { get; set; }
        public string DisplayName { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BackgroundColor { get; set; }
        public ComponentShape Shape { get; set; }
        public ComponentAttribute Attributes { get; set; }

        public UIComponent()
        {

        }
    }
}
