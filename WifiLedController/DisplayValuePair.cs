using System;

namespace WifiLedController
{
    internal class DisplayValuePair<TDisplay, TValue> : IEquatable<DisplayValuePair<TDisplay, TValue>>
    {
        public TDisplay Display { get; set; }
        public TValue Value { get; set; }

        public DisplayValuePair(TDisplay Display, TValue Value)
        {
            this.Display = Display;
            this.Value = Value;
        }

        //ToString override to show only the display value
        public override string ToString()
        {
            return Display.ToString();
        }

        //Implementing Equals we check if the display parts are the same but NOT the values.
        public bool Equals(DisplayValuePair<TDisplay, TValue> other)
        {
            return (this.Display.Equals(other.Display));
        }
    }
}