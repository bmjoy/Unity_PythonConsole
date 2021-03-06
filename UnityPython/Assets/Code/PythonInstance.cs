﻿using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

/// source: https://stackoverflow.com/questions/14139766/run-a-particular-python-function-in-c-sharp-with-ironpython

public class PythonInstance {
    private ScriptEngine engine;
    private ScriptScope scope;
    private ScriptSource source;
    private CompiledCode compiled;
    private object pythonClass;

    public PythonInstance(string code, string className = "PyClass") {
        //creating engine and stuff
        engine = Python.CreateEngine();
        
        engine.Runtime.LoadAssembly(typeof(UnityEngine.GameObject).Assembly);
        scope = engine.CreateScope();

        //loading and compiling code
        source = engine.CreateScriptSourceFromString(code, Microsoft.Scripting.SourceCodeKind.Statements);
        compiled = source.Compile();

        //now executing this code (the code should contain a class)
        compiled.Execute(scope);

        //now creating an object that could be used to access the stuff inside a python script
        pythonClass = engine.Operations.Invoke(scope.GetVariable(className));
    }

    public void SetVariable(string variable, object value) {
        scope.SetVariable(variable, value);
    }

    public dynamic GetVariable(string variable) {
        return scope.GetVariable(variable);
    }

    public void CallMethod(string method, params dynamic[] arguments) {
        engine.Operations.InvokeMember(pythonClass, method, arguments);
    }

    public object CallFunction(string method, params dynamic[] arguments) {
        return engine.Operations.InvokeMember(pythonClass, method, arguments);
    }

}