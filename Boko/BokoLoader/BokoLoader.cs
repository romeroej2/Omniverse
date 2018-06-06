using ff14bot.AClasses;
using ff14bot.Helpers;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using Newtonsoft.Json;

namespace BokoLoader
{
    public class BokoLoader : BotPlugin
    {
        // Change these settings to reflect your project!
        private const int ProjectId = 19;

        private const string ProjectName = "Boko";
        private const string ProjectMainType = "Boko.BokoPlugin";
        private const string ProjectAssemblyName = "Boko.dll";
        private static readonly Color LogColor = Color.FromRgb(219, 180, 87);
        public override string Description => "Combat Chocobo Manager.";
        public override string Author => "Omninewb";
        public override string ButtonText => "Settings";
        public override Version Version => new Version($"{File.ReadAllText(VersionPath)}");
        public override bool WantButton => true;

        private static readonly object Locker = new object();
        private static readonly string ProjectAssembly = Path.Combine(Environment.CurrentDirectory, $@"Plugins\{ProjectName}\{ProjectAssemblyName}");
        private static readonly string GreyMagicAssembly = Path.Combine(Environment.CurrentDirectory, @"GreyMagic.dll");
        private static readonly string VersionPath = Path.Combine(Environment.CurrentDirectory, $@"Plugins\{ProjectName}\version.txt");
        private static readonly string BaseDir = Path.Combine(Environment.CurrentDirectory, $@"Plugins\{ProjectName}");
        private static readonly string ProjectTypeFolder = Path.Combine(Environment.CurrentDirectory, @"Plugins");
        private static bool _updaterStarted, _updaterFinished, _loaded;

        public BokoLoader()
        {
            if (_updaterStarted) { return; }
            _updaterStarted = true;

            Task.Factory.StartNew(AutoUpdate);
        }

        private static object Product { get; set; }

        private static MethodInfo InitFunc { get; set; }
        private static MethodInfo ButtonFunc { get; set; }
        private static MethodInfo PulseFunc { get; set; }

        private static MethodInfo EnabledFunc { get; set; }
        private static MethodInfo DisabledFunc { get; set; }
        private static MethodInfo ShutDownFunc { get; set; }

        #region Overrides

        public override string Name => ProjectName;

        public override void OnInitialize()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { InitFunc.Invoke(Product, null); }
        }

