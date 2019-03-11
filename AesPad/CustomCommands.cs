using System.Windows.Input;

namespace AesPad {
    class CustomCommands {
        public static readonly RoutedUICommand Blur = new RoutedUICommand(
            "Blur", 
            "Blur", 
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.B, ModifierKeys.Alt)
            }
        );
    }
}
