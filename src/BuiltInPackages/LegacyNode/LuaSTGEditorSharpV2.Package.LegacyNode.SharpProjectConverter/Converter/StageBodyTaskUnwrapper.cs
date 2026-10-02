using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Package.LegacyNode.SharpProjectConverter.Converter;

/// <summary>
/// Fuses the V1 stage body task with the V2 stage life cycle wrapper.
/// <para>
/// The V1 editor wraps every stage body in a task
/// (<c>lasttask=task.New(self,function() ... end)</c>, see <c>.Task.TaskNode</c>), and the V1 stage
/// template then waits for that very task
/// (<c>while coroutine.status(self.task[1])~='dead' do task.Wait() end</c>) before running its
/// finish sequence.  The V2 stage generator hands the same body to
/// <c>stage.group.initTask(self, f)</c>, which already runs <c>f</c> inside a task and waits for
/// that task before its own finish sequence.  Keeping the converted <c>Task</c> node therefore
/// nests a second task inside the one the framework waits for: the callback returns as soon as the
/// inner task is spawned, the framework sees its task die, runs its finish sequence, and the stage
/// ends on the frame it started.  The inner wrapper is redundant in V2, so its children move up to
/// the stage and the body runs directly in the framework's task.
/// </para>
/// <para>
/// The converter runs on the task node itself, after the traversal has already pushed that node's
/// children for conversion, so lifting them cannot skip them.  Only a sole body task of a stage is
/// fused, and only in the plain shape: a target other than <c>self</c> changes who owns the task,
/// and <c>redirect self</c> rebinds <c>self</c> inside the body, so both keep the nested task
/// exactly as V1 emitted it.
/// </para>
/// </summary>
public sealed class StageBodyTaskUnwrapper : ISharpNodeFormatConverter
{
    private const string StageType = "Stage";
    private const string TaskType = "Task";

    public NodeData Convert(NodeData source, SharpNodeFormattingContext context)
    {
        if (source.TypeUID != TaskType)
        {
            return source;
        }

        var stage = source.PhysicalParent;
        if (stage is not { TypeUID: StageType } || stage.PhysicalChildren.Count != 1)
        {
            return source;
        }

        if (source.GetProperty("target", "self") != "self"
            || IsEnabled(source.GetProperty("redirect", "false"))
            || IsEnabled(source.GetProperty("redirect_self_to_target", "false")))
        {
            return source;
        }

        // Detach first: the task is the stage's only child, so appending afterwards keeps the
        // original order of the body nodes.  Detaching also clears `PhysicalParent`, which keeps
        // `SharpNodeFormatConverter.Convert` from re-inserting the task at a stale index.
        stage.Remove(0);
        while (source.PhysicalChildren.Count > 0)
        {
            stage.Add(source.Remove(0));
        }
        return source;
    }

    private static bool IsEnabled(string value)
    {
        return bool.TryParse(value, out var parsed) ? parsed : value == "1";
    }
}
