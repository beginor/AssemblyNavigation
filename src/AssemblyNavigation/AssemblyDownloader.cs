using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using Mono.Cecil;

namespace Beginor.AssemblyNavigation {

	public class AssemblyDownloader {

		private static readonly IDictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
		private readonly ISet<string> _loadingSet = new HashSet<string>();
		private static readonly object LoadingSetLock = new object();

		private static readonly string[] SilverlightRuntimeAssemblyNames = new[] {
			"Microsoft.VisualBasic.dll",
			"Microsoft.Xna.Framework.dll",
			"Microsoft.Xna.Framework.Graphics.dll",
			"Microsoft.Xna.Framework.Graphics.Shaders.dll",
			"mscorlib.dll",
			"System.Core.dll",
			"System.dll",
			"System.Net.dll",
			"System.Runtime.Serialization.dll",
			"System.ServiceModel.dll",
			"System.ServiceModel.Web.dll",
			"System.Windows.Browser.dll",
			"System.Windows.dll",
			"System.Windows.RuntimeHost.dll",
			"System.Windows.Xna.dll",
			"System.Xml.dll"
		};

		private bool _isbusy;
		private string _loadingAssemblyName;

		public event EventHandler<DownloadCommpletedEventArgs> DownloadCommpleted;
		public event EventHandler<AsyncCompletedEventArgs> DownloadFailed;

		public Assembly GetAssembly(string assemblyName) {
			return LoadedAssemblies.ContainsKey(assemblyName) ? LoadedAssemblies[assemblyName] : null;
		}

		public void OnDownloadFailed(Exception ex) {
			var handler = this.DownloadFailed;
			if (handler != null) {
				handler(this, new AsyncCompletedEventArgs(ex, true, null));
			}
		}

		private void OnDownloadAssemblyCommpleted(DownloadCommpletedEventArgs e) {
			var handler = this.DownloadCommpleted;
			if (handler != null) {
				handler(this, e);
			}
		}

		public void DownloadAsync(string assemblyName) {
			if (this._isbusy) {
				throw new InvalidOperationException(string.Format("AssemblyDownloader is loading {0}, please waite ...", this._loadingAssemblyName));
			}
			assemblyName = EnsureAssemblyNameEndsWdithDll(assemblyName);
			this._loadingAssemblyName = assemblyName;
			if (IsAssemblyLoaded(assemblyName)) {
				this.DownloadCompleted();
			}
			else {
				DownloadAssemblyAsyncCore(assemblyName);
			}
		}

		private void DownloadAssemblyAsyncCore(string assemblyName) {
			this._isbusy = true;
			assemblyName = EnsureAssemblyNameEndsWdithDll(assemblyName);
			var name = assemblyName;
			var webClient = new WebClient();
			webClient.OpenReadCompleted += (sender, e) => this.OnReadOneAssembly(name, e);
			try {
				webClient.OpenReadAsync(new Uri(assemblyName, UriKind.Relative));
			}
			catch (Exception ex) {
				this.OnDownloadFailed(ex);
			}
		}

		internal static string EnsureAssemblyNameEndsWdithDll(string assemblyName) {
			if (!assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
				assemblyName += ".dll";
			}
			return assemblyName;
		}

		private void OnReadOneAssembly(string name, OpenReadCompletedEventArgs e) {
			if (e.Error != null) {
				this.OnDownloadFailed(e.Error);
				this._isbusy = false;
				return;
			}
			var assemblyStream = e.Result;
			var references = GetReferenceAssemblyNames(assemblyStream);
			AddNotLoadedReferenceAssemblyToLoadingSet(references);

			assemblyStream.Seek(0, SeekOrigin.Begin);
			LoadToAssemblyPart(assemblyStream, name);

			if (this._loadingSet.Count > 0) {
				var asm = this._loadingSet.First();
				lock (LoadingSetLock) {
					this._loadingSet.Remove(asm);
				}
				this.DownloadAssemblyAsyncCore(asm);
			}
			else {
				this.DownloadCompleted();
			}
		}

		private void DownloadCompleted() {
			this._isbusy = false;
			var assembly = this.GetAssembly(this._loadingAssemblyName);
			this.OnDownloadAssemblyCommpleted(new DownloadCommpletedEventArgs(assembly));
		}

		private static void LoadToAssemblyPart(Stream assemblyStream, string name) {
			var part = new AssemblyPart {
				Source = name
			};
			var assembly = part.Load(assemblyStream);
			LoadedAssemblies.Add(name, assembly);
		}

		private void AddNotLoadedReferenceAssemblyToLoadingSet(IEnumerable<string> references) {
			var referencesNotLoaded = from reference in references
											  where !(IsAssemblyLoaded(reference))
											  select reference;
			foreach (var @ref in referencesNotLoaded) {
				lock (LoadingSetLock) {
					if (!this._loadingSet.Contains(@ref)) {
						this._loadingSet.Add(@ref);
					}
				}
			}
		}

		private static bool IsAssemblyLoaded(string assemblyName) {
			return SilverlightRuntimeAssemblyNames.Any(asmName => asmName.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
				|| LoadedAssemblies.ContainsKey(assemblyName)
				|| Deployment.Current.Parts.Any(ap => ap.Source.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
		}

		private static IEnumerable<string> GetReferenceAssemblyNames(Stream assemblyStream) {
			var asmDef = AssemblyDefinition.ReadAssembly(assemblyStream);
			return asmDef.MainModule.AssemblyReferences.Select(anr => anr.Name + ".dll");
		}
	}
}