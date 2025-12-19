namespace UETexturePainter.Services;

public interface IUndoableAction
{
    void Undo();
    void Redo();
    string Description { get; }
}

public class UndoRedoStack
{
    private readonly Stack<IUndoableAction> _undoStack = new();
    private readonly Stack<IUndoableAction> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void Execute(IUndoableAction action)
    {
        action.Redo();
        _undoStack.Push(action);
        _redoStack.Clear();
    }

    public void Undo()
    {
        if (_undoStack.Count == 0)
        {
            return;
        }

        var action = _undoStack.Pop();
        action.Undo();
        _redoStack.Push(action);
    }

    public void Redo()
    {
        if (_redoStack.Count == 0)
        {
            return;
        }

        var action = _redoStack.Pop();
        action.Redo();
        _undoStack.Push(action);
    }
}
