﻿using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace ProAppCoordConversionModule.Helpers
{
    public static class StoryboardCompleteBehavior
    {
        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.RegisterAttached("Storyboard", typeof(Storyboard), typeof(StoryboardCompleteBehavior), new PropertyMetadata(null, OnStoryboardChanged));

        public static readonly DependencyProperty CompletedCommandProperty = DependencyProperty.RegisterAttached("CompletedCommand", typeof(ICommand), typeof(StoryboardCompleteBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty CompletedCommandParameterProperty = DependencyProperty.RegisterAttached("CompletedCommandParameter", typeof(object), typeof(StoryboardCompleteBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty StoryboardListenerProperty = DependencyProperty.RegisterAttached("StoryboardListener", typeof(StoryboardListener), typeof(StoryboardCompleteBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty StoryboardStartWhenProperty = DependencyProperty.RegisterAttached("StoryboardStartWhen", typeof(bool), typeof(StoryboardCompleteBehavior), new PropertyMetadata(false, StartStoryboard));

        public static bool GetStoryboardStartWhen(DependencyObject target)
        {
            return (bool)target.GetValue(StoryboardProperty);
        }

        public static void SetStoryboardStartWhen(DependencyObject target, bool value)
        {
            target.SetValue(StoryboardProperty, value);
        }

        private static void StartStoryboard(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listener = (StoryboardListener)d.GetValue(StoryboardCompleteBehavior.StoryboardListenerProperty);

            if (listener != null)
            {
                listener.Storyboard.Stop();
                if ((bool)e.NewValue)
                {
                    listener.Storyboard.Begin();
                }
            }
        }

        public static Storyboard GetStoryboard(DependencyObject target)
        {
            return (Storyboard)target.GetValue(StoryboardProperty);
        }

        public static void SetStoryboard(DependencyObject target, Storyboard value)
        {
            target.SetValue(StoryboardProperty, value);
        }

        private static void OnStoryboardChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var oldListener = (StoryboardListener)target.GetValue(StoryboardCompleteBehavior.StoryboardListenerProperty);
            if (oldListener != null)
            {
                oldListener.DeleteHandler();
            }
            Storyboard storyboard = (Storyboard)e.NewValue;
            var listener = new StoryboardListener(target, storyboard);
            target.SetValue(StoryboardCompleteBehavior.StoryboardListenerProperty, listener);
        }

        public static ICommand GetCompletedCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(CompletedCommandProperty);
        }

        public static void SetCompletedCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CompletedCommandProperty, value);
        }

        public static object GetCompletedCommandParameter(DependencyObject target)
        {
            return (object)target.GetValue(CompletedCommandParameterProperty);
        }

        public static void SetCompletedCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CompletedCommandParameterProperty, value);
        }

        public class StoryboardListener
        {
            public DependencyObject DependencyObject { get; private set; }
            public Storyboard Storyboard { get; private set; }

            public StoryboardListener(DependencyObject dependencyObject, Storyboard storyboard)
            {
                this.DependencyObject = dependencyObject;
                this.Storyboard = storyboard;
                this.Storyboard.Completed += new EventHandler(Storyboard_Completed);
            }

            public void DeleteHandler()
            {
                this.Storyboard.Completed -= new EventHandler(Storyboard_Completed);
            }

            private void Storyboard_Completed(object sender, EventArgs e)
            {
                ICommand command = StoryboardCompleteBehavior.GetCompletedCommand(DependencyObject);
                object commandParameter = StoryboardCompleteBehavior.GetCompletedCommandParameter(DependencyObject);
                if (command != null && command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
        }
    }
}