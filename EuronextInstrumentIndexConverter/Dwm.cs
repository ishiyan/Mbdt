using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EuronextInstrumentIndexConverter
{
    /// <summary>
    /// The Desktop Window Manager (DWM) related utilities.
    /// </summary>
    public static class Dwm
    {
        #region Margins
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="thickness">The thickness.</param>
            public MARGINS(Thickness thickness)
            {
                Left = (int)thickness.Left;
                Right = (int)thickness.Right;
                Top = (int)thickness.Top;
                Bottom = (int)thickness.Bottom;
            }
            /// <summary>
            /// The left margin.
            /// </summary>
            public int Left;
            /// <summary>
            /// The right margin.
            /// </summary>
            public int Right;
            /// <summary>
            /// The top margin.
            /// </summary>
            public int Top;
            /// <summary>
            /// The bottom margin.
            /// </summary>
            public int Bottom;
        }
        #endregion

        #region Imports
        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();
        #endregion

        #region ExtendGlassFrame
        /// <summary>
        /// Extends the Desktop Window Manager (DWM) glass window frame behind the client area.
        /// </summary>
        /// <param name="window">The window for which the frame is extended into the client area.</param>
        /// <param name="margin">The thickness to extend. Negative values are used to create the "sheet of glass" effect where the client area is rendered as a solid surface with no window border.</param>
        /// <returns>True if Desktop Window Manager (DWM) composition is enabled and extension succeeded; false otherwise.</returns>
        public static bool ExtendGlassFrame(this Window window, Thickness margin)
        {
            if (Environment.OSVersion.Version.Major < 6 || !DwmIsCompositionEnabled())
                return false;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (IntPtr.Zero == hwnd)
                throw new InvalidOperationException("The window must be shown before extending glass.");

            // Set the background to transparent from both the WPF and Win32 perspectives.
            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

            MARGINS margins = new MARGINS(margin);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }

        /// <summary>
        /// Creates the "sheet of glass" effect where the client area is rendered as a solid surface with no window border.
        /// </summary>
        /// <param name="window">The window for which the frame is extended into the client area.</param>
        /// <returns>True if Desktop Window Manager (DWM) composition is enabled and extension succeeded; false otherwise.</returns>
        public static bool ExtendGlassFrame(this Window window)
        {
            return window.ExtendGlassFrame(new Thickness(-1));
        }
        #endregion
    }
}
