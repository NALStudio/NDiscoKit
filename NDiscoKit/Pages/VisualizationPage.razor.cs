using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NDiscoKit.Services;

namespace NDiscoKit.Pages;
public partial class VisualizationPage : IAsyncDisposable
{
    [Inject]
    private ILogger<VisualizationPage> Logger { get; init; } = default!;

    [Inject]
    private IJSRuntime JS { get; init; } = default!;

    [Inject]
    private AudioRecordingService Recording { get; init; } = default!;

    private ElementReference canvasContainerRef;
    private DotNetObjectReference<VisualizationPage>? thisRef;

    // private readonly VisualizationProcessor processor = new();

    private Task<IJSObjectReference>? module;

    protected override void OnInitialized()
    {
        Recording.SourceChanged += Recording_SourceChanged;
        Recording.DataAvailable += Recording_DataAvailable;

        module = JS.InvokeAsync<IJSObjectReference>("import", "./_content/NDiscoKit/Pages/VisualizationPage.razor.js").AsTask();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IJSObjectReference module = await this.module!;
        if (firstRender)
        {
            thisRef = DotNetObjectReference.Create(this);
            await module.InvokeVoidAsync("start", canvasContainerRef, thisRef);
        }
        else
        {
            await module.InvokeVoidAsync("reloadCanvas", canvasContainerRef);
        }
    }

    private void Recording_DataAvailable(object? sender, ReadOnlyMemory<float> e)
    {
        // processor.Process(e.Span);
    }

    [JSInvokable]
    public double[] JsGetBars()
    {
        return [1, 1, 1];
        // return processor.Output;
    }

    private void Recording_SourceChanged(object? sender, Models.AudioSource? e)
    {
        // Array.Clear(processor.Output);
    }

    public async ValueTask DisposeAsync()
    {
        Recording.SourceChanged -= Recording_SourceChanged;
        Recording.DataAvailable -= Recording_DataAvailable;

        if (module is not null)
        {
            IJSObjectReference obj = await module;
            await obj.InvokeVoidAsync("stop");
            await obj.DisposeAsync();
        }
        thisRef?.Dispose();

        GC.SuppressFinalize(this);
    }

    // TODO: Consolidate all outputs (mono32, stereo32 and fixed size buffers) into one service to reduce duplicated computation.
    private static void Stereo16ToMono32(in ReadOnlySpan<short> stereo16, in Span<float> mono32)
    {
        if (stereo16.Length != 2 * mono32.Length)
            throw new ArgumentException("stereo16 must be exactly twice as large as mono32");

        for (int i = 0; i < mono32.Length; i++)
        {
            short left = stereo16[i * 2];
            short right = stereo16[(i * 2) + 1];

            // Take the average of the two channels and convert it to float
            // Equals to: ((left + right) / 2) / short.MaxValue
            mono32[i] = (left + right) / (float)(short.MaxValue * 2);
        }
    }
}
