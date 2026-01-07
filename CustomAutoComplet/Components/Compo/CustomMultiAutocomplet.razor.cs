using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CustomAutoComplet.Components.Compo
{
    public partial class CustomMultiAutocomplet<TItem> : ComponentBase
    {
        /* =======================
 * Parameters (API)
 * =======================*/

        [Parameter] public required Func<string, CancellationToken, IAsyncEnumerable<TItem>> SearchFunc { get; set; }

        [Parameter] public List<TItem> Values { get; set; } = new();
        [Parameter] public EventCallback<List<TItem>> ValuesChanged { get; set; }

        [Parameter] public required Func<TItem, string> DisplayFunc { get; set; }

        [Parameter] public RenderFragment<AutocompleteItemContext<TItem>> ItemTemplate { get; set; } = default!;
        [Parameter]  public required Func<TItem, object> KeySelector { get; set; }

        [Parameter] public bool Disabled { get; set; }
        [Parameter] public string Placeholder { get; set; } = "";
        [Parameter] public int MinCharacters { get; set; } = 2;
        [Parameter] public int DebounceInterval { get; set; } = 300;
        [Parameter] public string MaxHeight { get; set; } = "300px";

        /* =======================
         * State
         * =======================*/

        private readonly List<TItem> _items = new();
        private CancellationTokenSource _cts = new();
        private string _searchText = "";
        private bool _loading;
        private bool _open;
        private TItem? _highlighted;
        private int _searchVersion;
        private Exception? _error;

        private string PopupStyle =>
            $"position:absolute; width:100%; z-index:20; max-height:{MaxHeight}; overflow-y:auto";

        public sealed class AutocompleteItemContext<TItem>
        {
            public required TItem Item { get; init; }
            public required string SearchText { get; init; }
        }


        /* =======================
         * Search
         * =======================*/
        private bool IsSelected(TItem item)
        {
            var key = KeySelector(item);
            return Values.Any(v => Equals(KeySelector(v), key));
        }
        private async Task HandleSearchAsync(string text)
        {
            if (Disabled || string.IsNullOrWhiteSpace(text) || text.Length < MinCharacters)
            {
                Close();
                return;
            }

            _cts.Cancel();
            _cts = new CancellationTokenSource();
            var version = ++_searchVersion;

            _items.Clear();
            _error = null;
            _loading = true;
            _open = true;
            StateHasChanged();

            try
            {
                await foreach (var item in SearchFunc(text, _cts.Token))
                {
                    if (version != _searchVersion)
                        break;

                    if (IsSelected(item))
                        continue;

                    _items.Add(item);

                    if (_items.Count == 1)
                        _highlighted = item;

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (OperationCanceledException)
            {
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

        private async Task SelectAsync(TItem item)
        {
            if (IsSelected(item))
                return;

            Values.Add(item);
            _searchText = "";
            Close();

            await ValuesChanged.InvokeAsync(Values);
        }
        private async Task RemoveAsync(TItem item)
        {
            var key = KeySelector(item);

            Values.RemoveAll(v =>
                Equals(KeySelector(v), key));

            await ValuesChanged.InvokeAsync(Values);
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Backspace" &&
                string.IsNullOrEmpty(_searchText) &&
                Values.Any())
            {
                var last = Values.Last();
                Values.Remove(last);
                await ValuesChanged.InvokeAsync(Values);
                return;
            }

            if (!_open || !_items.Any())
                return;

            var index = _items.IndexOf(_highlighted!);

            switch (e.Key)
            {
                case "ArrowDown":
                    _highlighted = _items[Math.Min(index + 1, _items.Count - 1)];
                    break;

                case "ArrowUp":
                    _highlighted = _items[Math.Max(index - 1, 0)];
                    break;

                case "Enter":
                    if (_highlighted != null)
                        await SelectAsync(_highlighted);
                    break;

                case "Escape":
                    Close();
                    break;
            }
        }
        private void Close()
        {
            _open = false;
            _items.Clear();
            _highlighted = default;
        }

        private string? GetItemClass(TItem item)
            => EqualityComparer<TItem>.Default.Equals(item, _highlighted)
                ? "mud-primary"
                : null;
    }
}
