using MudBlazor;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoKit.Dialogs;
public partial class LoadingDialog
{
    private static readonly DialogOptions _defaultOptions = new()
    {
        BackdropClick = false,
        CloseButton = false,
        CloseOnEscapeKey = false
    };

    public readonly struct CloseHandle : IDisposable
    {
        private readonly IDialogReference? _dialogReference;
        internal CloseHandle(IDialogReference dialogReference)
        {
            _dialogReference = dialogReference;
        }

        [MemberNotNullWhen(false, nameof(_dialogReference))]
        public bool IsDefault => _dialogReference is null;

        /// <summary>
        /// Close the loading dialog.
        /// </summary>
        /// <exception cref="InvalidOperationException">The struct has not been initialized using the constructor.</exception>
        public void Close()
        {
            if (IsDefault)
                throw new InvalidOperationException("This handle instance has not been initialized.");

            _dialogReference.Close();
        }

        void IDisposable.Dispose() => Close();
    }

    public static async Task<CloseHandle> ShowAsync(IDialogService dialogService, string title)
    {
        IDialogReference reference = await dialogService.ShowAsync<LoadingDialog>(title, _defaultOptions);
        return new CloseHandle(reference);
    }

    public static async Task<CloseHandle> ShowAsync(IDialogService dialogService, string title, string message)
    {
        DialogParameters<LoadingDialog> parameters = new()
        {
            { x => x.Message, message }
        };

        IDialogReference reference = await dialogService.ShowAsync<LoadingDialog>(title, parameters, _defaultOptions);
        return new CloseHandle(reference);
    }
}
