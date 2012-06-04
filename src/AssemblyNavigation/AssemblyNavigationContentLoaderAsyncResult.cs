using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Beginor.AssemblyNavigation {

	public class AssemblyNavigationContentLoaderAsyncResult : IAsyncResult {

		public bool IsCompleted {
			get;
			internal set;
		}

		public WaitHandle AsyncWaitHandle {
			get {
				return null;
			}
		}

		public object AsyncState {
			get;
			internal set;
		}

		public bool CompletedSynchronously {
			get {
				return false;
			}
		}

		public Assembly Assembly {
			get;
			internal set;
		}

		public string TypeName {
			get;
			internal set;
		}

		public string AssemblyName {
			get;
			internal set;
		}

		public object GetResultInstance() {
			var assembly = this.Assembly;
			if (assembly == null) {
				var part = Deployment.Current.Parts.FirstOrDefault(p => p.Source.Equals(AssemblyName));
				if (part != null) {
					var streamInfo = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative));
					assembly = part.Load(streamInfo.Stream);
				}
			}
			object result = null;
			if (assembly != null) {
				result = assembly.CreateInstance(this.TypeName);
			}
			return result;
		}
	}
}