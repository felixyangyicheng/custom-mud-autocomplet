using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CustomAutoComplet.Components.Compo;

public partial class CustomAutocomplet<TItem> : ComponentBase
{
    /* =======================
     * Parameters (API)
     * =======================*/

    [Parameter] public required Func<string, CancellationToken, IAsyncEnumerable<TItem>> SearchFunc { get; set; }
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; } = default!;
    [Parameter] public RenderFragment LoadingTemplate { get; set; } = default!;
    [Parameter] public RenderFragment EmptyTemplate { get; set; } = default!;
    [Parameter] public RenderFragment<Exception> ErrorTemplate { get; set; } = default!;

    [Parameter] public TItem Value { get; set; } = default!;
    [Parameter] public EventCallback<TItem> ValueChanged { get; set; }

    [Parameter] public bool Clearable { get; set; } = true;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Placeholder { get; set; } = "";
    [Parameter] public int DebounceInterval { get; set; } = 300;
    [Parameter] public int MinCharacters { get; set; } = 2;
    [Parameter] public string MaxHeight { get; set; } = "300px";
    [Parameter] public int ItemSize { get; set; } = 40;

    /* =======================
     * State
     * =======================*/

    protected readonly List<TItem> _items = new();
    protected CancellationTokenSource _cts=new();
    protected string _searchText="";
    protected bool _loading;
    protected bool _open;
    protected TItem _highlighted=default!;
    protected int _searchVersion;
    protected Exception _error=default!;

    protected string PopupStyle =>
        $"position:absolute; width:100%; z-index:20; max-height:{MaxHeight}; overflow-y:auto";

    /* =======================
     * Lifecycle
     * =======================*/

    protected override void OnParametersSet()
    {
        if (Value != null)
            _searchText = Value.ToString()!;
    }

    /* =======================
     * Search
     * =======================*/

    protected async Task HandleSearchAsync(string text)
    {
        if (Disabled || string.IsNullOrWhiteSpace(text) || text.Length < MinCharacters)
        {
            Close();
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var version = ++_searchVersion;

        _items.Clear();
        _error = null!;
        _loading = true;
        _open = true;
        StateHasChanged();

        try
        {
            await foreach (var item in SearchFunc(text, _cts.Token))
            {
                if (version != _searchVersion)
                    break;

                _items.Add(item);

                if (_items.Count == 1)
                    _highlighted = item;

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
        catch (Exception ex)
        {
            _error = ex;
        }
        finally
        {
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /* =======================
     * Interaction
     * =======================*/

    protected async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (!_open || !_items.Any())
            return;

        var index = _items.IndexOf(_highlighted);

        switch (e.Key)
        {
            case "ArrowDown":
                _highlighted = _items[Math.Min(index + 1, _items.Count - 1)];
                break;

            case "ArrowUp":
                _highlighted = _items[Math.Max(index - 1, 0)];
                break;

            case "Enter":
                await SelectAsync(_highlighted);
                break;

            case "Escape":
                Close();
                break;
        }
    }

    protected async Task SelectAsync(TItem item)
    {
        Value = item;
        if (item !=null)
        {

            _searchText = item.ToString()!;
        }
        Close();
        await ValueChanged.InvokeAsync(item);
    }

    protected void Clear()
    {
        Value = default!;
        _searchText = "";
        Close();
    }

    protected void Close()
    {
        _open = false;
        _items.Clear();
    }

    protected string GetItemClass(TItem item)
        => EqualityComparer<TItem>.Default.Equals(item, _highlighted)
            ? "mud-primary"
            : null;
}