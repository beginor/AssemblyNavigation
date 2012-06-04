using System;
using System.Reflection;

namespace Beginor.AssemblyNavigation {

	public static class DeploymentExtensions {

		private static readonly AssemblyDownloader Downloader = new AssemblyDownloader();
		
		public static void DownloadAssemblyAsync(string assemblyName, Action<Assembly> callback) {
			var handlers = new EventHandler<DownloadCommpletedEventArgs>[1];
			handlers[0] = (s, arg) => {
				Downloader.DownloadCommpleted -= handlers[0];
				if (callback != null) {
					callback(arg.Result);
				}
			};
			Downloader.DownloadCommpleted += handlers[0];
			Downloader.DownloadAsync(assemblyName);
		}
	}
}