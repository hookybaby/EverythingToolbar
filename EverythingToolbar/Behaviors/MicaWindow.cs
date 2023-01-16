﻿using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace EverythingToolbar.Behaviors
{
    public class MicaWindow : Behavior<Window>
    {
        public static readonly DependencyProperty MicaWindowStyleProperty =
            DependencyProperty.Register("MicaStyle", typeof(MicaWindowStyleType), typeof(MicaWindow), new FrameworkPropertyMetadata(MicaWindowStyleType.MainWindow));

        public MicaWindowStyleType MicaWindowStyle
        {
            get => (MicaWindowStyleType)GetValue(MicaWindowStyleProperty);
            set => SetValue(MicaWindowStyleProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (Environment.OSVersion.Version < Helpers.Utils.WindowsVersion.Windows11)
                return;

            if (AssociatedObject.IsLoaded)
            {
                OnMicaWindowLoaded(null, null);
            }
            else
            {
                AssociatedObject.Loaded += OnMicaWindowLoaded;
            }
        }

        private void OnMicaWindowContentRendered(object sender, EventArgs e)
        {
            HwndSource hwnd = (HwndSource)sender;
            int trueValue = 0x01;
            int backdropType = (int)MicaWindowStyle;
            DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
            DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, Marshal.SizeOf(typeof(int)));
        }

        private void OnMicaWindowLoaded(object sender, RoutedEventArgs e)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual((Visual)sender);
            presentationSource.ContentRendered += OnMicaWindowContentRendered;

            WindowChrome.SetWindowChrome(AssociatedObject, new WindowChrome()
            {
                ResizeBorderThickness = new Thickness(3),
                GlassFrameThickness = new Thickness(-1),
                CaptionHeight = 0,
                UseAeroCaptionButtons = false,
            });
        }

        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029,
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        }

        [Flags]
        public enum MicaWindowStyleType : int
        {
            Auto = 0,
            Disable = 1,
            MainWindow = 2, // Mica
            TransientWindow = 3, // Acrylic
            TabbedWindow = 4, // Tabbed
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);
    }
}
