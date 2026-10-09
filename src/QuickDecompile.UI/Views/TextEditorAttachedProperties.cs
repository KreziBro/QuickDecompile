using Avalonia;
using AvaloniaEdit;

namespace QuickDecompile.UI.Views;

public static class TextEditorAttachedProperties
{
    public static readonly AttachedProperty<string> SyncTextProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, string>(
            "SyncText", typeof(TextEditorAttachedProperties));

    static TextEditorAttachedProperties()
    {
        SyncTextProperty.Changed.AddClassHandler<TextEditor>((editor, e) =>
        {
            if (e.NewValue is string newText)
            {
                if (editor.Document != null)
                {
                    editor.Document.Text = newText;
                }
                else
                {
                    editor.Document = new AvaloniaEdit.Document.TextDocument(newText);
                }
            }
        });
    }

    public static string GetSyncText(TextEditor element)
    {
        return element.GetValue(SyncTextProperty);
    }

    public static void SetSyncText(TextEditor element, string value)
    {
        element.SetValue(SyncTextProperty, value);
    }
}
