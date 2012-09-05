using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Tools;

namespace JavaScriptBuildTask
{
    public class JS : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// Please note that the account that instantiates the Impersonator class needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        
        /// <summary>
        /// Must be set to true, if you want to actually do the impersonation
        /// </summary>
        public bool Impersonate { get; set; }

        public JS()
        {
            
            AllowClr = true;
            Debug = true;
            DisableSecurity = true;
        }
        Jint.JintEngine je = new Jint.JintEngine();
        public bool Execute()
        {
            Tools.Impersonator imp = null;
            try
            {
                if (this.Impersonate && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)) imp = new Impersonator(Username, Domain, Password);

                List<string> results = new List<string>();
                try
                {
                    if (Sources != null && Sources.Length > 0)
                    {
                        foreach (var item in Sources)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                if (System.IO.File.Exists(item))
                                {
                                    string script = System.IO.File.ReadAllText(item);
                                    results.Add(Execute(script));
                                }
                                else
                                {
                                    results.Add("");
                                }
                            }
                            else
                            {
                                results.Add("");
                            }

                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.Code))
                            results.Add(Execute(this.Code));
                        else
                            results.Add("");
                    }

                }
                catch (Exception e)
                {
                    results.Add(e.ToString());
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                    return false;
                }

                this.CompilerResults = results.ToArray();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (this.Impersonate && imp != null) imp.Dispose();

            }

        }

        private bool initDone = false;

        private string Execute(string Input)
        {
            if (!initDone)
            {
                je.AllowClr = this.AllowClr;
                je.SetDebugMode(this.Debug);
                if (DisableSecurity) je.DisableSecurity();

                je.SetFunction("print", new Action<object>(Console.WriteLine));
                je.SetParameter("BuildEngine", BuildEngine);
                je.SetParameter("HostObject", BuildEngine);
                je.SetParameter("Task", this);
                initDone = true;
            }
            var result = je.Run(Input);

            if (result == null)
                return "";
            else
                return result.ToString();
        }

        [Required]        
        public string Code { get; set; }

        [Output]
        public string[] CompilerResults { get; set; }

        public string[] Sources { get; set; }
        public bool Debug { get; set; }
        public bool AllowClr { get; set; }
        public bool DisableSecurity { get; set; }

    }
}
