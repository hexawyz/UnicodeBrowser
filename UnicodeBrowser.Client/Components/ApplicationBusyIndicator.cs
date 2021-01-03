using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace UnicodeBrowser.Client.Components
{
	public sealed class ApplicationBusyIndicator : ComponentBase, IDisposable
	{
		[Inject]
		public ApplicationState ApplicationState { get; set; }

		[Parameter]
		public string Class { get; set; }
		
		protected override Task OnInitializedAsync()
		{
			ApplicationState.PropertyChanged += OnApplicationStatePropertyChanged;
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			ApplicationState.PropertyChanged -= OnApplicationStatePropertyChanged;
		}

		private void OnApplicationStatePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ApplicationState.IsBusy))
			{
				StateHasChanged();
			}
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			if (ApplicationState.IsBusy)
			{
				int sequence = 0;

				builder.OpenElement(sequence, "span");
				if (!string.IsNullOrEmpty(Class))
				{
					builder.AddAttribute(++sequence, "class", Class);
				}
				builder.AddAttribute(++sequence, "role", "alert");
				builder.AddAttribute(++sequence, "aria-busy", "true");
				builder.OpenElement(++sequence, "i");
				builder.AddAttribute(++sequence, "class", "fas fa-spinner fa-pulse");
				builder.AddAttribute(++sequence, "title", "The application is busy…");
				builder.AddAttribute(++sequence, "role", "img");
				builder.AddAttribute(++sequence, "aria-label", "The application is busy…");
				builder.CloseElement();
				builder.CloseElement();
			}

			base.BuildRenderTree(builder);
		}
	}
}
