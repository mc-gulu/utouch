﻿using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ulib;

namespace udesign
{
    public class LuaRuntime
    {
        public static LuaRuntime Instance;

        public bool Init()
        {
            Script script = new Script();

            String bootstrapFilename = Properties.Settings.Default.LuaBootstrap;
            try
            {
                DynValue val = script.DoFile(bootstrapFilename);
                if (!val.IsNil())
                {
                    Session.Log("Lua 引导脚本执行异常（返回非 nil 值）.");
                    return false;
                }
            }
            catch (MoonSharp.Interpreter.InterpreterException e)
            {
                Session.LogException(e, e.DecoratedMessage);
                return false;
            }

            m_bootstrap = script;

            return true;
        }

        public Script BootstrapScript { get { return m_bootstrap; } }

        private Script m_bootstrap;
    }
}
