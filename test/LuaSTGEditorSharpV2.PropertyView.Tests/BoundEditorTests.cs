using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LuaSTGEditorSharpV2.WPF.Controls;
using Xceed.Wpf.Toolkit;
using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public class BoundEditorTests
{
    [Fact]
    public void SupportedEditorsBindValueWatermarkAndEnabledState()
    {
        RunOnSta(() =>
        {
            VerifyEditor(
                new WatermarkTextBox(),
                TextBox.TextProperty,
                WatermarkTextBox.WatermarkProperty);
            VerifyEditor(
                new WatermarkComboBox(),
                ComboBox.TextProperty,
                WatermarkComboBox.WatermarkProperty);
            VerifyEditor(
                new ExpressionSpinner(),
                ExpressionSpinner.TextProperty,
                ExpressionSpinner.WatermarkProperty);
        });
    }

    [Fact]
    public void UnsupportedEditorFailsImmediately()
    {
        RunOnSta(() =>
        {
            var editor = new TextBox();
            Assert.Throws<InvalidOperationException>(
                () => BoundEditor.SetProperty(editor, new BoundProperty()));
        });
    }

    private static void VerifyEditor(
        FrameworkElement editor,
        DependencyProperty textProperty,
        DependencyProperty watermarkProperty)
    {
        var container = new Grid();
        var property = new BoundProperty
        {
            Value = "12",
        };

        BoundEditor.SetProperty(editor, property);
        container.Children.Add(editor);
        BoundEditor.SetConflictText(container, "Values differ");

        Assert.Equal("12", editor.GetValue(textProperty));
        Assert.True(editor.IsEnabled);
        Assert.Null(editor.GetValue(watermarkProperty));

        editor.SetCurrentValue(textProperty, "24");
        var valueBinding =
            BindingOperations.GetBindingExpressionBase(editor, textProperty);
        Assert.NotNull(valueBinding);
        valueBinding!.UpdateSource();
        Assert.Equal("24", property.Value);

        property.HasConflict = true;

        Assert.False(editor.IsEnabled);
        Assert.Equal("Values differ", editor.GetValue(watermarkProperty));

        BoundEditor.SetConflictText(container, "Different values");
        Assert.Equal("Different values", editor.GetValue(watermarkProperty));

        property.HasConflict = false;

        Assert.True(editor.IsEnabled);
        Assert.Null(editor.GetValue(watermarkProperty));

        BoundEditor.SetProperty(editor, null);

        Assert.Null(
            BindingOperations.GetBindingExpressionBase(editor, textProperty));
        Assert.Null(
            BindingOperations.GetBindingExpressionBase(
                editor,
                watermarkProperty));
        Assert.Null(
            BindingOperations.GetBindingExpressionBase(
                editor,
                UIElement.IsEnabledProperty));
    }

    private static void RunOnSta(Action action)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
            }
        })
        {
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        Assert.True(
            thread.Join(TimeSpan.FromSeconds(10)),
            "STA test thread did not complete.");
        failure?.Throw();
    }
}
