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

        public static readonly RoutedUICommand Time = new RoutedUICommand(
            "Time",
            "Time",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.T, ModifierKeys.Alt)
            }
        );

        public static readonly RoutedUICommand Backup = new RoutedUICommand(
            "Backup",
            "Backup",
            typeof(CustomCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.U, ModifierKeys.Alt)
            }
        );
    }
}