        public override void OnButtonPress()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { ButtonFunc.Invoke(Product, null); }
        }

        public override void OnPulse()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { PulseFunc.Invoke(Product, null); }
        }

        public override void OnEnabled()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { EnabledFunc.Invoke(Product, null); }
        }

        public override void OnDisabled()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { DisabledFunc.Invoke(Product, null); }
        }

        public override void OnShutdown()
        {
            if (!_loaded && Product == null && _updaterFinished) { LoadProduct(); }
            if (Product != null) { ShutDownFunc.Invoke(Product, null); }
        }

        #endregion Overrides

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string name);

        public static bool Unblock(string fileName)
        {
            return DeleteFile(fileName + ":Zone.Identifier");
        }

        public static void RedirectAssembly()
        {
            ResolveEventHandler handler = (sender, args) =>
            {
                string name = Assembly.GetEntryAssembly().GetName().Name;
                var requestedAssembly = new AssemblyName(args.Name);
                return requestedAssembly.Name != name ? null : Assembly.GetEntryAssembly();
            };

            AppDomain.CurrentDomain.AssemblyResolve += handler;

            ResolveEventHandler greyMagicHandler = (sender, args) =>
            {
                var requestedAssembly = new AssemblyName(args.Name);
                return requestedAssembly.Name != "GreyMagic" ? null : Assembly.LoadFrom(GreyMagicAssembly);
            };

            AppDomain.CurrentDomain.AssemblyResolve += greyMagicHandler;
        }

        private static string CompiledAssembliesPath => Path.Combine(Utilities.AssemblyDirectory, "CompiledAssemblies");

        private static Assembly LoadAssembly(string path)
        {
            if (!File.Exists(path)) { return null; }
            if (!Directory.Exists(CompiledAssembliesPath))
            {
                Directory.CreateDirectory(CompiledAssembliesPath);
            }

            var t = DateTime.Now.Ticks;
            var name = $"{Path.GetFileNameWithoutExtension(path)}{t}.{Path.GetExtension(path)}";
            var pdbPath = path.Replace(Path.GetExtension(path), "pdb");
            var pdb = $"{Path.GetFileNameWithoutExtension(path)}{t}.pdb";
            var capath = Path.Combine(CompiledAssembliesPath, name);
            if (File.Exists(capath))
            {
                try
                {
                    File.Delete(capath);
                }
                catch (Exception)
                {
                    //
                }
            }
            if (File.Exists(pdb))
            {
                try
                {
                    File.Delete(pdb);
                }
                catch (Exception)
                {
                    //
                }
            }

            if (!File.Exists(capath))
            {
                File.Copy(path, capath);
            }

            if (!File.Exists(pdb) && File.Exists(pdbPath))
            {
                File.Copy(pdbPath, pdb);
            }

            Assembly assembly = null;
            Unblock(path);
            try { assembly = Assembly.LoadFrom(path); }
            catch (Exception e) { Logging.WriteException(e); }

            return assembly;
        }

        private static object Load()
        {
            Log("Loading...");

            RedirectAssembly();

            var assembly = LoadAssembly(ProjectAssembly);
            if (assembly == null) { return null; }

            Type baseType;
            try { baseType = assembly.GetType(ProjectMainType); }
            catch (Exception e)
            {
                Log(e.ToString());
                return null;
            }

            object bb;
            try { bb = Activator.CreateInstance(baseType); }
            catch (Exception e)
            {
                Log(e.ToString());
                return null;
            }

            if (bb != null) { Log(ProjectName + " was loaded successfully."); }
            else { Log("Could not load " + ProjectName + ". This can be due to a new version of Rebornbuddy being released. An update should be ready soon."); }

            return bb;
        }

        private static void LoadProduct()
        {
            lock (Locker)
            {
                if (Product != null) { return; }
                Product = Load();
                _loaded = true;
                if (Product == null) { return; }

                PulseFunc = Product.GetType().GetMethod("OnPulse");
                EnabledFunc = Product.GetType().GetMethod("OnEnabled");
                DisabledFunc = Product.GetType().GetMethod("OnDisabled");
                ShutDownFunc = Product.GetType().GetMethod("ShutDown");
                ButtonFunc = Product.GetType().GetMethod("OnButtonPress");
                InitFunc = Product.GetType().GetMethod("OnInitialize", new[] { typeof(int) });
                if (InitFunc != null)
                {
#if RB_CN
                Log($"{ProjectName} CN loaded.");
                InitFunc.Invoke(Product, new[] {(object)2});
#else
                    Log($"{ProjectName} 64 loaded.");
                    InitFunc.Invoke(Product, new[] { (object)1 });
#endif
                }
            }
        }

        private static void Log(string message)
        {
            message = "[Auto-Updater][" + ProjectName + "] " + message;
            Logging.Write(LogColor, message);
        }

        private static string GetLocalVersion()
        {
            if (!File.Exists(VersionPath)) { return null; }
            try
            {
                var version = File.ReadAllText(VersionPath);
                return version;
            }
            catch { return null; }
        }

        private static void AutoUpdate()
        {
            var stopwatch = Stopwatch.StartNew();
            var local = GetLocalVersion();

            var message = new VersionMessage { LocalVersion = local, ProductId = ProjectId };
            var responseMessage = GetLatestVersion(message).Result;

            var latest = responseMessage.LatestVersion;

            if (local == latest || latest == null)
            {
                _updaterFinished = true;
                LoadProduct();
                return;
            }

            Log($"Updating to version {latest}.");
            var bytes = responseMessage.Data;
            if (bytes == null || bytes.Length == 0) { return; }

            if (!Clean(BaseDir))
            {
                Log("Could not clean directory for update.");
                _updaterFinished = true;
                return;
            }

            Log("Extracting new files.");
            if (!Extract(bytes, ProjectTypeFolder))
            {
                Log("Could not extract new files.");
                _updaterFinished = true;
                return;
            }

            if (File.Exists(VersionPath)) { File.Delete(VersionPath); }
            try { File.WriteAllText(VersionPath, latest); }
            catch (Exception e) { Log(e.ToString()); }

            stopwatch.Stop();
            Log($"Update complete in {stopwatch.ElapsedMilliseconds} ms.");
            _updaterFinished = true;
            LoadProduct();
        }

        private static bool Clean(string directory)
        {
            foreach (var file in new DirectoryInfo(directory).GetFiles())
            {
                try { file.Delete(); }
                catch { return false; }
            }

            foreach (var dir in new DirectoryInfo(directory).GetDirectories())
            {
                try { dir.Delete(true); }
                catch { return false; }
            }

            return true;
        }

        private static bool Extract(byte[] files, string directory)
        {
            using (var stream = new MemoryStream(files))
            {
                var zip = new FastZip();

                try { zip.ExtractZip(stream, directory, FastZip.Overwrite.Always, null, null, null, false, true); }
                catch (Exception e)
                {
                    Log(e.ToString());
                    return false;
                }
            }

            return true;
        }

        private static async Task<VersionMessage> GetLatestVersion(VersionMessage message)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.omniverse.tech");

                var json = JsonConvert.SerializeObject(message);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync("/api/products/version", content);
                }
                catch (Exception e)
                {
                    Log(e.Message);
                    return null;
                }

                var contents = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<VersionMessage>(contents);
                return responseObject;
            }
        }

        private class VersionMessage
        {
            public int ProductId { get; set; }
            public string LocalVersion { get; set; }
            public string LatestVersion { get; set; }
            public byte[] Data { get; set; } = new byte[0];
        }
    }
}