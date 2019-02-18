using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Emse.Updater.Windows.Service
{
    public class LoggerAdapter
    {
        #region singleton
        private static LoggerAdapter instance = null;
        private static readonly object padlock = new object();
        private static NLog.Logger logger = null;

        LoggerAdapter()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static LoggerAdapter Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new LoggerAdapter();
                    }
                    return instance;
                }
            }
        }


        #endregion


        public void Warn(string v)
        {
            logger.Warn(v);
        }

        public string Debug(string v)
        {
            logger.Debug(v);
            return v;
        }

        public List<string> Fatal(Exception ex, string ctx = null)
        {
            string msg = "";

            if (ctx != null)
                msg = ctx + "=>";

            var res = GetErrorsFromException(ex);
            msg = res.Item2;
            logger.Fatal(ex, msg);

            return res.Item1;
        }

        public void Error(Exception ex, string ctx, string tag, string msg = null)
        {
            var res = GetErrorsFromException(ex);

            LogEventInfo li = new LogEventInfo(LogLevel.Error, "LoggerAdapter", res.Item2);
            li.Exception = ex;

            if (tag != null)
                li.Properties.Add("tag", tag);
            if (ctx != null)
                li.Properties.Add("ctx", ctx);

            if (!string.IsNullOrEmpty(msg))
                li.Message = msg + Environment.NewLine;

            logger.Log(li);            
        }

        private static Tuple<List<string>, string> GetErrorsFromException(Exception ex, List<string> errs = null, string errMsg = null)
        {
            if (errMsg == null)
                errMsg = "";
            if (errs == null)
                errs = new List<string>();

            if (ex is AggregateException)
            {
                foreach (Exception inex in (ex as AggregateException).InnerExceptions)
                {
                    string taskState = "";
                    if (inex.Data.Contains("state") && inex.Data["state"] != null)
                        taskState = inex.Data["state"].ToString() + ":";

                    errs.Add(taskState + inex.Message);
                    if (errMsg != "")
                        errMsg += Environment.NewLine;
                    errMsg += taskState + inex.Message;
                    if (ex.StackTrace != null)
                        errMsg += "stackTrace:" + ex.StackTrace.ToString();


                    if (inex.InnerException != null)
                    {
                        errs.Add("--------inner error---------");
                        GetErrorsFromException(inex.InnerException, errs, errMsg);
                    }
                }
            }
            else
            {
                string taskState = "";
                if (ex.Data.Contains("state") && ex.Data["state"] != null)
                    taskState = ex.Data["state"].ToString() + ":";

                errs.Add(taskState + ex.Message);
                errMsg += taskState + ex.Message;
                if (ex.StackTrace != null)
                    errMsg += "stackTrace:" + ex.StackTrace.ToString();

                if (ex.InnerException != null)
                {
                    errs.Add("--------inner error---------");
                    errMsg += Environment.NewLine + "--------inner error---------" + Environment.NewLine;
                    GetErrorsFromException(ex.InnerException, errs, errMsg);
                }
            }

            return new Tuple<List<string>, string>(errs, errMsg);
        }

        public void Error(List<string> errors)
        {
            string msg = "";
            foreach (var err in errors)
            {
                if (msg != "")
                    msg += Environment.NewLine;
                msg += err;
            }

            logger.Error(msg);
        }
    }
}
