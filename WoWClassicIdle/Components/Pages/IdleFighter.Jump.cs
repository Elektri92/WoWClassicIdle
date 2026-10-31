using Microsoft.JSInterop;

namespace WoWClassicIdle.Components.Pages;

// Space jump (js/jump.js): the animation runs in the browser, the page only counts the jumps
public partial class IdleFighter
{
    private IJSObjectReference? _jumpHandle;

    [JSInvokable]
    public void OnJumped() => _engine.RecordJump();

    private async Task DisposeJumpHandleAsync()
    {
        if (_jumpHandle == null) return;
        try
        {
            await _jumpHandle.InvokeVoidAsync("dispose");
            await _jumpHandle.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
