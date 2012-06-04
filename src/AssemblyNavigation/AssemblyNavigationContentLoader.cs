using System;
using System.Windows.Navigation;

namespace Beginor.AssemblyNavigation {

	public class AssemblyNavigationContentLoader : INavigationContentLoader {

		private static AssemblyDownloader _downloader;

		public IAsyncResult BeginLoad(Uri targetUri, Uri currentUri, AsyncCallback userCallback, object asyncState) {
			var typeFullName = targetUri.ToString();
			if (string.IsNullOrEmpty(typeFullName)) {
				return null;
			}
			var arr = typeFullName.Split(',');
			var typeName = arr[0];
			var assemblyName = AssemblyDownloader.EnsureAssemblyNameEndsWdithDll(arr[1]);
			var asyncResult = new AssemblyNavigationContentLoaderAsyncResult {
				AsyncState = asyncState,
				TypeName = typeName,
				AssemblyName = assemblyName
			};
			BeginLoadCore(userCallback, asyncResult);
			return asyncResult;
		}

		private static void BeginLoadCore(AsyncCallback userCallback, AssemblyNavigationContentLoaderAsyncResult result) {
			if (_downloader == null) {
				_downloader = new AssemblyDownloader();
			}
			var handlers = new EventHandler<DownloadCommpletedEventArgs>[1];
			handlers[0] = (sender, e) => {
				_downloader.DownloadCommpleted -= handlers[0];
				result.Assembly = e.Result;
				userCallback(result);
			};
			_downloader.DownloadCommpleted += handlers[0];
			_downloader.DownloadAsync(result.AssemblyName);
		}

		public void CancelLoad(IAsyncResult asyncResult) {
		}

		public LoadResult EndLoad(IAsyncResult asyncResult) {
			var result = asyncResult as AssemblyNavigationContentLoaderAsyncResult;
			if (result == null) {
				throw new InvalidOperationException(string.Format("Wrong kind of {0} passed in.  The {0} passed in should only come from {1}.", "IAsyncResult", "AssemblyNavigationContentLoader.BeginLoad"));
			}
			var loadResult = new LoadResult(result.GetResultInstance());
			return loadResult;
		}

		public bool CanLoad(Uri targetUri, Uri currentUri) {
			return targetUri.ToString().Split(',').Length == 2;
		}
	}
}