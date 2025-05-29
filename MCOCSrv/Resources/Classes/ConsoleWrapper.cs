using MCOCSrv.Resources.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MCOCSrv.Resources.Classes
{
    public class ConsoleWrapper : IDisposable
    {
        private readonly InstanceModel WorkingInstance;
        private readonly string WorkingPath;
        private readonly string ServerFile;
        private readonly string PropertiesFile;
        private readonly string EulaFile;
        private Process? ServerProcess;
        private bool WasDisposed = false;
        private int _state;
        public int State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    ServerStateChanged(value);
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] State changed to {value}.");
                }
            }
        }
        public ObservableCollection<string> ConsoleOutput { get; set; } = new();
        public Dictionary<string, string> Actions { get; set; } = new();
        public bool IsRunning => ServerProcess != null && !ServerProcess.HasExited;

        public ConsoleWrapper(InstanceModel instance)
        {
            this.WorkingInstance = instance;
            this.WorkingPath = instance.GetPath();
            this.ServerFile = Path.Combine(this.WorkingPath, $"{instance.id}-{instance.Version}.jar");
            this.PropertiesFile = Path.Combine(this.WorkingPath, "server.properties");
            this.EulaFile = Path.Combine(this.WorkingPath, "eula.txt");
            this.State = 0;
        }

        //Get wrapper's instance name
        public string GetWorkingInstance()
        {
            return WorkingInstance.Name;
        }

        //Check if running, if serverfile exists and for eula.
        //Set up a process, subsc ribe to methods calling the handlers
        public void StartServer()
        {
            State = 1;
            if (IsRunning)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server already running!");
                State = 2;
                return;
            }
            if (!File.Exists(ServerFile))
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server File does not exist! File: {ServerFile}");
                State = 0;
                return;
            }
            if (!EulaCheck())
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] User did not agree to Minecraft EULA! - creating file automatically...");
                string eula = "eula=true";
                File.WriteAllText(EulaFile, eula);

            }
            //Start setup
            ServerProcess = new Process();
            ServerProcess.StartInfo.FileName = "javaw";
            ServerProcess.StartInfo.Arguments = $"-jar {ServerFile} {WorkingInstance.MinHeap}M {WorkingInstance.MaxHeap}M {WorkingInstance.LaunchArguments} nogui";
            ServerProcess.StartInfo.WorkingDirectory = WorkingPath;
            //Output redirection
            ServerProcess.StartInfo.RedirectStandardOutput = true;
            ServerProcess.StartInfo.RedirectStandardError = true;
            ServerProcess.StartInfo.RedirectStandardInput = true;
            ServerProcess.StartInfo.CreateNoWindow = true;
            ServerProcess.OutputDataReceived += ConsoleOutputReceived;
            ServerProcess.ErrorDataReceived += ConsoleOutputReceived;
            //Exit Setup
            ServerProcess.EnableRaisingEvents = true;
            ServerProcess.Exited += ConsoleExited;

            try
            {
                ServerProcess.Start();
                ServerProcess.BeginOutputReadLine();
                ServerProcess.BeginErrorReadLine();
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server Started With Heap Min:{WorkingInstance.MinHeap} Max:{WorkingInstance.MaxHeap}, arguments: {WorkingInstance.LaunchArguments}");
                State = 2;
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Error starting server: {ex.Message}");
                State = 0;
            }
        }

        // try send command thought input handler
        public void SendCommand(string cmd)
        {
            if (!IsRunning)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Cannot send command - server not running!");
                ConsoleOutputHandler?.Invoke(this, "Server is not running!");
                return;
            }

            try
            {
                if (ServerProcess?.StandardInput != null && IsRunning)
                {
                    ServerProcess.StandardInput.WriteLine(cmd);
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Command sent: /{cmd}");
                }
                else
                {
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Cannot send command - Input not avaible or server isnt running.");
                }
            }
            catch (IOException ex)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Error sending command - IO STREAM PROBABLY CLOSED: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Error sending command - INVALID OPERATION (SERVER EXITED UNEXPECTEDLY) {ex.Message}");
            }
            catch (Exception ex)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Error sending command: {ex.Message}");
            }
        }

        //Send stop command if running - after that dispose of resources safely
        public async Task StopServer()
        {
            State = 1;
            if (!IsRunning)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server already not running!");
                State = 0;
                return;
            }
            UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] STOPPING SERVER...");
            SendCommand("stop");
            int timeout = 10000;
            if (ServerProcess.WaitForExit(timeout))
            {
                State = 0;
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] SERVER STOPPED.");
            }
            else
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] SERVER TIMEOUT OF {timeout} - ATTEMPTING KILL...");
                try
                {
                    ServerProcess.Kill();
                    State = 0;
                }
                catch (InvalidOperationException ex)
                {
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server Exited Unexpectedly (likely before kill but after timeout) {ex.Message}");
                    State = 0;
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] ERROR KILLING SERVER PROCESS: {ex.Message}");
                    State = 1;
                }

                DisposeProcess();
            }

        }
        //Dispose of the process safely (prepare for next use)
        private void DisposeProcess()
        {
            if (ServerProcess != null)
            {
                //Get rid of reading
                try { ServerProcess.CancelOutputRead(); } catch { }
                try { ServerProcess.CancelErrorRead(); } catch { }

                //Remove handlers
                ServerProcess.OutputDataReceived -= ConsoleOutputReceived;
                ServerProcess.ErrorDataReceived -= ConsoleOutputReceived;
                ServerProcess.Exited -= ConsoleExited;
                //If process has not finished, attempt kill.
                if (!ServerProcess.HasExited)
                {
                    try
                    {
                        ServerProcess.Kill();
                    }
                    catch (InvalidOperationException ex)
                    {
                        UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] CANNOT DISPOSE OF PROCESS - KILL FAILED (Process could have already exited: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] CANNOT DISPOSE OF PROCESS - KILL FAILED: {ex.Message}");
                    }
                }
                //Try auto dispose
                try
                {
                    ServerProcess.Dispose();
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server Process object disposed.");
                }
                catch (Exception ex)
                {
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Error disposing Server Process object: {ex.Message}");
                }
                ServerProcess = null;
            }
        }

        //Request close - wait 10 seconds, if still running fail the restart. Start again on succesful close
        public async Task RestartServer()
        {
            if (IsRunning)
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] ATTEMPTING RESTART");
                await StopServer();
                await Task.Delay(10000);
                if (!IsRunning)
                {
                    StartServer();
                }
                else
                {
                    UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server Couldnt stop - cannot restart.");
                }
            }
            else
            {
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server not running - starting instead...");
                StartServer();
            }
        }

        //Check if EULA file exists - and if it does if its true
        public bool EulaCheck()
        {
            if (File.Exists(EulaFile))
            {
                string a = File.ReadAllText(EulaFile);
                if (a.Contains("eula=true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //Disposes of any unhandled processes and releases resources on demand (finalizer for closing consoles and ending its use)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (WasDisposed) { return; }
            if (disposing && ServerProcess != null)
            {
                DisposeProcess();
            }
            WasDisposed = true;
        }

        //Calls every time console outputs data
        public event EventHandler<string>? ConsoleOutputHandler;

        private void ConsoleOutputReceived(object? sender, DataReceivedEventArgs a)
        {
            if (a.Data != null)
            {
                ConsoleOutput.Add(a.Data);
                ConsoleOutputHandler?.Invoke(this, a.Data);
            }
        }

        //Calls when server closes (any reason to process end)
        public event EventHandler<int>? ConsoleExitHandler;
        private void ConsoleExited(object? sender, EventArgs a)
        {
            if (ServerProcess != null)
            {
                State = 0;
                var exitcode = ServerProcess.ExitCode;
                UILogger.LogUI($"[CONSOLE WRAPPER - {WorkingInstance.Name}] Server Exited with code: {exitcode}");

                DisposeProcess();

                ConsoleExitHandler?.Invoke(this, exitcode);
            }
        }

        public event EventHandler<int>? ServerStateHandler;

        private void ServerStateChanged(int state)
        {
            //0-stopped
            //1-working
            //2-running
            ServerStateHandler?.Invoke(this, state);
        }
    }
}
