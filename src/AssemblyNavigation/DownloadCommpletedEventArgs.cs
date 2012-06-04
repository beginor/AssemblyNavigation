using System;
using System.ComponentModel;
using System.Reflection;

namespace Beginor.AssemblyNavigation {

	public class DownloadCommpletedEventArgs : AsyncCompletedEventArgs {

		public DownloadCommpletedEventArgs(Assembly assembly)
			: this(null, false, null) {
			this.Result = assembly;
		}

		public DownloadCommpletedEventArgs(Exception error, bool cancelled, object userState)
			: base(error, cancelled, userState) {
		}

		public Assembly Result {
			get;
			private set;
		}
	}
}