using System.Text.RegularExpressions;
using System.Threading.Channels;
using Microsoft.Playwright;

namespace Tests.E2e.Infra;

/// <summary>
///     Simulates a slow network between the browser and the Blazor Server circuit
///     by delaying every SignalR WebSocket frame in both directions. Frame order is preserved.
///     Must be called before the page navigates, so the circuit's socket is routed.
/// </summary>
public static partial class BlazorLatency {
    public static Task AddAsync(IPage page, TimeSpan oneWayDelay) =>
        page.RouteWebSocketAsync(BlazorSocket(), ws => {
            IWebSocketRoute server = ws.ConnectToServer();
            ws.OnMessage(DelayedForwarder(oneWayDelay, server));
            server.OnMessage(DelayedForwarder(oneWayDelay, ws));
        });

    private static Action<IWebSocketFrame> DelayedForwarder(TimeSpan delay, IWebSocketRoute target) {
        var queue = Channel.CreateUnbounded<(DateTime Due, string? Text, byte[]? Binary)>();

        _ = Task.Run(async () => {
            await foreach ((DateTime due, var text, var binary) in queue.Reader.ReadAllAsync()) {
                TimeSpan wait = due - DateTime.UtcNow;
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait);

                if (binary is not null)
                    target.Send(binary);
                else
                    target.Send(text ?? string.Empty);
            }
        });

        return frame => queue.Writer.TryWrite((DateTime.UtcNow + delay, frame.Text, frame.Binary));
    }

    [GeneratedRegex("/_blazor")]
    private static partial Regex BlazorSocket();
}
