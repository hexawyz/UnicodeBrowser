using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.ComponentModel;

namespace UnicodeBrowser.Client.Components
{
	public sealed class ApplicationBusyIndicator : BlazorComponent, IDisposable
	{
		[Inject]
		public ApplicationState ApplicationState { get; set; }

		[Parameter]
		internal string Class { get; set; }
		
		protected override void OnInit()
		{
			ApplicationState.PropertyChanged += OnApplicationStatePropertyChanged;
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
